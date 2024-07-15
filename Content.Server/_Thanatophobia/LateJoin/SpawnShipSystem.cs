using System.Linq;
using System.Numerics;
using Content.Server.GameTicking;
using Content.Server.Maps;
using Content.Server.Station.Systems;
using Content.Server.Worldgen;
using Content.Server.Worldgen.Components.Debris;
using Content.Server.Worldgen.Systems;
using Content.Server.Worldgen.Tools;
using Content.Shared.Roles;
using Content.Shared.Thanatophobia.CCVar;
using Content.Shared.Thanatophobia.LateJoin;
using Robust.Server.Maps;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Thanatophobia.LateJoin;
public sealed partial class SpawnShipSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly WorldControllerSystem _worldControlSystem = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly IConfigurationManager _cfgManager = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<RoundStartShipTryStartLobbyUIMessage>(GetShipSpawnMessage);
        SubscribeNetworkEvent<RoundStartShipTryCreateLobbyUIMessage>(CreateLobbyMessage);
        SubscribeNetworkEvent<RoundStartGetShipLobbiesUIMessage>(ListLobbiesMessage);
        SubscribeNetworkEvent<RoundStartShipTryJoinLobbyUIMessage>(JoinLobbyMessage);
        SubscribeNetworkEvent<RoundStartShipTryLeaveLobbyUIMessage>(LeaveLobbyMessage);
        SubscribeNetworkEvent<RoundStartShipTryKickPlayerUIMessage>(TryKickMessage);
        SubscribeNetworkEvent<RoundStartShipTryChangeLobbyInfoUIMessage>(ChangeInfoMessage);
    }

    public override void Update(float frameTime) // This is going to copy Afterlight ngl bc I can't figure out how to spawn ships without grid fuckery or possibly a bunch of lag.
    {
        base.Update(frameTime);

        var spawnerComps = EntityQueryEnumerator<WorldShipSpawnerComponent>();

        while (spawnerComps.MoveNext(out var uid, out var spawnerComp))
        {
            if (spawnerComp.NeedsSetup)
            {
                spawnerComp.NeedsSetup = false;

                for (var x = -spawnerComp.SpawnArea; x < spawnerComp.SpawnArea; x++)
                {
                    for (var y = -spawnerComp.SpawnArea; y < spawnerComp.SpawnArea; y++)
                    {
                        var cCoords = new Vector2i(x, y);
                        var chunk = _worldControlSystem.GetOrCreateChunk(cCoords, uid);

                        if (!TryComp<DebrisFeaturePlacerControllerComponent>(chunk, out var debrisComp) || !TryComp<MapComponent>(uid, out var mapComp))
                            continue;

                        if (debrisComp.OwnedDebris.Count != 0)
                            continue;

                        spawnerComp.FreeCoordinates.Add(new MapCoordinates(WorldGen.ChunkToWorldCoordsCentered(cCoords), mapComp.MapId));
                    }
                }
            }
        }
    }

    private void CreateLobbyMessage(RoundStartShipTryCreateLobbyUIMessage msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession is not { } _)
            return;

        if (args.SenderSession.AttachedEntity != null)
            return;

        var maps = EntityQuery<WorldShipSpawnerComponent>().ToList();

        if (maps.Count <= 0)
            return;

        var map = maps[0];

        if (map.PlayersToCode.Any(x => x.Key == args.SenderSession.UserId) && map.Lobbies.Any(x => x.Key == map.PlayersToCode[args.SenderSession.UserId]))
            return;

        var poolStr = _cfgManager.GetCVar(TPCCVars.ShipSpawnPool);

        if (!_protoManager.TryIndex<ShipSpawnPoolPrototype>(poolStr, out var poolProto))
            return;

        // Oh lord this looks painful.
        // Okay so: We can't have the same code generate for two lobbies.
        // But we also can't try generating the code forever, unless we want big lag.
        // Solution? We try 10 times then give up.
        // I am such a good coder - Pixel

        for (var i = 0; i < 10; i++)
        {
            var code = "";
            for (var ii = 0; ii < 8; ii++)
            {
                var charOrd = 0;
                if (_random.NextFloat() <= .5f)
                {
                    charOrd = _random.Next(48, 55);
                }
                else
                {
                    charOrd = _random.Next(65, 88);

                    if (charOrd >= 71) // To hopefully prevent generating words that could be offensive.
                        charOrd++;
                    if (charOrd >= 78)
                        charOrd++;
                }

                var character = (char) charOrd;
                code += character;
            }

            if (!map.Lobbies.Any(x => x.Key == code))
            {
                map.Lobbies[code] = new ShipLobby(args.SenderSession.UserId, poolProto.Ships.Keys.ToList()[0]);
                map.PlayersToCode[args.SenderSession.UserId] = code;
                map.CodeToPlayers[code] = new() { args.SenderSession.UserId };
                return;
            }
        }
    }

    private void ListLobbiesMessage(RoundStartGetShipLobbiesUIMessage _, EntitySessionEventArgs args)
    {
        if (args.SenderSession is not { } _)
            return;

        if (args.SenderSession.AttachedEntity != null)
            return;

        UpdateLobbies(args.SenderSession);
    }

    private void TryKickMessage(RoundStartShipTryKickPlayerUIMessage ev, EntitySessionEventArgs args)
    {
        if (ev.PlayerIndex <= 0)
            return;

        if (args.SenderSession is not { } _)
            return;

        if (args.SenderSession.AttachedEntity != null)
            return;

        var maps = EntityQuery<WorldShipSpawnerComponent>().ToList();

        if (maps.Count <= 0)
            return;

        var map = maps[0];

        if (!map.PlayersToCode.Any(x => x.Key == args.SenderSession.UserId))
            return;

        var lobbyCode = map.PlayersToCode[args.SenderSession.UserId];

        if (!map.Lobbies.Any(x => x.Key == lobbyCode))
            return;

        var lobby = map.Lobbies[lobbyCode];

        if (lobby.Owner != args.SenderSession.UserId)
            return;

        var players = map.CodeToPlayers[lobbyCode];

        if (ev.PlayerIndex >= players.Count)
            return;

        LeaveLobby(players[ev.PlayerIndex], map);
    }

    /// <summary>
    /// Sends an update to the client.
    /// </summary>
    /// <param name="senderSession"></param>
    /// <param name="map"></param>
    private void UpdateLobbies(ICommonSession senderSession, WorldShipSpawnerComponent? map = null)
    {
        if (map == null)
        {
            var maps = EntityQuery<WorldShipSpawnerComponent>().ToList();

            if (maps.Count <= 0)
                return;

            map = maps[0];
        }

        var poolStr = _cfgManager.GetCVar(TPCCVars.ShipSpawnPool);

        var msg = new RoundStartShipListLobbiesUIMessage();

        if (!_protoManager.TryIndex<ShipSpawnPoolPrototype>(poolStr, out var poolProto))
            return;

        msg.IsPlayerInLobby = map.PlayersToCode.Any(x => x.Key == senderSession.UserId) && map.Lobbies.Any(x => x.Key == map.PlayersToCode[senderSession.UserId]);

        if (msg.IsPlayerInLobby)
        {
            var lobbyCode = map.PlayersToCode[senderSession.UserId];
            var lobby = map.Lobbies[lobbyCode];
            msg.IsLobbyOwner = lobby.Owner == senderSession.UserId;
            msg.LobbyPlayerCount = map.CodeToPlayers[lobbyCode].Count;
            msg.LobbyCode = lobbyCode;
            msg.IsHidden = lobby.Private;
            msg.CurrentShip = lobby.MapID.Id;

            foreach (var player in map.CodeToPlayers[lobbyCode])
            {
                if (_playerManager.TryGetPlayerData(player, out var playerData))
                    msg.PlayerNames.Add(playerData.UserName);
                else
                    msg.PlayerNames.Add("Unknown");
            }

            if (poolProto.Ships.Any(x => x.Key == lobby.MapID))
            {
                msg.LobbyPlayerMax = poolProto.Ships[lobby.MapID].MaxPlayers;
            }
        }
        else
        {
            foreach (var lobby in map.Lobbies)
            {
                if (!_playerManager.TryGetPlayerData(lobby.Value.Owner, out var plrData) || lobby.Value.Private)
                    continue;

                msg.Lobbies.Add(new()
                {
                    Code = lobby.Key,
                    Max = poolProto.Ships[lobby.Value.MapID].MaxPlayers,
                    Players = map.CodeToPlayers[lobby.Key].Count,
                    OwnerName = plrData.UserName,
                    ShuttleName = Loc.GetString($"tp-ui-ship-name-{lobby.Value.MapID}")
                });
            }
        }
        RaiseNetworkEvent(msg, senderSession);
    }

    private void JoinLobbyMessage(RoundStartShipTryJoinLobbyUIMessage ev, EntitySessionEventArgs args)
    {
        if (args.SenderSession is not { } _)
            return;

        if (args.SenderSession.AttachedEntity != null)
            return;

        var maps = EntityQuery<WorldShipSpawnerComponent>().ToList();

        if (maps.Count <= 0)
            return;

        var map = maps[0];

        if (map.PlayersToCode.Any(x => x.Key == args.SenderSession.UserId) && map.Lobbies.Any(x => x.Key == map.PlayersToCode[args.SenderSession.UserId]))
            return;

        if (!map.Lobbies.Any(x => x.Key == ev.LobbyCode))
            return;

        if (!map.CodeToPlayers.Any(x => x.Key == ev.LobbyCode))
            return;

        var poolStr = _cfgManager.GetCVar(TPCCVars.ShipSpawnPool);

        if (!_protoManager.TryIndex<ShipSpawnPoolPrototype>(poolStr, out var poolProto))
            return;

        if (!poolProto.Ships.Any(x => x.Key == map.Lobbies[ev.LobbyCode].MapID))
            return;

        if (poolProto.Ships[map.Lobbies[ev.LobbyCode].MapID].MaxPlayers <= map.CodeToPlayers[ev.LobbyCode].Count)
            return;

        map.PlayersToCode[args.SenderSession.UserId] = ev.LobbyCode;
        map.CodeToPlayers[ev.LobbyCode].Add(args.SenderSession.UserId);

        foreach (var player in map.CodeToPlayers[ev.LobbyCode])
        {
            if (_playerManager.TryGetSessionById(player, out var playerSession))
                UpdateLobbies(playerSession, map);
        }
    }

    private void ChangeInfoMessage(RoundStartShipTryChangeLobbyInfoUIMessage ev, EntitySessionEventArgs args)
    {
        if (args.SenderSession is not { } _)
            return;

        if (args.SenderSession.AttachedEntity != null)
            return;

        var maps = EntityQuery<WorldShipSpawnerComponent>().ToList();

        if (maps.Count <= 0)
            return;

        var map = maps[0];

        if (!map.PlayersToCode.Any(x => x.Key == args.SenderSession.UserId))
            return;

        var lobbyCode = map.PlayersToCode[args.SenderSession.UserId];

        if (!map.Lobbies.Any(x => x.Key == lobbyCode))
            return;

        var lobby = map.Lobbies[lobbyCode];

        if (lobby.Owner != args.SenderSession.UserId)
            return;

        lobby.Private = ev.Private;

        var poolStr = _cfgManager.GetCVar(TPCCVars.ShipSpawnPool);

        if (!_protoManager.TryIndex<ShipSpawnPoolPrototype>(poolStr, out var poolProto))
            return;

        if (!poolProto.Ships.Any(x => x.Key == ev.Ship))
            return;

        lobby.MapID = ev.Ship;

        foreach (var player in map.CodeToPlayers[lobbyCode])
        {
            if (_playerManager.TryGetSessionById(player, out var playerSession))
                UpdateLobbies(playerSession, map);
        }
    }

    private void LeaveLobbyMessage(RoundStartShipTryLeaveLobbyUIMessage _, EntitySessionEventArgs args)
    {
        if (args.SenderSession is not { } _)
            return;

        if (args.SenderSession.AttachedEntity != null)
            return;

        LeaveLobby(args.SenderSession.UserId);
    }

    /// <summary>
    /// Used to kick a player out of a lobby.
    /// </summary>
    /// <param name="senderUserId"></param>
    /// <param name="map"></param>
    private void LeaveLobby(NetUserId senderUserId, WorldShipSpawnerComponent? map = null)
    {
        if (map == null)
        {
            var maps = EntityQuery<WorldShipSpawnerComponent>().ToList();

            if (maps.Count <= 0)
                return;

            map = maps[0];
        }

        if (!map.PlayersToCode.Any(x => x.Key == senderUserId))
            return;

        var lobbyCode = map.PlayersToCode[senderUserId];

        if (!map.Lobbies.Any(x => x.Key == lobbyCode))
            return;

        var lobby = map.Lobbies[lobbyCode];

        if (lobby.Owner == senderUserId)
        {
            map.Lobbies.Remove(lobbyCode);
            foreach (var player in map.CodeToPlayers[lobbyCode])
            {
                if (map.PlayersToCode.Any(x => x.Key == player))
                {
                    map.PlayersToCode.Remove(player);
                    if (_playerManager.TryGetSessionById(player, out var playerSession))
                        UpdateLobbies(playerSession, map);
                }
            }

            map.CodeToPlayers.Remove(lobbyCode);
        }
        else
        {
            if (map.CodeToPlayers[lobbyCode].Any(x => x == senderUserId))
                map.CodeToPlayers[lobbyCode].Remove(senderUserId);

            map.PlayersToCode.Remove(senderUserId);

            if (_playerManager.TryGetSessionById(senderUserId, out var mainPlayerSession))
                UpdateLobbies(mainPlayerSession, map);

            foreach (var player in map.CodeToPlayers[lobbyCode])
            {
                if (_playerManager.TryGetSessionById(player, out var playerSession))
                    UpdateLobbies(playerSession, map);
            }
        }
    }

    private void GetShipSpawnMessage(RoundStartShipTryStartLobbyUIMessage _, EntitySessionEventArgs args)
    {
        if (args.SenderSession is not { } _)
            return;

        if (args.SenderSession.AttachedEntity != null)
            return;

        var maps = EntityQuery<WorldShipSpawnerComponent>().ToList();

        if (maps.Count <= 0)
            return;

        var map = maps[0];

        if (!map.PlayersToCode.Any(x => x.Key == args.SenderSession.UserId))
            return;

        var lobbyCode = map.PlayersToCode[args.SenderSession.UserId];

        if (!map.Lobbies.Any(x => x.Key == lobbyCode))
            return;

        var lobby = map.Lobbies[lobbyCode];

        if (lobby.Owner != args.SenderSession.UserId)
            return;

        var poolStr = _cfgManager.GetCVar(TPCCVars.ShipSpawnPool);

        if (!_protoManager.TryIndex<ShipSpawnPoolPrototype>(poolStr, out var poolProto))
            return;

        if (!poolProto.Ships.Any(x => x.Key == lobby.MapID) ||
            !_protoManager.TryIndex(lobby.MapID, out var gameMap))
            return;

        var safetyBounds = Box2.UnitCentered.Enlarged(48);

        _random.Shuffle(map.FreeCoordinates); // Shuffle the available coordinates, to make it appear more random.

        foreach (var coords in map.FreeCoordinates)
        {
            var collidingGrids = new List<Entity<MapGridComponent>>();
            _mapManager.FindGridsIntersecting(coords.MapId, safetyBounds.Translated(coords.Position), ref collidingGrids);

            if (collidingGrids.Count > 0)
                continue;

            MapLoadOptions loadOptions = new()
            {
                Offset = coords.Position,
                Rotation = _random.NextAngle(),
                LoadMap = false,
            };

            var grids = _gameTicker.LoadGameMap(gameMap, coords.MapId, loadOptions);
            Log.Warning($"{args.SenderSession} spawned in {lobby.MapID} and loaded {grids.Count} grids.");

            foreach (var player in map.CodeToPlayers[lobbyCode])
            {
                if (player == args.SenderSession.UserId && poolProto.Ships[lobby.MapID].CaptainRole != null)
                    _gameTicker.MakeJoinGame(args.SenderSession, (EntityUid) _stationSystem.GetOwningStation(grids[0])!, poolProto.Ships[lobby.MapID].CaptainRole);
                else if (_playerManager.TryGetSessionById(player, out var playerSession))
                    _gameTicker.MakeJoinGame(playerSession, (EntityUid) _stationSystem.GetOwningStation(grids[0])!, poolProto.Ships[lobby.MapID].CrewRole);
            }
            LeaveLobby(args.SenderSession.UserId, map);

            return; // Doesn't need to do anything else now that the map is spawned.
        }
    }
};
