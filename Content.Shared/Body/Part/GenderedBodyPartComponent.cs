using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Body.Part;

[RegisterComponent, NetworkedComponent]
public sealed partial class GenderedBodyPartComponent : Component
{
    [DataField]
    public Dictionary<Sex, BodyPartVisualiserSet> Sprites = new();
}
