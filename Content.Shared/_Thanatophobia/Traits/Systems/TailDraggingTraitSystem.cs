using System.Numerics;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Pulling;
using Content.Shared.Pulling.Components;
using Robust.Shared.Network;

namespace Content.Shared.Thanatophobia.Traits;

public sealed partial class SharedTailDraggingTraitSystem : EntitySystem
{
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearanceSystem = default!;
    [Dependency] private readonly SharedPullingStateManagementSystem _pullingSystem = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly MarkingManager _markingManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TailDraggingTraitComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, TailDraggingTraitComponent comp, ComponentStartup args)
    {
        if (TryComp<SharedPullerComponent>(uid, out var pullerComp))
            _pullingSystem.ChangeHandRequirement(uid, false, pullerComp);
    }
}
