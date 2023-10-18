using System.Linq;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Thanatophobia.ShipGuns.Ammo;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Server.Thanatophobia.ShipGuns.Ammo;
public sealed partial class LinkedAmmoProviderSystem : SharedLinkedAmmoProviderSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LinkedAmmoProviderComponent, ComponentStartup>(TryUpdate);
        SubscribeLocalEvent<LinkedAmmoProviderComponent, NewLinkEvent>(TryUpdate);
        SubscribeLocalEvent<LinkedAmmoProviderComponent, PortDisconnectedEvent>(TryUpdate);
    }

    private void TryUpdate<T>(EntityUid uid, LinkedAmmoProviderComponent comp, T args)
    {
        comp.Outputs.Clear();

        if (TryComp<DeviceLinkSourceComponent>(uid, out var sourceComp)
        && sourceComp.Outputs.Any(i => i.Key == comp.Port))
        {
            comp.Outputs = GetNetEntityList(sourceComp.Outputs[comp.Port].ToList());
        }

        Dirty(uid, comp);
        UpdateAppearance(uid);
    }
}
