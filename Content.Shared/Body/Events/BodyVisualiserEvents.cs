using Content.Shared.Body.Part;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Robust.Shared.Prototypes;

namespace Content.Shared.Body;

[ByRefEvent]
public sealed partial class GetBodyPartVisualEvent : EntityEventArgs
{
    public List<(string, List<BodyPartVisualiserSprite>)> Sprites = new();
    public Color SkinColour;
    public Color EyeColour;
    public bool OverrideColours;

    public GetBodyPartVisualEvent(Color skinColour, Color eyeColour, bool overrideColours)
    {
        OverrideColours = overrideColours;
        SkinColour = skinColour;
        EyeColour = eyeColour;
    }
}

[ByRefEvent]
public sealed partial class GetMarkingVisualEvent : EntityEventArgs
{
    public List<(string, List<BodyPartVisualiserSprite>)> Sprites = new();
    public Color SkinColour;
    public Color EyeColour;
    public bool OverrideColours;

    public GetMarkingVisualEvent(Color skinColour, Color eyeColour, bool overrideColours)
    {
        OverrideColours = overrideColours;
        SkinColour = skinColour;
        EyeColour = eyeColour;
    }
}

[ByRefEvent]
public sealed partial class RemoveMarkingByIDEvent : EntityEventArgs
{
    public ProtoId<MarkingPrototype> ID;

    public RemoveMarkingByIDEvent(ProtoId<MarkingPrototype> markingID)
    {
        ID = markingID;
    }
}

public sealed partial class ClearPartMarkingsEvent : EntityEventArgs
{
    public ClearPartMarkingsEvent()
    {
    }
}

[ByRefEvent]
public sealed partial class GetPartMarkingsEvent : EntityEventArgs
{
    public MarkingSet Markings = new();
    public GetPartMarkingsEvent()
    {
    }
}
public sealed partial class UpdateGenderedBodyPartEvent : EntityEventArgs
{
    public Sex Sex;
    public UpdateGenderedBodyPartEvent(Sex sex)
    {
        Sex = sex;
    }
}
