using Content.Shared.Damage;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.StatusEffect;
using Content.Shared.Movement.Events;
using Content.Shared.Interaction;
using System.Runtime.InteropServices;
using Content.Shared.Standing;
using Content.Shared.Interaction.Events;
using Content.Shared.Throwing;
using Content.Shared.Hands;
using Content.Shared.Item;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Movement.Components;
using Content.Server.Bed.Sleep;
using Content.Shared.Movement.Systems;

namespace Content.Shared.StatusEffect;

/// <summary>
/// Mostly for effects that don't spawn stuff/delete entities
/// </summary>
public abstract partial class SharedStatusEffectsSystem
{
    public void InitializeEffects()
    {
        SubscribeLocalEvent<DamageEntityEffectComponent, StatusEffectActivateEvent>(DamageEffect);

        SubscribeLocalEvent<AttackDamageEffectComponent, StatusEffectRelayEvent<MeleeHitEvent>>(AttackDamageEffect);
        SubscribeLocalEvent<DefenceEffectComponent, StatusEffectRelayEvent<DamageModifyEvent>>(DefenceEffect);

        SubscribeLocalEvent<AdjustSpeedEffectComponent, StatusEffectModifiedEvent>(UpdateMovementSpeedEffect);
        SubscribeLocalEvent<AdjustSpeedEffectComponent, ComponentShutdown>(UpdateMovementSpeedEffect);
        SubscribeLocalEvent<AdjustSpeedEffectComponent, StatusEffectRelayEvent<RefreshMovementSpeedModifiersEvent>>(RefreshMovementSpeedEffect);

        SubscribeLocalEvent<PreventMovementEffectComponent, StatusEffectRelayEvent<UpdateCanMoveEvent>>(StopMoveEffect);
        SubscribeLocalEvent<PreventMovementEffectComponent, StatusEffectRelayEvent<ChangeDirectionAttemptEvent>>(StopMoveRotateEffect);
        SubscribeLocalEvent<PreventMovementEffectComponent, StatusEffectModifiedEvent>(UpdateUnmoveEffect);
        SubscribeLocalEvent<PreventMovementEffectComponent, ComponentShutdown>(UpdateUnmoveEffect);
        SubscribeLocalEvent<AdjustTileFrictionEffectComponent, StatusEffectRelayEvent<TileFrictionEvent>>(TileFrictionEffect);
        SubscribeLocalEvent<ForcedDownedEffectComponent, StatusEffectModifiedEvent>(OnForceDownedUpdate);
        SubscribeLocalEvent<ForcedDownedEffectComponent, ComponentShutdown>(OnForceDownedShutdown);
        SubscribeLocalEvent<ForcedDownedEffectComponent, StatusEffectRelayEvent<StandAttemptEvent>>(OnForceDownedStandAttempt);

        SubscribeLocalEvent<PreventUseOfHandsEffectComponent, StatusEffectRelayEvent<InteractionAttemptEvent>>(OnHandsUseAttempt);
        SubscribeLocalEvent<PreventUseOfHandsEffectComponent, StatusEffectRelayEvent<UseAttemptEvent>>(OnHandsUseAttempt);
        SubscribeLocalEvent<PreventUseOfHandsEffectComponent, StatusEffectRelayEvent<ThrowAttemptEvent>>(OnHandsUseAttempt);
        SubscribeLocalEvent<PreventUseOfHandsEffectComponent, StatusEffectRelayEvent<DropAttemptEvent>>(OnHandsUseAttempt);
        SubscribeLocalEvent<PreventUseOfHandsEffectComponent, StatusEffectRelayEvent<AttackAttemptEvent>>(OnHandsUseAttempt);
        SubscribeLocalEvent<PreventUseOfHandsEffectComponent, StatusEffectRelayEvent<PickupAttemptEvent>>(OnHandsUseAttempt);
        SubscribeLocalEvent<PreventUseOfHandsEffectComponent, StatusEffectRelayEvent<IsEquippingAttemptEvent>>(OnHandsUseEquipAttempt);
        SubscribeLocalEvent<PreventUseOfHandsEffectComponent, StatusEffectRelayEvent<IsUnequippingAttemptEvent>>(OnHandsUseUnequipAttempt);

        SubscribeLocalEvent<BlindnessEffectComponent, StatusEffectModifiedEvent>(UpdateBlindlessEffect);
        SubscribeLocalEvent<BlindnessEffectComponent, ComponentShutdown>(UpdateBlindlessEffect);
        SubscribeLocalEvent<BlindnessEffectComponent, StatusEffectRelayEvent<CanSeeAttemptEvent>>(OnBlindAttempt);

        SubscribeLocalEvent<ForceSayOnApplyEffectComponent, StatusEffectOnApplicationEvent>(ForceSayOnApply);

        SubscribeLocalEvent<ClearEffectOnCritComponent, StatusEffectRelayEvent<MobStateChangedEvent>>(ClearEffectOnCrit);

        SubscribeLocalEvent<ReduceEffectTimeOnInteractComponent, StatusEffectRelayEvent<InteractHandEvent>>(ReduceTimeEffect);

        SubscribeLocalEvent<StatusEffectIconComponent, StatusEffectRelayEvent<GetStatusIconsEvent>>(OnGetStatusIcon);

        SubscribeLocalEvent<AlertEffectComponent, StatusEffectModifiedEvent>(AlertGet);
        SubscribeLocalEvent<AlertEffectComponent, StatusEffectRelayEvent<AlertEffectGoneEv>>(AlertGet);
        SubscribeLocalEvent<AlertEffectComponent, ComponentShutdown>(AlertShutdown);
    }

