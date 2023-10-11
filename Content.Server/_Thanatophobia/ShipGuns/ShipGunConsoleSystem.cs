using System.Linq;
using System.Numerics;
using System.Xml;
using Content.Server.DeviceLinking.Systems;
using Content.Server.Power.Components;
using Content.Server.UserInterface;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.ActionBlocker;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Mobs;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Thanatophobia.ShipGuns;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using FastAccessors;
using Robust.Server.GameObjects;
using Robust.Shared.Map;

namespace Content.Server.Thanatophobia.ShipGuns;
public sealed partial class ShipGunConsoleSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly GunSystem _gunSystem = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShipGunConsoleComponent, ComponentStartup>(OnUpdateEvent);
        SubscribeLocalEvent<ShipGunConsoleComponent, NewLinkEvent>(OnUpdateEvent);
        SubscribeLocalEvent<ShipGunConsoleComponent, AnchorStateChangedEvent>(OnUpdateRefEvent);
        SubscribeLocalEvent<ShipGunConsoleComponent, PowerChangedEvent>(OnUpdateRefEvent);

        SubscribeLocalEvent<ShipGunConsoleComponent, ShipGunConsolePosMessage>(RotationUpdate);
        SubscribeLocalEvent<ShipGunConsoleComponent, ShipGunConsoleShootMessage>(ShootEvent);
        SubscribeLocalEvent<ShipGunConsoleComponent, ShipGunConsoleUnshootMessage>(UnshootEvent);
        SubscribeLocalEvent<ShipGunConsoleComponent, ShipGunConsoleSetGroupMessage>(SetGroupEvent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
    }

    private void OnUpdateEvent<T>(EntityUid uid, ShipGunConsoleComponent comp, T args)
    {
        UpdateState(uid, comp);
    }

    private void OnUpdateRefEvent<T>(EntityUid uid, ShipGunConsoleComponent comp, ref T args)
    {
        UpdateState(uid, comp);
    }

    private void UpdateState(EntityUid uid, ShipGunConsoleComponent comp)
    {
        var xform = Transform(uid);
        var onGrid = xform.ParentUid == xform.GridUid;
        EntityCoordinates? coordinates = new EntityCoordinates(uid, Vector2.Zero);
        Angle? angle = onGrid ? xform.LocalRotation : null;

        var potentialGuns = GetPortEnts(uid, comp);

        List<ShipGunState> gunsInfo = new();

        if (potentialGuns != null)
        {
            foreach (var gun in potentialGuns)
            {
                if (!HasComp<ShipGunComponent>(gun))
                    continue;

                var ev = new GetAmmoCountEvent();
                RaiseLocalEvent(gun, ref ev);

                gunsInfo.Add(new ShipGunState()
                {
                    Uid = GetNetEntity(gun),
                });
            }
        }

        if (_uiSystem.TryGetUi(uid, RadarConsoleUiKey.Key, out var bui))
        {
            _uiSystem.SetUiState(bui, new ShipGunConsoleBoundUIState
            (
                comp.MaxRange,
                GetNetCoordinates(coordinates),
                angle,
                comp.CurGroup,
                gunsInfo,
                comp.GunPorts.Count()
            ));
        }
    }

    private List<EntityUid>? GetPortEnts(EntityUid uid, ShipGunConsoleComponent comp, DeviceLinkSourceComponent? sourceComp = null)
    {
        if (!Resolve(uid, ref sourceComp))
            return null;

        if (!comp.GunPorts.Any())
            return null;

        var ports = sourceComp.Outputs;

        if (!ports.TryGetValue(comp.GunPorts[Math.Clamp(comp.CurGroup, 0, comp.GunPorts.Count - 1)], out var entities))
            return null;

        return entities.ToList();
    }

    private void RotationUpdate(EntityUid uid, ShipGunConsoleComponent comp, ShipGunConsolePosMessage args)
    {
        if (args.Session.AttachedEntity is not { })
            return;

        var guns = GetPortEnts(uid, comp);

        if (guns == null)
            return;

        foreach (var gun in guns)
            RaiseLocalEvent(gun, new ShipGunUpdateRotation(GetCoordinates(args.MousePos).ToMapPos(EntityManager, _transformSystem)));
    }

    private void ShootEvent(EntityUid uid, ShipGunConsoleComponent comp, ShipGunConsoleShootMessage args)
    {
        if (args.Session.AttachedEntity is not { })
            return;

        var guns = GetPortEnts(uid, comp);

        if (guns == null)
            return;

        foreach (var gun in guns)
            RaiseLocalEvent(gun, new ShipGunShootEvent());
    }

    private void UnshootEvent(EntityUid uid, ShipGunConsoleComponent comp, ShipGunConsoleUnshootMessage args)
    {
        if (args.Session.AttachedEntity is not { })
            return;

        UnshootGuns(uid);

        UnshootGuns(uid, comp);
    }

    private void UnshootGuns(EntityUid uid, ShipGunConsoleComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        var guns = GetPortEnts(uid, comp);

        if (guns == null)
            return;

        foreach (var gun in guns)
            RaiseLocalEvent(gun, new ShipGunUnshootEvent());
    }

    private void SetGroupEvent(EntityUid uid, ShipGunConsoleComponent comp, ShipGunConsoleSetGroupMessage args)
    {
        if (args.Session.AttachedEntity is not { })
            return;

        UnshootGuns(uid);

        comp.CurGroup = Math.Clamp(args.Group, 0, comp.GunPorts.Count - 1);
        UpdateState(uid, comp);
    }
}
