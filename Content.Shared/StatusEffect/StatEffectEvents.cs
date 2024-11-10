

namespace Content.Shared.StatusEffect;

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
/// Raised when the effect is modified.
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
/// For when an effect loses all it's length.
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



public sealed class ForceSayOnApplyEffectEvent : EntityEventArgs { }
