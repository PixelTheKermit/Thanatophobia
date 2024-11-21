namespace Content.Shared.StatusEffect;
[RegisterComponent]
public sealed partial class ForceSleepingEffectComponent : Component
{
    [DataField]
    public int StrengthNeeded = 1;
}
