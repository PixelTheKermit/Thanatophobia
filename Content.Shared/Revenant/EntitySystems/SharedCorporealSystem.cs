using Content.Shared.Physics;
using Robust.Shared.Physics;
using System.Linq;
using Content.Shared.Movement.Systems;
using Content.Shared.Revenant.Components;
using Robust.Shared.Physics.Systems;
using Content.Shared.StatusEffect;

namespace Content.Shared.Revenant.EntitySystems;

/// <summary>
/// Makes the revenant solid when the component is applied.
/// Additionally applies a few visual effects.
/// Used for status effect.
/// </summary>
public abstract class SharedCorporealSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CorporealComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CorporealComponent, ComponentShutdown>(OnShutdown);
    }

    public virtual void OnStartup(EntityUid uid, CorporealComponent component, ComponentStartup args)
    {
        if (!TryComp<StatusEffectComponent>(uid, out var status) || status.Owner == null)
            return;

        _appearance.SetData(status.Owner.Value, RevenantVisuals.Corporeal, true);

        if (TryComp<FixturesComponent>(status.Owner.Value, out var fixtures) && fixtures.FixtureCount >= 1)
        {
            var fixture = fixtures.Fixtures.First();

            _physics.SetCollisionMask(status.Owner.Value, fixture.Key, fixture.Value, (int) (CollisionGroup.SmallMobMask | CollisionGroup.GhostImpassable), fixtures);
            _physics.SetCollisionLayer(status.Owner.Value, fixture.Key, fixture.Value, (int) CollisionGroup.SmallMobLayer, fixtures);
        }
    }

    public virtual void OnShutdown(EntityUid uid, CorporealComponent component, ComponentShutdown args)
    {
        if (!TryComp<StatusEffectComponent>(uid, out var status) || status.Owner == null)
            return;

        _appearance.SetData(status.Owner.Value, RevenantVisuals.Corporeal, false);

        if (TryComp<FixturesComponent>(status.Owner.Value, out var fixtures) && fixtures.FixtureCount >= 1)
        {
            var fixture = fixtures.Fixtures.First();

            _physics.SetCollisionMask(status.Owner.Value, fixture.Key, fixture.Value, (int) CollisionGroup.GhostImpassable, fixtures);
            _physics.SetCollisionLayer(status.Owner.Value, fixture.Key, fixture.Value, 0, fixtures);
        }
    }
}
