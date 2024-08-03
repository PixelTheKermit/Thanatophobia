using Content.Shared.Research.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Thanatophobia.Blueprints;

[RegisterComponent]
[NetworkedComponent]
public sealed partial class UsesMemoryBlueprintsComponent : Component
{
    [DataField]
    public List<ProtoId<LatheRecipePrototype>> Recipes = new();

    [DataField]
    public SoundSpecifier? SyncSound = null;
}
