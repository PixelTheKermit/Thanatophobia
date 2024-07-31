using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared.Thanatophobia.LateJoin;

[Prototype("shipSpawnPool")]
public sealed class ShipSpawnPoolPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("ships")]
    public Dictionary<string, ShipSpawnInfo> Ships = new();
}

[DataDefinition]
public sealed partial class ShipSpawnInfo
{
    /// <summary>
    /// The role of the captain. If none is given, uses the crew role.
    /// </summary>
    [DataField]
    public ProtoId<JobPrototype>? CaptainRole = null;

    /// <summary>
    /// The role of the general crew. This is a required field.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<JobPrototype> CrewRole = default!;

    /// <summary>
    /// The maximum amount of players that are allowed on the ship. This is a required field.
    /// </summary>
    [DataField(required: true)]
    public int MaxPlayers = default!;
}
