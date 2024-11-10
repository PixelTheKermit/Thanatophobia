using System.Linq;
using Content.Shared._ArcheCrawl.CCVar;
using Content.Shared.ActionBlocker;
using Content.Shared.Damage;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Hands;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Standing;
using Content.Shared.StatusIcon.Components;
using Content.Shared.Tag;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared.StatusEffect;

/// <summary>
/// The system in shared that controls most things related to AC's Status Effects.
/// </summary>
public abstract partial class SharedStatusEffectsSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly IPrototypeManager PrototypeManager = default!;
    [Dependency] protected readonly ActionBlockerSystem ActionBlocker = default!;
    [Dependency] protected readonly SharedAudioSystem AudioSystem = default!;
    [Dependency] protected readonly StandingStateSystem StandingStateSystem = default!;
    [Dependency] protected readonly BlindableSystem BlindableSystem = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly MobThresholdSystem _thresholdSystem = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StatusEffectsComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<StatusEffectComponent, ComponentStartup>(OnEffectStartup);

        // Event relays down here
        SubscribeLocalEvent<StatusEffectsComponent, MeleeHitEvent>(RelayEvent);
        SubscribeLocalEvent<StatusEffectsComponent, BeforeDamageChangedEvent>(RefRelayEvent);
        SubscribeLocalEvent<StatusEffectsComponent, DamageModifyEvent>(RelayEvent);
        SubscribeLocalEvent<StatusEffectsComponent, DamageChangedEvent>(RelayEvent);
        SubscribeLocalEvent<StatusEffectsComponent, GetStatusIconsEvent>(RefRelayEvent);
        SubscribeLocalEvent<StatusEffectsComponent, TileFrictionEvent>(RefRelayEvent);
        SubscribeLocalEvent<StatusEffectsComponent, InteractHandEvent>(RelayEvent);
        SubscribeLocalEvent<StatusEffectsComponent, UpdateCanMoveEvent>(RelayEvent);
        SubscribeLocalEvent<StatusEffectsComponent, StatusEffectModifiedEvent>(RelayEvent);
        SubscribeLocalEvent<StatusEffectsComponent, MobStateChangedEvent>(RelayEvent);
        SubscribeLocalEvent<StatusEffectsComponent, CanSeeAttemptEvent>(RefRelayEvent);
        SubscribeLocalEvent<StatusEffectsComponent, InteractionAttemptEvent>(RelayEvent);
        SubscribeLocalEvent<StatusEffectsComponent, UseAttemptEvent>(RelayEvent);
        SubscribeLocalEvent<StatusEffectsComponent, ThrowAttemptEvent>(RelayEvent);
        SubscribeLocalEvent<StatusEffectsComponent, DropAttemptEvent>(RelayEvent);
        SubscribeLocalEvent<StatusEffectsComponent, AttackAttemptEvent>(RelayEvent);
        SubscribeLocalEvent<StatusEffectsComponent, PickupAttemptEvent>(RelayEvent);
        SubscribeLocalEvent<StatusEffectsComponent, IsEquippingAttemptEvent>(RelayEvent);
        SubscribeLocalEvent<StatusEffectsComponent, IsUnequippingAttemptEvent>(RelayEvent);

        InitializeActivation();
        InitializeEffects();
    }

    #region Events
    /// <summary>
    /// The entire stat effect economy will collapse without this.
    /// </summary>
    private void OnStartup(EntityUid uid, StatusEffectsComponent comp, ComponentStartup args)
    {
        comp.StatusContainer = _container.EnsureContainer<Container>(uid, comp.StatusContainerId);
        comp.StatusContainer.OccludesLight = false;

        foreach (var collection in comp.Whitelist)
        {
            if (PrototypeManager.TryIndex(collection, out var prototype))
            {
                foreach (var effect in prototype.Collection)
                    comp.CachedWhitelist.Add(effect);
            }
        }

        foreach (var collection in comp.Blacklist)
        {
            if (PrototypeManager.TryIndex(collection, out var prototype))
            {
                foreach (var effect in prototype.Collection)
                    comp.CachedBlacklist.Add(effect);
            }
        }
    }

    private void OnEffectStartup(EntityUid uid, StatusEffectComponent comp, ComponentStartup args)
    {
        comp.Length = Timing.CurTime + TimeSpan.FromSeconds(comp.DefaultLength);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = Timing.CurTime;

        var query = EntityQueryEnumerator<StatusEffectsComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            var args = new StatusEffectUpdateEvent();
            if (comp.NextActivation < curTime)
            {
                comp.NextActivation = curTime + TimeSpan.FromSeconds(_cfg.GetCVar(ACCCVars.StatusEffectUpdateInterval));
                RelayEvent(uid, comp, args);
            }
        }
    }

    #endregion

    #region Functions

    /// <summary>
    /// Used to apply a status effect onto an entity "The Intended Way™"
    /// </summary>
    /// <param name="uid">The player that is recieving the status effect</param>
    /// <param name="statusEffect">The prototype of the status effect being applied</param>
    /// <param name="initialStrength">How powerful is the effect when being applied as a new effect? Only used if newStrength isn't.</param>
    /// <param name="newStrength">How powerful is the effect?</param>
    /// <param name="newLength">How long should the effect last</param>
    /// <param name="addOn">Should this add strength to the entity or simply just override it?</param>
    /// <param name="overrideEffect">If true, and there is already an effect present, it will be overwritten</param>
    /// <param name="comp"></param>
    public EntityUid? ApplyEffect(
        EntityUid uid,
        string effect,
        int initialStrength = 1,
        int? newStrength = null,
        TimeSpan? newLength = null,
        StatusEffectApplicationType applyType = StatusEffectApplicationType.Add,
        StatusEffectsComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return null;

        if (comp.StatusContainer == null)
            return null;

        if (!CanApplyEffect(uid, effect, comp))
            return null;

        if (!PrototypeManager.HasIndex<EntityPrototype>(effect))
        {
            Log.Error($"Entity prototype of '{effect}' could not be found.");
            return null;
        }

        if (TryGetStatusEffect(uid, effect, out var storedEffect, comp))
        {
            ModifyEffect(storedEffect!.Value, newStrength, newLength, applyType);
            return storedEffect;
        }

        var effectEnt = Spawn(effect, Transform(uid).Coordinates);
        EnsureComp<StatusEffectComponent>(effectEnt).Owner = uid;
        ModifyEffect(effectEnt, newStrength ?? initialStrength, newLength, StatusEffectApplicationType.Override);

        RaiseLocalEvent(effectEnt, new StatusEffectOnApplicationEvent(uid));

        _container.Insert(effectEnt, comp.StatusContainer);

        return effectEnt;
    }

    /// <summary>
    /// For legacy code. Shouldn't be used otherwise.
    /// </summary>
    public EntityUid? ApplyEffect(
        EntityUid uid,
        string effect,
        int initialStrength = 1,
        int? newStrength = null,
        TimeSpan? newLength = null,
        bool refresh = false)
    {
        if (refresh)
            return ApplyEffect(uid, effect, initialStrength, newStrength, newLength, StatusEffectApplicationType.Override);
        return ApplyEffect(uid, effect, initialStrength, newStrength, newLength, StatusEffectApplicationType.Add);
    }

    /// <summary>
    /// Remove an effect. Returns true if it was successful.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="effect"></param>
    /// <param name="comp"></param>
    /// <returns></returns>
    public bool TryRemoveEffect(
        EntityUid uid,
        string effect,
        StatusEffectsComponent? comp = null)
    {
        if (TryGetStatusEffect(uid, effect, out var storedEffect, comp))
        {
            // Straight up delete it lmao.
            QueueDel(storedEffect);
            return true;
        }
        return false;
    }


    /// <summary>
    /// Should only be used on an already applied effect. For properly applying effects, see ApplyEffect()
    /// </summary>
    /// <param name="uid">The effect UID</param>
    /// <param name="newStrength">What should be the new size of the effect?</param>
    /// <param name="newLength">Should the effect have a different length?</param>
    /// <param name="overrideEffect">Should the current effect settings be overwritten despite it's strength/length</param>
    /// <param name="comp"></param>
    public void ModifyEffect(
        EntityUid uid,
        int? newStrength = null,
        TimeSpan? newLength = null,
        StatusEffectApplicationType applyType = StatusEffectApplicationType.Override,
        StatusEffectComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        switch (applyType)
        {
            case StatusEffectApplicationType.Add:
                comp.OverallStrength = Math.Clamp(comp.OverallStrength + (newStrength ?? 0), 0, comp.MaxStrength);
                comp.Length += newLength ?? TimeSpan.Zero;
                break;
            case StatusEffectApplicationType.UseStrongest:
                comp.OverallStrength = Math.Clamp(newStrength ?? 0, comp.OverallStrength, comp.MaxStrength);
                comp.Length += (newLength ?? TimeSpan.Zero) / (1 / (comp.OverallStrength - (newStrength ?? comp.OverallStrength) + 1));
                break;
            case StatusEffectApplicationType.Override:
                comp.OverallStrength = Math.Clamp(newStrength ?? comp.OverallStrength, 0, comp.MaxStrength);
                comp.Length = newLength ?? comp.Length;
                break;
        }

        RaiseLocalEvent(uid, new StatusEffectModifiedEvent());

        if (comp.OverallStrength <= 0)
            QueueDel(uid);
    }

    /// <summary>
    /// Can the effect be applied onto the selected entity?
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="effect"></param>
    /// <param name="comp"></param>
    /// <returns></returns>
    public bool CanApplyEffect(EntityUid uid, string effect, StatusEffectsComponent? comp = null)
    {
        // Cannot apply effects if the entity cannot have effects in the first place!
        if (!Resolve(uid, ref comp))
            return false;

        // If the effect isn't on our *existing* whitelist, nuh uh!
        if (comp.CachedWhitelist.Count > 0 && !comp.CachedWhitelist.Contains(effect))
            return false;

        // If the effect is on our blacklist, nuh uh!
        if (comp.CachedBlacklist.Contains(effect))
            return false;

        return true;
    }
    /// <summary>
    /// Does the entity have the specified effect?
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="effect"></param>
    /// <param name="comp"></param>
    /// <returns></returns>

    public bool HasStatusEffect(EntityUid uid, string effect, StatusEffectsComponent? comp = null)
    {
        // Well obviously.
        if (!Resolve(uid, ref comp))
            return false;

        // No container? Likely no status effects.
        if (comp.StatusContainer == null)
            return false;

        if (!PrototypeManager.TryIndex<EntityPrototype>(effect, out var statusPrototype))
        {
            Log.Error($"Entity prototype of '{effect}' could not be found.");
            return false;
        }

        foreach (var storedEffect in comp.StatusContainer.ContainedEntities)
        {
            if (HasComp<StatusEffectComponent>(storedEffect)
            && TryComp<MetaDataComponent>(storedEffect, out var metaData) && metaData.EntityPrototype == statusPrototype)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Does the entity have an effect with the tag?
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="tag"></param>
    /// <param name="comp"></param>
    /// <returns></returns>
    public bool HasStatusEffectWithTag(EntityUid uid, string tag, StatusEffectsComponent? comp = null)
    {
        // Well obviously.
        if (!Resolve(uid, ref comp))
            return false;

        // No container? Likely no status effects.
        if (comp.StatusContainer == null)
            return false;

        if (!PrototypeManager.HasIndex<TagPrototype>(tag))
        {
            Log.Error($"Tag prototype of '{tag}' could not be found.");
            return false;
        }

        foreach (var storedEffect in comp.StatusContainer.ContainedEntities)
        {
            if (HasComp<StatusEffectComponent>(storedEffect) && _tagSystem.HasTag(uid, tag))
                return true;
        }

        return false;
    }

    public bool TryGetStatusEffect(EntityUid uid, string effect, out EntityUid? effectUid, StatusEffectsComponent? comp = null)
    {
        effectUid = null;
        // Well obviously.
        if (!Resolve(uid, ref comp))
            return false;

        // No container? Likely no status effects.
        if (comp.StatusContainer == null)
            return false;

        if (!PrototypeManager.TryIndex<EntityPrototype>(effect, out var statusPrototype))
        {
            Log.Error($"Entity prototype of '{effect}' could not be found.");
            return false;
        }

        foreach (var storedEffect in comp.StatusContainer.ContainedEntities)
        {
            if (TryComp<StatusEffectComponent>(storedEffect, out var statusEffectComp)
            && TryComp<MetaDataComponent>(storedEffect, out var metaData) && metaData.EntityPrototype == statusPrototype)
            {
                effectUid = storedEffect;
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Relays

    /// <summary>
    /// Used to relay an event that an entity recieved into it's effects so that the event can be modified by the effects.
    /// </summary>
    private void RelayEvent<TEvent>(EntityUid uid, StatusEffectsComponent comp, TEvent args)
    {
        var relayedArgs = new StatusEffectRelayEvent<TEvent>(args, uid);

        if (comp.StatusContainer == null)
            return;

        foreach (var effect in comp.StatusContainer.ContainedEntities)
        {
            RaiseLocalEvent(effect, relayedArgs);
        }
    }

    /// <summary>
    /// A way to relay an event without StatusEffectRelayEvent. This doesn't send over the owner entity.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="comp"></param>
    /// <param name="args"></param>
    private void RelayPureEvent(EntityUid uid, StatusEffectsComponent comp, object args)
    {
        if (comp.StatusContainer == null)
            return;

        foreach (var effect in comp.StatusContainer.ContainedEntities)
        {
            RaiseLocalEvent(effect, args);
        }
    }

    /// <summary>
    /// A ref version of RelayEvent. Does the same thing as it.
    /// </summary>
    private void RefRelayEvent<TEvent>(EntityUid uid, StatusEffectsComponent comp, ref TEvent args)
    {
        var relayedArgs = new StatusEffectRelayEvent<TEvent>(args, uid);

        if (comp.StatusContainer == null)
            return;

        foreach (var effect in comp.StatusContainer.ContainedEntities)
        {
            RaiseLocalEvent(effect, relayedArgs);
        }
    }

    /// <summary>
    /// A ref version of RelayPureEvent. Does the same thing as it.
    /// </summary>
    private void RefRelayPureEvent(EntityUid uid, StatusEffectsComponent comp, ref object args)
    {
        if (comp.StatusContainer == null)
            return;

        foreach (var effect in comp.StatusContainer.ContainedEntities)
        {
            RaiseLocalEvent(effect, args);
        }
    }

    #endregion
}

public enum StatusEffectApplicationType
{
    Add, // Add both.
    UseStrongest, // Use the strongest effect, then add more time. Weaken the length of the time if applying a weaker effect.
    Override, // Override all.
}
