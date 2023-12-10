using Content.Shared.Body.Part;
using Robust.Shared.Prototypes;

namespace Content.Shared.Thanatophobia.Traits;

[RegisterComponent]
public sealed partial class ReplaceBodyPartTraitComponent : Component
{
    [DataField]
    public List<(string container, ProtoId<EntityPrototype>? protoId)> Replace = new();
}
