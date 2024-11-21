

using Robust.Shared.Serialization;

namespace Content.Shared.StatusEffect;

/// <summary>
/// Raised on the owner when the effect is initially inflicted.
/// </summary>
public sealed class OwnerStatusEffectOnApply : EntityEventArgs
{
    public EntityUid Effect;

    public OwnerStatusEffectOnApply(EntityUid effect)
    {

        Effect = effect;
    }
}

/// <summary>
/// Raised when the effect is initially inflicted.
/// </summary>
public sealed class StatusEffectOnApplicationEvent : EntityEventArgs
{
    public EntityUid Victim;

    public StatusEffectOnApplicationEvent(EntityUid victim)
    {

        Victim = victim;
    }
}

/// <summary>
/// Raised when the effect wears off on the entity.
/// </summary>
public sealed class OwnerOnStatusEffectShutdown : EntityEventArgs
{
    public EntityUid Effect;

    public OwnerOnStatusEffectShutdown(EntityUid effect)
    {
        Effect = effect;
    }
}

/// <summary>
/// Raised when the effect is being modified.
/// </summary>
[ByRefEvent]
public sealed class StatusEffectModifyEvent : EntityEventArgs
{
    public int? Strength;
    public TimeSpan? Length;
    public readonly StatusEffectApplicationType ApplyType;

    public StatusEffectModifyEvent(int? strength, TimeSpan? length, StatusEffectApplicationType applyType)
    {
        Strength = strength;
        Length = length;
        ApplyType = applyType;
    }
}

/// <summary>
/// Raised on the user when the effect is being modified.
/// </summary>
[ByRefEvent]
public sealed class OwnerStatusEffectModifyEvent : EntityEventArgs
{
    public EntityUid Effect;
    public int? Strength;
    public TimeSpan? Length;
    public readonly StatusEffectApplicationType ApplyType;
    public OwnerStatusEffectModifyEvent(EntityUid effect, int? strength, TimeSpan? length, StatusEffectApplicationType applyType)
    {
        Effect = effect;
        Strength = strength;
        Length = length;
        ApplyType = applyType;
    }
}

/// <summary>
/// Raised after the effect has been modified.
/// </summary>
public sealed class StatusEffectModifiedEvent : EntityEventArgs { }

/// <summary>
/// Raised every second and a half by default.
/// </summary>
public sealed class StatusEffectUpdateEvent : EntityEventArgs { }

/// <summary>
/// This is used to carry over the original event's entity.
/// </summary>
public sealed class StatusEffectRelayEvent<TEvent> : EntityEventArgs
{
    public TEvent Args;
    public EntityUid Victim;

    public StatusEffectRelayEvent(TEvent args, EntityUid victim)
    {
        Args = args;
        Victim = victim;
    }
}

/// <summary>
/// For when an effect expires.
/// </summary>
public sealed class StatusEffectTimeoutEvent : EntityEventArgs
{
    public EntityUid Victim;

    public StatusEffectTimeoutEvent(EntityUid victim)
    {
        Victim = victim;
    }
}

/// <summary>
/// Used for effects that need to be activated.
/// </summary>
public sealed class StatusEffectActivateEvent : EntityEventArgs
{
    public EntityUid Victim;
    public StatusEffectActivateEvent(EntityUid victim)
    {
        Victim = victim;
    }
}

#region Network Events

/// <summary>
/// Raised when the effect is initially inflicted, and the client needs to know.
/// </summary>
[Serializable, NetSerializable]
public sealed class ClientStatusEffectOnApplicationEvent : EntityEventArgs
{
    public NetEntity Effect;

    public ClientStatusEffectOnApplicationEvent(NetEntity effect)
    {

        Effect = effect;
    }
}

/// <summary>
/// Raised after the effect has been modified, and the client needs to know.
/// </summary>
[Serializable, NetSerializable]
public sealed class ClientStatusEffectModifiedEvent : EntityEventArgs
{
    public NetEntity Effect;
    public ClientStatusEffectModifiedEvent(NetEntity effect)
    {
        Effect = effect;
    }
}

#endregion

public sealed class ForceSayOnApplyEffectEvent : EntityEventArgs { }
