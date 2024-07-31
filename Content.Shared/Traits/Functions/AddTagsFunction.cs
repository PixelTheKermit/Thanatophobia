using Content.Shared.Tag;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Traits;
public sealed partial class TraitAddTagsFunction : BaseTraitFunction
{
    [DataField]
    public List<ProtoId<TagPrototype>> Add = new();

    [DataField]
    public List<ProtoId<TagPrototype>> Remove = new();

    public override void AddTrait(EntityUid uid, TraitPrototype traitProto, IPrototypeManager protoManager, IEntityManager entityManager)
    {
        var sysMan = IoCManager.Resolve<IEntitySystemManager>();
        var tagSystem = sysMan.GetEntitySystem<TagSystem>();

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
