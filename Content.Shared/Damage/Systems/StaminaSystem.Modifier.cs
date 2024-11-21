using Content.Shared.Damage.Components;
using Content.Shared.StatusEffect;

namespace Content.Shared.Damage.Systems;

public sealed partial class StaminaSystem
{
    private void InitializeModifier()
    {
        SubscribeLocalEvent<StaminaModifierComponent, StatusEffectModifiedEvent>(OnModifierStartup);
        SubscribeLocalEvent<StaminaModifierComponent, ComponentShutdown>(OnModifierShutdown);
    }

    private void OnModifierStartup(EntityUid uid, StaminaModifierComponent comp, StatusEffectModifiedEvent args)
    {
        if (comp.AlreadyApplied)
            return;

        if (!TryComp<StatusEffectComponent>(uid, out var statusEffect) || statusEffect.Owner == null)
            return;

        if (!TryComp<StaminaComponent>(statusEffect.Owner.Value, out var stamina))
            return;

        stamina.CritThreshold *= comp.Modifier;
    }

    private void OnModifierShutdown(EntityUid uid, StaminaModifierComponent comp, ComponentShutdown args)
    {
        if (!comp.AlreadyApplied)
            return;

        if (!TryComp<StatusEffectComponent>(uid, out var statusEffect) || statusEffect.Owner == null)
            return;

        if (!TryComp<StaminaComponent>(statusEffect.Owner.Value, out var stamina))
            return;

        stamina.CritThreshold /= comp.Modifier;
    }

    /// <summary>
    /// Change the stamina modifier for an entity.
    /// If it has <see cref="StaminaComponent"/> it will also be updated.
    /// </summary>
    public void SetModifier(EntityUid uid, float modifier, StaminaComponent? stamina = null, StaminaModifierComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        var old = comp.Modifier;

        if (old.Equals(modifier))
            return;

        comp.Modifier = modifier;
        Dirty(uid, comp);

        if (Resolve(uid, ref stamina, false))
        {
            // scale to the new threshold, act as if it was removed then added
            stamina.CritThreshold *= modifier / old;
        }
    }
}
