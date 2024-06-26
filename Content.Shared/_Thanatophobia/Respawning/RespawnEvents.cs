using Robust.Shared.Serialization;

namespace Content.Shared.Thanatophobia.Respawning;
[Serializable, NetSerializable]
public sealed class SimpleRespawnUIMessage : EntityEventArgs
{
    public SimpleRespawnUIMessage()
    {
    }
}
