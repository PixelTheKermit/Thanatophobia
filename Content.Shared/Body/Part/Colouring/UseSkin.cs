using Robust.Shared.Serialization;

namespace Content.Shared.Body.Part;

[NetSerializable, Serializable]
public sealed partial class PartUseSkin : PartColouringType
{
    public override Color GetColour(Color skinColour, Color eyeColour)
    {
        return skinColour;
    }
}
