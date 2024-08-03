using Content.Shared.Research.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared.Thanatophobia.Blueprints;

[RegisterComponent]
public sealed partial class DigitalMemoryBlueprintComponent : Component
{
    [DataField]
    public List<ProtoId<LatheRecipePrototype>> Recipes = new();

    [DataField]
    public SoundSpecifier? Sound = null;

    [DataField]
    public string PopupString = "tp-blueprint-digital-memory-used";
}
