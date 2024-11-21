using Content.Shared.Drunk;
using Content.Shared.StatusEffect;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.Drunk;

public sealed class DrunkSystem : SharedDrunkSystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private DrunkOverlay _overlay = default!;
    private int _stacks = 0;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DrunkComponent, ComponentInit>(OnDrunkInit);
        SubscribeNetworkEvent<ClientStatusEffectOnApplicationEvent>(OnApplyInit);
        SubscribeLocalEvent<DrunkComponent, ComponentShutdown>(OnDrunkShutdown);

        SubscribeLocalEvent<DrunkComponent, StatusEffectRelayEvent<LocalPlayerAttachedEvent>>(OnPlayerAttached);
        SubscribeLocalEvent<DrunkComponent, StatusEffectRelayEvent<LocalPlayerDetachedEvent>>(OnPlayerDetached);

        _overlay = new();
    }

    private void OnPlayerAttached(EntityUid uid, DrunkComponent component, StatusEffectRelayEvent<LocalPlayerAttachedEvent> args)
    {
        AddOverlay();
    }

    private void OnPlayerDetached(EntityUid uid, DrunkComponent component, StatusEffectRelayEvent<LocalPlayerDetachedEvent> args)
    {
        RemoveOverlay();
    }

    private void OnDrunkInit(EntityUid uid, DrunkComponent component, ComponentInit args)
    {
        if (TryComp<StatusEffectComponent>(uid, out var statusEffectComp)
        && statusEffectComp.NetOwner != null
        && GetEntity(statusEffectComp.NetOwner.Value) == uid)
            AddOverlay();
    }

    private void OnApplyInit(ClientStatusEffectOnApplicationEvent args)
    {
        if (!HasComp<DrunkComponent>(GetEntity(args.Effect)))
            return;

        if (TryComp<StatusEffectComponent>(GetEntity(args.Effect), out var statusEffectComp)
        && statusEffectComp.NetOwner != null
        && GetEntity(statusEffectComp.NetOwner.Value) == _player.LocalEntity)
            AddOverlay();
    }

    private void OnDrunkShutdown(EntityUid uid, DrunkComponent component, ComponentShutdown args)
    {
        if (TryComp<StatusEffectComponent>(uid, out var statusEffectComp)
        && statusEffectComp.NetOwner != null
        && GetEntity(statusEffectComp.NetOwner.Value) == _player.LocalEntity)
            RemoveOverlay();
    }

    private void AddOverlay()
    {
        if (_stacks <= 0)
            _overlayMan.AddOverlay(_overlay);

        _stacks++;
    }

    private void RemoveOverlay()
    {
        _stacks--;

        if (_stacks <= 0)
        {
            _overlay.CurrentBoozePower = 0;
            _overlayMan.RemoveOverlay(_overlay);
        }
    }
}
