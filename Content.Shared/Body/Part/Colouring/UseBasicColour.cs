using Robust.Shared.Serialization;

namespace Content.Shared.Body.Part;

[NetSerializable, Serializable]
public sealed partial class PartUseBasicColour : PartColouringType
{
    [DataField]
    public Color Colour = Color.White;

    public override Color GetColour(Color skinColour, Color eyeColour)
    {
        return Colour;
    }
}
