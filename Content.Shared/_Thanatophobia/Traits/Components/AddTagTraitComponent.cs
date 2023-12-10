using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared.Thanatophobia.Traits;

[RegisterComponent]
public sealed partial class AddTagTraitComponent : Component
{
    [DataField]
    public List<ProtoId<TagPrototype>> Add = new();

    [DataField]
    public List<ProtoId<TagPrototype>> Remove = new();
}
