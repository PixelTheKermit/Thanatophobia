
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
using FastAccessors;
using Robust.Server.Maps;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
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
    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<RoundStartTrySpawnShipUIMessage>(GetShipSpawnMessage);
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

    private void GetShipSpawnMessage(RoundStartTrySpawnShipUIMessage msg, EntitySessionEventArgs args)
    {
        var poolStr = _cfgManager.GetCVar(TPCCVars.ShipSpawnPool);

        if (!_protoManager.TryIndex<ShipSpawnPoolPrototype>(poolStr, out var poolProto))
            return;

        if (!_protoManager.TryIndex<GameMapPrototype>(msg.ShipToSpawn, out var gameMap)
        || !poolProto.Ships.TryGetValue(msg.ShipToSpawn, out var job)
        || !_protoManager.HasIndex<JobPrototype>(job))
            return;

        if (args.SenderSession.AttachedEntity != null)
            return;

        var safetyBounds = Box2.UnitCentered.Enlarged(48);

        var maps = EntityQuery<WorldShipSpawnerComponent>().ToList();
        var map = _random.Pick(maps);

        _random.Shuffle(map.FreeCoordinates); // Shuffle the available coordinates, to make it appear more random.



        foreach (var coords in map.FreeCoordinates)
        {
            if (_mapManager.FindGridsIntersecting(coords.MapId, safetyBounds.Translated(coords.Position)).Any())
                continue;

            MapLoadOptions loadOptions = new()
            {
                Offset = coords.Position,
                Rotation = _random.NextAngle(),
                LoadMap = false,
            };

            var grids = _gameTicker.LoadGameMap(gameMap, coords.MapId, loadOptions);
            Log.Warning($"{args.SenderSession} spawned in {msg.ShipToSpawn} and loaded {grids.Count} grids.");

            _gameTicker.MakeJoinGame((IPlayerSession) args.SenderSession, (EntityUid) _stationSystem.GetOwningStation(grids[0])!, job);

            return; // Doesn't need to do anything else now that the map is spawned.
        }
    }
};
