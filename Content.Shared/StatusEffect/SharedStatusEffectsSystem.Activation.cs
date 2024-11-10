using Content.Shared.Damage;

namespace Content.Shared.StatusEffect;
public abstract partial class SharedStatusEffectsSystem
{
    public void InitializeActivation()
    {
        SubscribeLocalEvent<ActivateEffectEveryUpdateComponent, StatusEffectRelayEvent<StatusEffectUpdateEvent>>(RelayedActivation);
        SubscribeLocalEvent<ActivateEffectOnTimeoutComponent, StatusEffectTimeoutEvent>(TimeoutActivation);
        SubscribeLocalEvent<ActivateEffectOnDamageComponent, StatusEffectRelayEvent<DamageChangedEvent>>(HurtActivation);
    }

    private void RelayedActivation<TComp, TEvent>(EntityUid uid, TComp comp, StatusEffectRelayEvent<TEvent> args)
    {
        var ev = new StatusEffectActivateEvent(args.Victim);
        RaiseLocalEvent(uid, ev);
    }

    private void TimeoutActivation(EntityUid uid, ActivateEffectOnTimeoutComponent comp, StatusEffectTimeoutEvent args)
    {
        var ev = new StatusEffectActivateEvent(args.Victim);
        RaiseLocalEvent(uid, ev);
    }

    private void HurtActivation(EntityUid uid, ActivateEffectOnDamageComponent comp, StatusEffectRelayEvent<DamageChangedEvent> args)
    {
        if (args.Args.Origin == args.Victim || // This should prevent an infinite loop that causes the engine to crash.
            !args.Args.DamageIncreased)
            return;

        var ev = new StatusEffectActivateEvent(args.Victim);
        RaiseLocalEvent(uid, ev);
    }
}
