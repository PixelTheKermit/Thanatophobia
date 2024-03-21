using Robust.Shared.Serialization;

namespace Content.Shared.Body.Part;

[NetSerializable, Serializable]
[ImplicitDataDefinitionForInheritors]
public abstract partial class PartColouringType
{
    public abstract Color GetColour(Color skin, Color eyes);
}

