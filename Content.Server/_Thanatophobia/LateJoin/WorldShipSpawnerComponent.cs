using Content.Server.Maps;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.Thanatophobia.LateJoin;

[RegisterComponent]
public sealed partial class WorldShipSpawnerComponent : Component
{
    [DataField]
    public bool NeedsSetup = true;

    [DataField]
    public int SpawnArea = 16;

    [DataField]
    public List<MapCoordinates> FreeCoordinates = new();

    // We want quick accessing of these lists. I kinda wish there was a better solution...
    // HONESTLY THERE PROBABLY IS! I JUST LACK THE NESSESSARY KNOWLEDGE!!!
    public Dictionary<string, ShipLobby> Lobbies = new();
    public Dictionary<NetUserId, string> PlayersToCode = new();
    public Dictionary<string, List<NetUserId>> CodeToPlayers = new();
}

[DataDefinition]
public sealed partial class ShipLobby
{
    public NetUserId Owner;
    public ProtoId<GameMapPrototype> MapID;
    public bool Private = true;

    public ShipLobby(NetUserId owner, ProtoId<GameMapPrototype> mapID)
    {
        Owner = owner;
        MapID = mapID;
    }

}
