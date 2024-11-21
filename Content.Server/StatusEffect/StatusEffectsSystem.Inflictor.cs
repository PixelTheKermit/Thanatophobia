using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server.StatusEffect;

public sealed partial class StatusEffectsSystem
{
    public void InitializeInflictor()
    {
        SubscribeLocalEvent<InflictEffectOnDamageComponent, MeleeHitEvent>(InflictOnHit);
    }

    private void InflictOnHit(EntityUid uid, InflictEffectOnDamageComponent comp, MeleeHitEvent args)
    {
        foreach (var entity in args.HitEntities)
        {
            if (!TryComp<StatusEffectsComponent>(entity, out var StatusEffects))
                continue;

            ApplyEffect(entity, comp.Effect, 1, comp.Strength, TimeSpan.FromSeconds(comp.Length), comp.Replace);
        }
    }
}

