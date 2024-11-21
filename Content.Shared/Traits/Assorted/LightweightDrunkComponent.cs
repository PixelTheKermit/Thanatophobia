using Robust.Shared.GameStates;
using Content.Shared.Drunk;

namespace Content.Shared.Traits.Assorted;

/// <summary>
/// Used for the lightweight trait. DrunkSystem will check for this component and modify the boozePower accordingly if it finds it.
/// TODO: I'm feeling kinda lazy. Fix this later.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedDrunkSystem))]
public sealed partial class LightweightDrunkComponent : Component
{
    [DataField("boozeStrengthMultiplier"), ViewVariables(VVAccess.ReadWrite)]
    public float BoozeStrengthMultiplier = 4f;
}
