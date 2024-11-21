using Content.Shared.Alert;

namespace Content.Shared.StatusEffect;
[RegisterComponent]
public sealed partial class AlertEffectComponent : Component
{
    [DataField(required: true)]
    public AlertType Alert = default!;
}
public sealed partial class AlertEffectGoneEv : EntityEventArgs { }
