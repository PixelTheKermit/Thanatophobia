using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared.Traits;
public sealed partial class TraitWeightlessModifierFunction : BaseTraitFunction
{
    [DataField]
    public float WeightlessAcceleration = 1f;

    [DataField]
    public float WeightlessFriction = 1f;

    [DataField]
    public float WeightlessModifier = 1f;

    public override void AddTrait(EntityUid uid, TraitPrototype traitProto, IPrototypeManager protoManager, IEntityManager entityManager)
    {
        var sysManager = IoCManager.Resolve<IEntitySystemManager>();
        var speedModifierSystem = sysManager.GetEntitySystem<MovementSpeedModifierSystem>();

        if (entityManager.TryGetComponent<MovementSpeedModifierComponent>(uid, out var speedComp))
            speedModifierSystem.ChangeWeightlessSpeed
            (
                uid,
                speedComp.WeightlessAcceleration * WeightlessAcceleration,
                speedComp.WeightlessFriction * WeightlessFriction,
                speedComp.WeightlessModifier * WeightlessModifier,
                speedComp
            );
    }
}
