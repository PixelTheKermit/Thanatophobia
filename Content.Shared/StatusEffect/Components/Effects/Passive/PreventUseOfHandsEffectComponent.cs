namespace Content.Shared.StatusEffect;
[RegisterComponent]
public sealed partial class PreventUseOfHandsEffectComponent : Component
{
    [DataField]
    public int StrengthNeeded = 1;
}