    #region Active Effects

    private void DamageEffect(EntityUid uid, DamageEntityEffectComponent comp, StatusEffectActivateEvent args)
    {
        if (!TryComp<StatusEffectComponent>(uid, out var effectComp) || !TryComp<DamageableComponent>(args.Victim, out var damageComp))
            return;

        var damage = comp.Damage;

        if (comp.ScaleWithStrength)
            damage *= effectComp.OverallStrength;

        if (!comp.IsFatalDamage &&
            _thresholdSystem.TryGetDeadThreshold(args.Victim, out var deadThreshold) &&
            deadThreshold < damageComp.Damage.GetTotal() + damage.GetTotal())
            return;

        _damageableSystem.TryChangeDamage(args.Victim, damage, true, false, origin: args.Victim);
    }

    #endregion

    #region Passive Effects
    private void DefenceEffect(EntityUid uid, DefenceEffectComponent comp, StatusEffectRelayEvent<DamageModifyEvent> args)
    {
        if (comp.LifeStage > ComponentLifeStage.Running)
            return;

        if (!TryComp<StatusEffectComponent>(uid, out var effectComp))
            return;

        args.Args.Damage = DamageSpecifier.ApplyModifierSet(args.Args.Damage, ScaleModiferWithStrength(comp.Modifiers, effectComp.OverallStrength));
    }

    private void AttackDamageEffect(EntityUid uid, AttackDamageEffectComponent comp, StatusEffectRelayEvent<MeleeHitEvent> args)
    {
        if (comp.LifeStage > ComponentLifeStage.Running)
            return;

        if (!TryComp<StatusEffectComponent>(uid, out var effectComp))
            return;

        args.Args.BonusDamage = DamageSpecifier.ApplyModifierSet(args.Args.BonusDamage, ScaleModiferWithStrength(comp.Modifiers, effectComp.OverallStrength));
    }

    private void OnGetStatusIcon(EntityUid uid, StatusEffectIconComponent component, StatusEffectRelayEvent<GetStatusIconsEvent> args)
    {
        if (component.LifeStage > ComponentLifeStage.Running)
            return;

        args.Args.StatusIcons.Add(PrototypeManager.Index<StatusIconPrototype>(component.StatusIcon));
    }

    private void UpdateUnmoveEffect<TEvent>(EntityUid uid, PreventMovementEffectComponent component, ref TEvent args)
    {
        if (!TryComp<StatusEffectComponent>(uid, out var effectComp) || effectComp.Owner == null)
            return;

        if (TryComp<InputMoverComponent>(effectComp.Owner.Value, out var inputMove))
        {
            ActionBlocker.UpdateCanMove(effectComp.Owner.Value, inputMove);
        }
    }
    private void StopMoveEffect(EntityUid uid, PreventMovementEffectComponent component, StatusEffectRelayEvent<UpdateCanMoveEvent> args)
    {
        if (component.LifeStage > ComponentLifeStage.Running)
            return;

        if (!TryComp<StatusEffectComponent>(uid, out var effectComp))
            return;

        if (effectComp.OverallStrength >= component.StrengthNeeded)
            args.Args.Cancel();
    }


