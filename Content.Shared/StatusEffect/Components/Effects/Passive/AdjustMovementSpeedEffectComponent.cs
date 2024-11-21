namespace Content.Shared.StatusEffect;
[RegisterComponent]
public sealed partial class AdjustSpeedEffectComponent : Component
{
    [DataField]
    public float WalkingSpeed = 1f;

    [DataField]
    public float SprintingSpeed = 1f;
}
