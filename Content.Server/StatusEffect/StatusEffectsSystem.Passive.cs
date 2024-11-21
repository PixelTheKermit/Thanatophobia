using Content.Server.Bed.Sleep;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server.StatusEffect;

public sealed partial class StatusEffectsSystem
{
    public void InitializePassive()
    {
        SubscribeLocalEvent<ForceSleepingEffectComponent, StatusEffectModifiedEvent>(UpdateSleepEffect);
        SubscribeLocalEvent<ForceSleepingEffectComponent, ComponentShutdown>(UpdateSleepEffect);
        SubscribeLocalEvent<ForceSleepingEffectComponent, StatusEffectRelayEvent<TryWakeUpEv>>(OnWakeAttempt);
    }

    private void UpdateSleepEffect<TEvent>(EntityUid uid, ForceSleepingEffectComponent component, TEvent args)
    {
        if (!TryComp<StatusEffectComponent>(uid, out var effectComp) || effectComp.Owner == null)
            return;

        if (effectComp.OverallStrength >= component.StrengthNeeded)
            SleepingSystem.TrySleeping(effectComp.Owner.Value);
    }

    private void OnWakeAttempt(EntityUid uid, ForceSleepingEffectComponent component, StatusEffectRelayEvent<TryWakeUpEv> args)
    {
        if (component.LifeStage > ComponentLifeStage.Running)
            return;

        if (!TryComp<StatusEffectComponent>(uid, out var effectComp))
            return;

        if (effectComp.OverallStrength >= component.StrengthNeeded)
            args.Args.Cancel();
    }

}

