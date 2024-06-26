using Content.Shared.Research.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Thanatophobia.Blueprints;

[RegisterComponent]
public sealed partial class BlueprintModuleComponent : Component
{
    [DataField]
    public List<ProtoId<LatheRecipePrototype>> Recipes = new();
}
