using Content.Shared.Damage;

namespace Content.Shared.StatusEffect;
[RegisterComponent]
public sealed partial class PreventMovementEffectComponent : Component
{
    [DataField]
    public int StrengthNeeded = 1;
}
