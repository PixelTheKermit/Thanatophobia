using Robust.Shared.Serialization;

namespace Content.Shared.LateJoin;

[Serializable, NetSerializable]
public sealed class GetLateJoinTypeUIMessage : EntityEventArgs
{
}

public sealed class GetLateJoinTypeEvent : EntityEventArgs
{
    public LateJoinType LateType = LateJoinType.Default;
    public Dictionary<string, object> Data = new();

    public bool Handled => LateType != LateJoinType.Default;
}


[Serializable, NetSerializable]
public sealed class GiveLateJoinTypeUIMessage : EntityEventArgs
{
    public LateJoinType LateType;

    public Dictionary<string, object> Data;

    public GiveLateJoinTypeUIMessage(LateJoinType lateType, Dictionary<string, object> data)
    {
        LateType = lateType;
        Data = data;
    }
}

public enum LateJoinType
{
    Default,
    SpawnShips
}
