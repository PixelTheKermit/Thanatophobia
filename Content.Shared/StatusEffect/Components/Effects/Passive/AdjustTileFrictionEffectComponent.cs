namespace Content.Shared.StatusEffect;
[RegisterComponent]
public sealed partial class AdjustTileFrictionEffectComponent : Component
{
    [DataField]
    public float Multiplier = 0.4f;
}
