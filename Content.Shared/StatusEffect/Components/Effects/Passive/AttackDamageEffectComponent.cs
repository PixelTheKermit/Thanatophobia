using Content.Shared.Damage;

namespace Content.Shared.StatusEffect;
[RegisterComponent]
public sealed partial class AttackDamageEffectComponent : Component
{
    [DataField("modifiers", required: true)]
    public DamageModifierSet Modifiers = default!;
}
