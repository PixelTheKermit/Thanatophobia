using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Traits;
using Robust.Shared.Prototypes;

namespace Content.Shared.Humanoid.Markings;
public sealed partial class TraitGiveGearFunction : BaseTraitFunction
{
    [DataField(required: true)]
    public string ItemId = default!;
    public override void AddTrait(EntityUid uid, TraitPrototype traitProto, IPrototypeManager protoManager, IEntityManager entityManager)
    {
        var sysManager = IoCManager.Resolve<IEntitySystemManager>();
        var handsSystem = sysManager.GetEntitySystem<SharedHandsSystem>();

        if (!entityManager.TryGetComponent(uid, out HandsComponent? handsComponent))
            return;

        var coords = entityManager.GetComponent<TransformComponent>(uid).Coordinates;
        var inhandEntity = entityManager.SpawnEntity(ItemId, coords);
        handsSystem.TryPickup(uid, inhandEntity, handsComp: handsComponent);
    }
}
