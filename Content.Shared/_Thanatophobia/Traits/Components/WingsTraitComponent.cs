using Content.Shared.Humanoid.Markings;
using Robust.Shared.Prototypes;

namespace Content.Shared.Thanatophobia.Traits;

[RegisterComponent]
public sealed partial class WingsTraitComponent : Component
{
    [DataField]
    public float WeightlessAcceleration = 1f;

    [DataField]
    public float WeightlessFriction = 1f;

    [DataField]
    public float WeightlessModifier = 1f;
}
