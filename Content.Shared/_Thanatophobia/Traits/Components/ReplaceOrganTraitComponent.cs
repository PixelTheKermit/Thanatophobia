using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared.Thanatophobia.Traits;

[RegisterComponent]
public sealed partial class ReplaceOrganTraitComponent : Component
{
    [DataField]
    public List<(string container, ProtoId<EntityPrototype>? protoId)> Replace = new();
}