    private void StopMoveRotateEffect(EntityUid uid, PreventMovementEffectComponent component, StatusEffectRelayEvent<ChangeDirectionAttemptEvent> args)
    {
        if (component.LifeStage > ComponentLifeStage.Running)
            return;

        if (!TryComp<StatusEffectComponent>(uid, out var effectComp))
            return;

        if (effectComp.OverallStrength >= component.StrengthNeeded)
            args.Args.Cancel();
    }

    private void OnHandsUseAttempt<TEvent>(EntityUid uid, PreventUseOfHandsEffectComponent component, StatusEffectRelayEvent<TEvent> args) where TEvent : CancellableEntityEventArgs
    {
        if (component.LifeStage > ComponentLifeStage.Running)
            return;

        if (!TryComp<StatusEffectComponent>(uid, out var effectComp))
            return;

        if (effectComp.OverallStrength >= component.StrengthNeeded)
            args.Args.Cancel();
    }

    private void OnHandsUseEquipAttempt(EntityUid uid, PreventUseOfHandsEffectComponent component, StatusEffectRelayEvent<IsEquippingAttemptEvent> args)
    {
        if (component.LifeStage > ComponentLifeStage.Running)
            return;

        if (!TryComp<StatusEffectComponent>(uid, out var effectComp))
            return;

        if (effectComp.OverallStrength >= component.StrengthNeeded && args.Args.Equipee == uid)
            args.Args.Cancel();
    }

    private void OnHandsUseUnequipAttempt(EntityUid uid, PreventUseOfHandsEffectComponent component, StatusEffectRelayEvent<IsUnequippingAttemptEvent> args)
    {
        if (component.LifeStage > ComponentLifeStage.Running)
            return;

        if (!TryComp<StatusEffectComponent>(uid, out var effectComp))
            return;

        if (effectComp.OverallStrength >= component.StrengthNeeded && args.Args.Unequipee == uid)
            args.Args.Cancel();
    }

    private void TileFrictionEffect(EntityUid uid, AdjustTileFrictionEffectComponent component, ref StatusEffectRelayEvent<TileFrictionEvent> args)
    {
        if (component.LifeStage > ComponentLifeStage.Running)
            return;

        if (!TryComp<StatusEffectComponent>(uid, out var effectComp))
            return;

        args.Args.Modifier *= (float) Math.Pow(component.Multiplier, effectComp.OverallStrength);
    }

    private void OnForceDownedUpdate(EntityUid uid, ForcedDownedEffectComponent component, StatusEffectModifiedEvent args)
    {
        if (component.LifeStage > ComponentLifeStage.Running)
            return;

        if (!TryComp<StatusEffectComponent>(uid, out var effectComp) || effectComp.Owner == null)
            return;

        if (effectComp.OverallStrength < component.StrengthNeeded)
            return;

        StandingStateSystem.Down(effectComp.Owner.Value);
    }

    private void OnForceDownedShutdown(EntityUid uid, ForcedDownedEffectComponent component, ComponentShutdown args)
    {
        if (!TryComp<StatusEffectComponent>(uid, out var effectComp) || effectComp.Owner == null)
            return;

        StandingStateSystem.Stand(effectComp.Owner.Value);
    }
    private void OnForceDownedStandAttempt(EntityUid uid, ForcedDownedEffectComponent component, StatusEffectRelayEvent<StandAttemptEvent> args)
    {
        if (component.LifeStage > ComponentLifeStage.Running)
            return;

        if (!TryComp<StatusEffectComponent>(uid, out var effectComp))
            return;

        if (effectComp.OverallStrength >= component.StrengthNeeded)
            args.Args.Cancel();
    }

    private void UpdateBlindlessEffect<TEvent>(EntityUid uid, BlindnessEffectComponent component, TEvent args)
    {
        if (!TryComp<StatusEffectComponent>(uid, out var effectComp) || effectComp.Owner == null)
            return;

        BlindableSystem.UpdateIsBlind(effectComp.Owner.Value);
    }

