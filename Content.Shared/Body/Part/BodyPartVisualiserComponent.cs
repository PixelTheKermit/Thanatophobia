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
    public Dictionary<string, List<BodyPartVisualiserSprite>> Sprites = new();

    /// <summary>
    /// A dictionary for custom sprites. Used by markings that may replace the base body parts.
    /// </summary>
    [DataField]
    public Dictionary<string, List<BodyPartVisualiserSprite>> CustomSprites = new();

    /// <summary>
    /// A list of markings. Wow.
    /// This is for stuff like gradients, tattoos, or hair.
    /// </summary>
    [DataField]
    public MarkingSet Markings = new();
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class BodyPartVisualiserSprite
{
    [DataField]
    public Color? Colour;

    [DataField]
    public PartColouringType? ColouringType = null;

    [DataField]
    public SpriteSpecifier? Sprite;

    [DataField]
    public string? ID;
}
