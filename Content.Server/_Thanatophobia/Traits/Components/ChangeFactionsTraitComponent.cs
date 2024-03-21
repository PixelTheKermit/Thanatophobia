using Content.Server.NPC.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Thanatophobia.Traits;

[RegisterComponent]
public sealed partial class ChangeFactionsTraitComponent : Component
{
    [DataField]
    public List<ProtoId<NpcFactionPrototype>> Add = new();

    [DataField]
    public List<ProtoId<NpcFactionPrototype>> Remove = new();
}
