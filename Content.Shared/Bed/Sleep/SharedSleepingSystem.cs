using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage.ForceSay;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Hands;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Movement.Events;
using Content.Shared.Pointing;
using Content.Shared.Speech;
using Content.Shared.Standing;
using Content.Shared.Throwing;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.Bed.Sleep
{
    public abstract class SharedSleepingSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly BlindableSystem _blindableSystem = default!;
        [Dependency] private readonly StandingStateSystem _standingStateSystem = default!;
        [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;

        [ValidatePrototypeId<EntityPrototype>] private const string WakeActionId = "ActionWake";

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<SleepingComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<SleepingComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<SleepingComponent, SpeakAttemptEvent>(OnSpeakAttempt);
            SubscribeLocalEvent<SleepingComponent, CanSeeAttemptEvent>(OnSeeAttempt);
            SubscribeLocalEvent<SleepingComponent, PointAttemptEvent>(OnPointAttempt);

            SubscribeLocalEvent<SleepingComponent, StandAttemptEvent>(OnCancellableAttempt);
            SubscribeLocalEvent<SleepingComponent, UpdateCanMoveEvent>(OnCancellableAttempt);
            SubscribeLocalEvent<SleepingComponent, ChangeDirectionAttemptEvent>(OnCancellableAttempt);
            SubscribeLocalEvent<SleepingComponent, InteractionAttemptEvent>(OnCancellableAttempt);
            SubscribeLocalEvent<SleepingComponent, UseAttemptEvent>(OnCancellableAttempt);
            SubscribeLocalEvent<SleepingComponent, ThrowAttemptEvent>(OnCancellableAttempt);
            SubscribeLocalEvent<SleepingComponent, DropAttemptEvent>(OnCancellableAttempt);
            SubscribeLocalEvent<SleepingComponent, AttackAttemptEvent>(OnCancellableAttempt);
            SubscribeLocalEvent<SleepingComponent, PickupAttemptEvent>(OnCancellableAttempt);

            SubscribeLocalEvent<SleepingComponent, IsEquippingAttemptEvent>(OnHandsUseEquipAttempt);
            SubscribeLocalEvent<SleepingComponent, IsUnequippingAttemptEvent>(OnHandsUseUnequipAttempt);
        }


        private void OnMapInit(EntityUid uid, SleepingComponent component, MapInitEvent args)
        {
            var ev = new SleepStateChangedEvent(true);
            RaiseLocalEvent(uid, ev);
            _blindableSystem.UpdateIsBlind(uid);
            _standingStateSystem.Down(uid);
            _actionBlocker.UpdateCanMove(uid);
            _actionsSystem.AddAction(uid, ref component.WakeAction, WakeActionId, uid);

            // TODO remove hardcoded time.
            _actionsSystem.SetCooldown(component.WakeAction, _gameTiming.CurTime, _gameTiming.CurTime + TimeSpan.FromSeconds(15));
        }

        private void OnShutdown(EntityUid uid, SleepingComponent component, ComponentShutdown args)
        {
            _actionsSystem.RemoveAction(uid, component.WakeAction);
            var ev = new SleepStateChangedEvent(false);
            RaiseLocalEvent(uid, ev);
            _standingStateSystem.Stand(uid);
            _actionBlocker.UpdateCanMove(uid);
            _blindableSystem.UpdateIsBlind(uid);
        }

        private void OnSpeakAttempt(EntityUid uid, SleepingComponent component, SpeakAttemptEvent args)
        {
            // TODO reduce duplication of this behavior with MobStateSystem somehow
            if (HasComp<AllowNextCritSpeechComponent>(uid))
            {
                RemCompDeferred<AllowNextCritSpeechComponent>(uid);
                return;
            }

            args.Cancel();
        }

        private void OnSeeAttempt(EntityUid uid, SleepingComponent component, CanSeeAttemptEvent args)
        {
            if (component.LifeStage <= ComponentLifeStage.Running)
                args.Cancel();
        }

        private void OnCancellableAttempt<TEvent>(EntityUid uid, SleepingComponent component, TEvent args) where TEvent : CancellableEntityEventArgs
        {
            if (component.LifeStage <= ComponentLifeStage.Running)
                args.Cancel();
        }

        private void OnHandsUseEquipAttempt(EntityUid uid, SleepingComponent component, IsEquippingAttemptEvent args)
        {
            if (component.LifeStage > ComponentLifeStage.Running)
                return;

            if (args.Equipee == uid)
                args.Cancel();
        }

        private void OnHandsUseUnequipAttempt(EntityUid uid, SleepingComponent component, IsUnequippingAttemptEvent args)
        {
            if (component.LifeStage > ComponentLifeStage.Running)
                return;

            if (args.Unequipee == uid)
                args.Cancel();
        }

        private void OnPointAttempt(EntityUid uid, SleepingComponent component, PointAttemptEvent args)
        {
            args.Cancel();
        }
    }

    public sealed partial class TryWakeUpEv : CancellableEntityEventArgs
    {
        public readonly EntityUid? User;
        public TryWakeUpEv(EntityUid? user)
        {
            User = user;
        }
    }
}


public sealed partial class SleepActionEvent : InstantActionEvent { }

public sealed partial class WakeActionEvent : InstantActionEvent { }

/// <summary>
/// Raised on an entity when they fall asleep or wake up.
/// </summary>
public sealed class SleepStateChangedEvent : EntityEventArgs
{
    public bool FellAsleep = false;

    public SleepStateChangedEvent(bool fellAsleep)
    {
        FellAsleep = fellAsleep;
    }
}