    private void OnBlindAttempt(EntityUid uid, BlindnessEffectComponent component, StatusEffectRelayEvent<CanSeeAttemptEvent> args)
    {
        if (component.LifeStage > ComponentLifeStage.Running)
            return;

        if (!TryComp<StatusEffectComponent>(uid, out var effectComp))
            return;

        if (effectComp.OverallStrength >= component.StrengthNeeded)
            args.Args.Cancel();
    }

    private void UpdateMovementSpeedEffect<TEvent>(EntityUid uid, AdjustSpeedEffectComponent component, TEvent args)
    {
        if (!TryComp<StatusEffectComponent>(uid, out var effectComp) || effectComp.Owner == null)
            return;

        MovementSpeedModifierSystem.RefreshMovementSpeedModifiers(effectComp.Owner.Value);
    }

    private void RefreshMovementSpeedEffect(EntityUid uid, AdjustSpeedEffectComponent component, StatusEffectRelayEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        if (component.LifeStage > ComponentLifeStage.Running)
            return;

        if (!TryComp<StatusEffectComponent>(uid, out var effectComp))
            return;

        args.Args.ModifySpeed((float) Math.Pow(component.WalkingSpeed, effectComp.OverallStrength), (float) Math.Pow(component.SprintingSpeed, effectComp.OverallStrength));
    }

    private void AlertGet<TEvent>(EntityUid uid, AlertEffectComponent component, TEvent args)
    {
        if (component.LifeStage > ComponentLifeStage.Running)
            return;

        if (!TryComp<StatusEffectComponent>(uid, out var effectComp) || effectComp.Owner == null)
            return;

        AlertsSystem.ShowAlert(effectComp.Owner.Value, component.Alert, cooldown: effectComp.IsTimed ? (effectComp.AppliedTime, effectComp.Length) : null);
    }

    private void AlertShutdown(EntityUid uid, AlertEffectComponent component, ComponentShutdown args)
    {
        if (!TryComp<StatusEffectComponent>(uid, out var effectComp) || effectComp.Owner == null)
            return;

        AlertsSystem.ClearAlert(effectComp.Owner.Value, component.Alert);
        RaiseLocalEvent(uid, new AlertEffectGoneEv());
    }

    #endregion

    #region Specific Effects

    private void ReduceTimeEffect(EntityUid uid, ReduceEffectTimeOnInteractComponent comp, StatusEffectRelayEvent<InteractHandEvent> args)
    {
        if (!TryComp<StatusEffectComponent>(uid, out var effectComp))
            return;

        if (args.Args.User == uid && comp.InteractWithSelf == false)
            return;

        if (comp.InteractSound != null)
            AudioSystem.PlayPredicted(comp.InteractSound, uid, effectComp.Owner);

        effectComp.Length -= comp.TimeToDecrease;
    }

    private void ClearEffectOnCrit(EntityUid uid, ClearEffectOnCritComponent comp, StatusEffectRelayEvent<MobStateChangedEvent> args)
    {
        switch (args.Args.NewMobState)
        {
            case MobState.Alive:
                break;
            case MobState.Critical:
                QueueDel(uid);
                break;
            case MobState.Dead:
                QueueDel(uid);
                break;
            case MobState.Invalid:
            default:
                return;
        }
    }

    private void ForceSayOnApply(EntityUid uid, ForceSayOnApplyEffectComponent comp, StatusEffectOnApplicationEvent args)
    {
        RaiseLocalEvent(args.Victim, new ForceSayOnApplyEffectEvent());
    }

    #endregion

    #region Math functions

    private static DamageModifierSet ScaleModiferWithStrength(DamageModifierSet modifierSet, float strength)
    {
        DamageModifierSet newModifier = new();

        foreach (var coefficient in modifierSet.Coefficients)
        {
            newModifier.Coefficients[coefficient.Key] = (float) Math.Pow(coefficient.Value, strength);
        }

        foreach (var flatReduction in modifierSet.FlatReduction)
        {
            newModifier.FlatReduction[flatReduction.Key] = (float) Math.Pow(flatReduction.Value, strength);
        }

        return newModifier;
    }
    #endregion
}
