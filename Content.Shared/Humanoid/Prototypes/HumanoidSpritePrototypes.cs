using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Humanoid.Prototypes;

/// <summary>
///     Base sprites for a species (e.g., what replaces the empty tagged layer,
///     or settings per layer)
/// </summary>
[Prototype("speciesBaseSprites")]
public sealed partial class HumanoidSpeciesBaseSpritesPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     Sprites that this species will use on the given humanoid
    ///     visual layer. If a key entry is empty, it is assumed that the
    ///     visual layer will not be in use on this species, and will
    ///     be ignored.
    /// </summary>
    [DataField("sprites", required: true)]
    public Dictionary<string, string> Sprites = new();
}
