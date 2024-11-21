using Content.Shared.Damage;

namespace Content.Shared.StatusEffect;
[RegisterComponent]
public sealed partial class PreventMovementEffectComponent : Component
{
    [DataField]
    public int StrengthNeeded = 1;

    // I'm so fucking tired of this shit
    // Okay this is useless now, but I want to leave this here as a remnant of my FURY.
    public bool TheFuckThisBoolean = false;
}
