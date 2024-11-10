using Content.Shared.StatusIcon;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.StatusEffect;

[RegisterComponent]
public sealed partial class StatusEffectIconComponent : Component
{
    [DataField("statusIcon", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<StatusIconPrototype>))]
    public string StatusIcon = string.Empty;
}
