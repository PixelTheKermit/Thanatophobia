using Content.Shared.Thanatophobia.LateJoin;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.Thanatophobia.LateJoin;

/// <summary>
/// If this rule appears, players will only be able to spawn in via purchasing a ship in the lobby.
/// </summary>
[RegisterComponent]
public sealed partial class LobbyShipSpawnerRuleComponent : Component
{
    [DataField(required: true)]
    public ProtoId<ShipSpawnPoolPrototype> ShipPool = default!;

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
    public string MapID;
    public bool Private = true;

    public ShipLobby(NetUserId owner, string mapID)
    {
        Owner = owner;
        MapID = mapID;
    }

}
