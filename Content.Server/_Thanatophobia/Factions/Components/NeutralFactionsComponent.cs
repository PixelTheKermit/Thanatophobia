
using Content.Server.NPC.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Thanatophobia.Factions;
[RegisterComponent]
public sealed partial class NeutralFactionsComponent : Component
{
    [DataField]
    public List<ProtoId<NpcFactionPrototype>> Neutral = new();

    [DataField]
    public List<EntityUid> HasHurt = new();
}
