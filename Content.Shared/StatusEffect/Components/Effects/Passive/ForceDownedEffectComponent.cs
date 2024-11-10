using Content.Shared.Damage;

namespace Content.Shared.StatusEffect;
[RegisterComponent]
public sealed partial class ForcedDownedEffectComponent : Component
{
    [DataField]
    public int StrengthNeeded = 1;
}
