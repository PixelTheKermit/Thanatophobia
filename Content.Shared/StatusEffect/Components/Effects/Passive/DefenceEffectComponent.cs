using Content.Shared.Damage;

namespace Content.Shared.StatusEffect;
[RegisterComponent]
public sealed partial class DefenceEffectComponent : Component
{
    [DataField("modifiers", required: true)]
    public DamageModifierSet Modifiers = default!;
}

