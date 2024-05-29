using System.Linq;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Robust.Shared.Prototypes;

namespace Content.Shared.Traits;
public sealed partial class TraitAddMarkingFunction : BaseTraitFunction
{
    public override bool DynamicUpdates => true;

    /// <summary>
    /// Use in case you have a trait that adds multiple markings and you do not want to have all those markings replaced by one marking.
    /// </summary>
    [DataField]
    public string? SpecialId = null;

    /// <summary>
    /// The marking prototype ID.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<MarkingPrototype> MarkingId = default!;

    /// <summary>
    /// The color of the marking. Set to white by default.
    /// </summary>
    [DataField]
    public List<Color> MarkingColours = new();

    public override void AddTrait(EntityUid uid, TraitPrototype traitProto, IPrototypeManager protoManager, IEntityManager entityManager)
    {
        var sysManager = IoCManager.Resolve<IEntitySystemManager>();
        var humanoidAppearanceSystem = sysManager.GetEntitySystem<SharedHumanoidAppearanceSystem>();

        if (!entityManager.TryGetComponent<HumanoidAppearanceComponent>(uid, out var appearanceComp) ||
            !protoManager.TryIndex(MarkingId, out var markingProto))
            return;

        var traitId = SpecialId ?? traitProto.ID;

        if (appearanceComp.TraitMarkings.Any(x => x.Key == traitId))
        {
            humanoidAppearanceSystem.AddMarking(uid, appearanceComp.TraitMarkings[traitId].MarkingId, appearanceComp.TraitMarkings[traitId].MarkingColors, true, true, appearanceComp);
            appearanceComp.TraitMarkings.Remove(traitId);
        }
        else
        {
            var colors = MarkingColoring.GetMarkingLayerColors(markingProto, appearanceComp.SkinColor, appearanceComp.EyeColor, new MarkingSet());
            var dictColors = colors.ToDictionary(x => colors.IndexOf(x));

            for (var i = 0; i < MarkingColours.Count; i++)
                dictColors[i] = MarkingColours[i];

            humanoidAppearanceSystem.AddMarking(uid, MarkingId, dictColors.Values.ToList(), true, true, appearanceComp);
        }

    }
}
