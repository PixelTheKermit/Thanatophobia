using Content.Shared.Humanoid.Markings;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Body.Part;

[RegisterComponent, NetworkedComponent]
public sealed partial class BodyPartVisualiserComponent : Component
{
    [DataField]
    public bool IsReplaceable = true;

    /// <summary>
    /// A dictionary. The key is the layer you want to use.
    /// Support for multiple layers, for things like tails which may have a front and back sprite.
    /// </summary>
    [DataField]
    public BodyPartVisualiserSet Sprites = new();

    /// <summary>
    /// A dictionary for custom sprites. Used by markings that may replace the base body parts.
    /// </summary>
    [DataField]
    public BodyPartVisualiserSet? CustomSprites;

    /// <summary>
    /// A list of markings. Wow.
    /// This is for stuff like gradients, tattoos, or hair.
    /// </summary>
    [DataField]
    public MarkingSet Markings = new();
}

[DataDefinition, NetSerializable, Serializable]
public sealed partial class BodyPartVisualiserSet
{
    [DataField]
    public Dictionary<string, List<SpriteSpecifier?>> Sprites;

    [DataField]
    public List<PartColouringType> DefaultColouring;

    [DataField]
    public List<Color> Colours;
}
