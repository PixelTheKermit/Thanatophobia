using Content.Shared.Tag;
using Content.Shared.Traits;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Humanoid.Markings;
public sealed partial class TraitAddTagsFunction : BaseTraitFunction
{
    [DataField]
    public List<ProtoId<TagPrototype>> Add;

    [DataField]
    public List<ProtoId<TagPrototype>> Remove;

    public override void AddTrait(EntityUid uid, TraitPrototype traitProto, IPrototypeManager protoManager, IEntityManager entityManager)
    {
        var tagSystem = IoCManager.Resolve<TagSystem>();

        foreach (var tag in Add)
        {
            tagSystem.AddTag(uid, tag);
        }

        foreach (var tag in Remove)
        {
            tagSystem.RemoveTag(uid, tag);
        }
    }
}
