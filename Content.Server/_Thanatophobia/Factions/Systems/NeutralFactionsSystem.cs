using System.Linq;
using Content.Server.NPC.Components;
using Content.Server.NPC.Systems;
using Content.Shared.Damage;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server.Thanatophobia.Factions;
public sealed partial class NeutralFactionsSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NeutralFactionsComponent, UserFactionIgnoreEvent>(OnFactionIgnore);

        SubscribeLocalEvent<NeutralFactionsComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnFactionIgnore(EntityUid uid, NeutralFactionsComponent comp, UserFactionIgnoreEvent args)
    {
        if (comp.HasHurt.Any(x => x == args.Target))
            return;

        if (!TryComp<NpcFactionMemberComponent>(args.Target, out var factionComp))
            return;

        foreach (var faction in comp.Neutral)
        {
            if (factionComp.Factions.Any(x => x == faction))
            {
                args.Ignore = true;
                return;
            }
        }
    }

    private void OnDamageChanged(EntityUid uid, NeutralFactionsComponent comp, DamageChangedEvent args)
    {
        if (args.Origin == null || comp.HasHurt.Any(x => x == args.Origin))
            return;

        if (!TryComp<NpcFactionMemberComponent>(args.Origin, out var factionComp))
            return;

        if (!args.DamageIncreased)
            return;

        comp.HasHurt.Add(args.Origin.Value);
    }
}
