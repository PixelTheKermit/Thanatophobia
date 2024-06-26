using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Humanoid.Markings;

/// <summary>
/// This will put a marking into a list, where the trait that is specified here will then use to replace it's current marking.
/// </summary>

[NetSerializable, Serializable]
public sealed partial class ReplaceTraitMarking : BaseMarkingFunction
{
    [DataField(required: true)]
    public string TraitId = "uwu";

    [DataField(required: true)]
    public ProtoId<MarkingPrototype> MarkingId = default!;

    [DataField(required: true)]
    public int ColourCount = default!;

    public override void AddMarking(EntityUid uid, Marking markingObject, IEnumerable<BaseContainer> bodyPartContainers, IPrototypeManager protoManager, IEntityManager entityManager)
    {
        if (!entityManager.TryGetComponent<HumanoidAppearanceComponent>(uid, out var humanoidComp))
            return;

        humanoidComp.TraitMarkings[TraitId] = new Marking(MarkingId, markingObject.MarkingColors);
    }

    public override int GetSpriteCount()
    {
        return ColourCount;
    }

    public override Dictionary<string, List<SpriteSpecifier?>> GetSprites()
    {
        return new();
    }
}
