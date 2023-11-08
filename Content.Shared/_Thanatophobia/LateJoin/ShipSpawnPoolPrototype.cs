using Robust.Shared.Prototypes;

namespace Content.Shared.Thanatophobia.LateJoin;

[Prototype("shipSpawnPool")]
public sealed class ShipSpawnPoolPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("ships")]
    public Dictionary<string, string> Ships = new();
}
