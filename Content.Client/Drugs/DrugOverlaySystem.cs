using Content.Shared.Drugs;
using Content.Shared.StatusEffect;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.Drugs;

/// <summary>
///     System to handle drug related overlays.
/// </summary>
public sealed class DrugOverlaySystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private RainbowOverlay _overlay = default!;
    private int _stacks = 0;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SeeingRainbowsComponent, ComponentInit>(OnInit);
        SubscribeNetworkEvent<ClientStatusEffectOnApplicationEvent>(OnApplyInit);
        SubscribeLocalEvent<SeeingRainbowsComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<SeeingRainbowsComponent, StatusEffectRelayEvent<LocalPlayerAttachedEvent>>(OnPlayerAttached);
        SubscribeLocalEvent<SeeingRainbowsComponent, StatusEffectRelayEvent<LocalPlayerDetachedEvent>>(OnPlayerDetached);

        _overlay = new();
    }

    private void OnPlayerAttached(EntityUid uid, SeeingRainbowsComponent component, StatusEffectRelayEvent<LocalPlayerAttachedEvent> args)
    {
        AddOverlay();
    }

    private void OnPlayerDetached(EntityUid uid, SeeingRainbowsComponent component, StatusEffectRelayEvent<LocalPlayerDetachedEvent> args)
    {
        RemoveOverlay();
    }

    private void OnInit(EntityUid uid, SeeingRainbowsComponent component, ComponentInit args)
    {
        if (TryComp<StatusEffectComponent>(uid, out var statusEffectComp)
        && statusEffectComp.NetOwner != null
        && GetEntity(statusEffectComp.NetOwner.Value) == uid)
            AddOverlay();
    }

    private void OnApplyInit(ClientStatusEffectOnApplicationEvent args)
    {
        if (!HasComp<SeeingRainbowsComponent>(GetEntity(args.Effect)))
            return;

        if (TryComp<StatusEffectComponent>(GetEntity(args.Effect), out var statusEffectComp)
        && statusEffectComp.NetOwner != null
        && GetEntity(statusEffectComp.NetOwner.Value) == _player.LocalEntity)
            AddOverlay();
    }

    private void OnShutdown(EntityUid uid, SeeingRainbowsComponent component, ComponentShutdown args)
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
            _overlay.Intoxication = 0;
            _overlayMan.RemoveOverlay(_overlay);
        }
    }
}
