using System.Numerics;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Pulling;
using Content.Shared.Pulling.Components;
using Robust.Shared.Network;

namespace Content.Shared.Thanatophobia.Traits;

public sealed partial class SharedWingsTraitSystem : EntitySystem
{
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearanceSystem = default!;
    [Dependency] private readonly SharedPullingStateManagementSystem _pullingSystem = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speedModifierSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WingsTraitComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, WingsTraitComponent comp, ComponentStartup args)
    {
        if (TryComp<MovementSpeedModifierComponent>(uid, out var speedComp))
            _speedModifierSystem.ChangeWeightlessSpeed(uid, comp.WeightlessAcceleration, comp.WeightlessFriction, comp.WeightlessModifier, speedComp);
    }
}
