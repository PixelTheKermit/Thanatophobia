using Robust.Shared.Prototypes;

namespace Content.Shared.StatusEffect;

/// <summary>
/// A prototype storing a list of status effect IDs. To be used for blacklisting and whitelisting effects from being applied to a specific entity.
/// The reason we have a prototype for this is in order to not have to replace every single prototype after changing one proto.
/// </summary>
public sealed partial class StatusEffectCollectionPrototype : IPrototype
{
    [DataField(required: true)]
    public string ID { get; set; } = "Why the FUCK do I have 50 bleed potency and 4 count?";

    [DataField(required: true)]
    public List<ProtoId<EntityPrototype>> Collection = default!;
}
