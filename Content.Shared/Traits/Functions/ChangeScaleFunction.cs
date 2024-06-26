using System.Numerics;
using Robust.Shared.Prototypes;

namespace Content.Shared.Traits;
public sealed partial class TraitChangeScaleFunction : BaseTraitFunction
{
    [DataField(required: true)]
    public float Height;
    public override void AddTrait(EntityUid uid, TraitPrototype traitProto, IPrototypeManager protoManager, IEntityManager entityManager)
    {
        var sysManager = IoCManager.Resolve<IEntitySystemManager>();
        var appearanceSystem = sysManager.GetEntitySystem<SharedAppearanceSystem>();

        entityManager.EnsureComponent<ScaleVisualsComponent>(uid);

        if (!appearanceSystem.TryGetData(uid, ScaleVisuals.Scale, out Vector2 oldScale))
            oldScale = Vector2.One;

        appearanceSystem.SetData(uid, ScaleVisuals.Scale, oldScale * Height);
    }
}
