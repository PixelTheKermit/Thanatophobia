using Content.Shared.Research.Prototypes;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared.Thanatophobia.Blueprints;

[RegisterComponent]
public sealed partial class BlueprintMemoryComponent : Component
{
    [DataField]
    public Dictionary<NetUserId, List<ProtoId<LatheRecipePrototype>>> PlayerToBlueprint = new();
}
