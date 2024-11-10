using Content.Shared.Speech.EntitySystems;
using Content.Shared.StatusEffect;
using Content.Shared.Traits.Assorted;

namespace Content.Shared.Drunk;

public abstract class SharedDrunkSystem : EntitySystem
{
    [Dependency] private readonly SharedStatusEffectsSystem _statusEffectsSystem = default!;
    [Dependency] private readonly SharedSlurredSystem _slurredSystem = default!;

    public void TryApplyDrunkenness(EntityUid uid, float boozePower, bool applySlur = true,
        StatusEffectsComponent? status = null)
    {
        if (!Resolve(uid, ref status, false))
            return;

        if (TryComp<LightweightDrunkComponent>(uid, out var trait))
            boozePower *= trait.BoozeStrengthMultiplier;

        if (applySlur)
        {
            _slurredSystem.DoSlur(uid, TimeSpan.FromSeconds(boozePower), status);
        }

        _statusEffectsSystem.ApplyEffect(uid, "Drunk", 1, null, TimeSpan.FromSeconds(boozePower), StatusEffectApplicationType.Add);
    }

    public void TryRemoveDrunkenness(EntityUid uid)
    {
        _statusEffectsSystem.ApplyEffect(uid, "Drunk", 0, 0, null, StatusEffectApplicationType.Override);
    }
    public void TryRemoveDrunkenessTime(EntityUid uid, double timeRemoved)
    {
        _statusEffectsSystem.ApplyEffect(uid, "Drunk", 0, null, -TimeSpan.FromSeconds(timeRemoved), StatusEffectApplicationType.Add);
    }

}
