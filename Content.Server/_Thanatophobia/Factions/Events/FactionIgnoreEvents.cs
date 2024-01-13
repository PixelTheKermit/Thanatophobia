namespace Content.Server.Thanatophobia.Factions;

public sealed partial class UserFactionIgnoreEvent : EntityEventArgs
{
    public bool Ignore;
    public EntityUid Target;
    public UserFactionIgnoreEvent(EntityUid target)
    {
        Target = target;
    }
}

public sealed partial class TargetFactionIgnoreEvent : EntityEventArgs
{
    public bool Ignore;
    public EntityUid User;
    public TargetFactionIgnoreEvent(EntityUid user)
    {
        User = user;
    }
}
