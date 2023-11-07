using System.Linq;
using Content.Shared.DeviceLinking;
using Content.Shared.Mobs;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Network;

namespace Content.Shared.Thanatophobia.ShipGuns.Ammo;

public abstract partial class SharedLinkedAmmoProviderSystem : EntitySystem
{

    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LinkedAmmoProviderComponent, TakeAmmoEvent>(OnAmmoEv);
        SubscribeLocalEvent<LinkedAmmoProviderComponent, GetAmmoCountEvent>(OnAmmoCountEv);
    }

    private void OnAmmoEv(EntityUid uid, LinkedAmmoProviderComponent comp, TakeAmmoEvent ammo)
    {
        foreach (var ammoUid in comp.Outputs)
        {
            if (HasComp<LinkedAmmoProviderComponent>(GetEntity(ammoUid)) // To prevent iteration issues in the future.
            || !comp.Whitelist.IsValid(GetEntity(ammoUid)))
                continue;

            RaiseLocalEvent(GetEntity(ammoUid), ammo);

            if (ammo.Ammo.Any())
                break;
        }
        UpdateAppearance(uid);
    }

    private void OnAmmoCountEv(EntityUid uid, LinkedAmmoProviderComponent comp, ref GetAmmoCountEvent args)
    {
        foreach (var ammoUid in comp.Outputs)
        {
            if (HasComp<LinkedAmmoProviderComponent>(GetEntity(ammoUid))
            || !comp.Whitelist.IsValid(GetEntity(ammoUid)))
                continue;

            var moreAmmo = new GetAmmoCountEvent();

            RaiseLocalEvent(GetEntity(ammoUid), ref moreAmmo);

            args.Capacity += moreAmmo.Capacity;
            args.Count += moreAmmo.Count;
        }
    }

    public void UpdateAppearance(EntityUid uid)
    {
        if (!TryComp<AppearanceComponent>(uid, out var visualComp))
            return;

        var ammoEv = new GetAmmoCountEvent();
        RaiseLocalEvent(uid, ref ammoEv);

        _appearanceSystem.SetData(uid, AmmoVisuals.HasAmmo, ammoEv.Count != 0, visualComp);
        _appearanceSystem.SetData(uid, AmmoVisuals.AmmoCount, ammoEv.Count, visualComp);
        _appearanceSystem.SetData(uid, AmmoVisuals.AmmoMax, ammoEv.Capacity, visualComp);
    }
}
