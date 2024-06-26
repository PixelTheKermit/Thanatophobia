using Content.Server.NPC.Components;
using Content.Server.NPC.Systems;

namespace Content.Server.Thanatophobia.Traits;

public sealed partial class ChangeFactionsTraitSystem : EntitySystem
{
    [Dependency] private readonly NpcFactionSystem _factionSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangeFactionsTraitComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, ChangeFactionsTraitComponent comp, ComponentStartup args)
    {
        if (!TryComp<NpcFactionMemberComponent>(uid, out var factionComp))
            return;

        foreach (var faction in comp.Add)
        {
            _factionSystem.AddFaction(uid, faction);
        }

        foreach (var faction in comp.Remove)
        {
            _factionSystem.RemoveFaction(uid, faction);
        }
    }
}
