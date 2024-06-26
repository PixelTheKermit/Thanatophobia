using Content.Shared.Tag;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Utility;

namespace Content.Shared.Traits;
public sealed partial class TraitAddComponentsFunction : BaseTraitFunction
{

    [DataField(required: true)]
    public ComponentRegistry Components { get; private set; } = default!;

    public override void AddTrait(EntityUid uid, TraitPrototype traitProto, IPrototypeManager protoManager, IEntityManager entityManager)
    {
        var serializationManager = IoCManager.Resolve<ISerializationManager>();

        foreach (var entry in Components.Values)
        {
            var comp = (Component) serializationManager.CreateCopy(entry.Component, notNullableOverride: true);
            comp.Owner = uid;
            entityManager.AddComponent(uid, comp, true);
        }
    }
}
