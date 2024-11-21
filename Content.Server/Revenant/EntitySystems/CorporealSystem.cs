using Content.Server.GameTicking;
using Content.Shared.Eye;
using Content.Shared.Revenant.Components;
using Content.Shared.Revenant.EntitySystems;
using Content.Shared.StatusEffect;
using Robust.Server.GameObjects;

namespace Content.Server.Revenant.EntitySystems;

public sealed class CorporealSystem : SharedCorporealSystem
{
    [Dependency] private readonly VisibilitySystem _visibilitySystem = default!;
    [Dependency] private readonly GameTicker _ticker = default!;

    public override void OnStartup(EntityUid uid, CorporealComponent component, ComponentStartup args)
    {
        base.OnStartup(uid, component, args);

        if (!TryComp<StatusEffectComponent>(uid, out var status) || status.Owner == null)
            return;

        if (TryComp<VisibilityComponent>(status.Owner.Value, out var visibility))
        {
            _visibilitySystem.RemoveLayer(status.Owner.Value, visibility, (int) VisibilityFlags.Ghost, false);
            _visibilitySystem.AddLayer(status.Owner.Value, visibility, (int) VisibilityFlags.Normal, false);
            _visibilitySystem.RefreshVisibility(status.Owner.Value, visibility);
        }
    }

    public override void OnShutdown(EntityUid uid, CorporealComponent component, ComponentShutdown args)
    {
        base.OnShutdown(uid, component, args);

        if (!TryComp<StatusEffectComponent>(uid, out var status) || status.Owner == null)
            return;

        if (TryComp<VisibilityComponent>(status.Owner.Value, out var visibility) && _ticker.RunLevel != GameRunLevel.PostRound)
        {
            _visibilitySystem.AddLayer(status.Owner.Value, visibility, (int) VisibilityFlags.Ghost, false);
            _visibilitySystem.RemoveLayer(status.Owner.Value, visibility, (int) VisibilityFlags.Normal, false);
            _visibilitySystem.RefreshVisibility(status.Owner.Value, visibility);
        }
    }
}
