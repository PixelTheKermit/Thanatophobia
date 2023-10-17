using System.Numerics;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.DeviceLinking;
using Content.Shared.Thanatophobia.ShipGuns;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map;

namespace Content.Server.Thanatophobia.ShipGuns;
public sealed partial class ShipGunSystem : EntitySystem
{
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly GunSystem _gunSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShipGunComponent, ShipGunUpdateRotation>(OnRotUpdate);
        SubscribeLocalEvent<ShipGunComponent, ShipGunShootEvent>(OnShootEvent);
        SubscribeLocalEvent<ShipGunComponent, ShipGunUnshootEvent>(OnUnshootEvent);

        SubscribeLocalEvent<ShipGunComponent, MoveEvent>(TryUpdateUI);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var shipGuns = EntityQueryEnumerator<ShipGunComponent, GunComponent>();

        while (shipGuns.MoveNext(out var uid, out var comp, out var gunComp))
        {
            if (comp.IsRotating)
            {
                var worldRot = _transformSystem.GetWorldRotation(uid);
                _transformSystem.SetWorldRotation(uid, worldRot + Math.Clamp(Angle.ShortestDistance(worldRot, comp.PointTo), -comp.MaxRotSpeed * frameTime, comp.MaxRotSpeed * frameTime));

                if (Angle.ShortestDistance(worldRot, comp.PointTo) == 0)
                    comp.IsRotating = false;
            }
        }
    }

    private void OnRotUpdate(EntityUid uid, ShipGunComponent comp, ShipGunUpdateRotation args)
    {
        var rotVector = args.PointTo - _transformSystem.GetWorldPosition(uid);
        comp.PointTo = rotVector.ToWorldAngle();
        comp.IsRotating = true;
    }

    private void OnShootEvent(EntityUid uid, ShipGunComponent comp, ShipGunShootEvent args)
    {
        if (!TryComp<GunComponent>(uid, out var gunComp))
            return;

        _gunSystem.AttemptShoot(uid, uid, gunComp, new EntityCoordinates(uid, new Vector2(0, -1)));
        TryUpdateUI(uid, comp);
    }

    private void OnUnshootEvent(EntityUid uid, ShipGunComponent comp, ShipGunUnshootEvent args)
    {
        comp.IsShooting = false;

        if (!TryComp<GunComponent>(uid, out var gunComp))
            return;

        if (gunComp.ShotCounter == 0)
            return;

        gunComp.ShotCounter = 0;
        gunComp.ShootCoordinates = null;
    }

    private void TryUpdateUI<T>(EntityUid uid, ShipGunComponent comp, ref T args)
    {
        TryUpdateUI(uid, comp);
    }

    private void TryUpdateUI(EntityUid uid, ShipGunComponent comp)
    {
        if (!TryComp<DeviceLinkSinkComponent>(uid, out var sinkComp))
            return;

        var sources = sinkComp.LinkedSources;

        foreach (var source in sources)
            RaiseLocalEvent(source, new ShipGunUpdateUIEvent());
    }
}

public sealed partial class ShipGunUpdateRotation : EntityEventArgs
{
    public Vector2 PointTo;

    public ShipGunUpdateRotation(Vector2 pointTo)
    {
        PointTo = pointTo;
    }
}

public sealed partial class ShipGunShootEvent : EntityEventArgs
{
}

public sealed partial class ShipGunUnshootEvent : EntityEventArgs
{
}

public sealed partial class ShipGunUpdateUIEvent : EntityEventArgs
{
}
