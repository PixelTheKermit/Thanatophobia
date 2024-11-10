using Content.Shared.StatusEffect;

namespace Content.Shared.Speech.EntitySystems;

public abstract class SharedStutteringSystem : EntitySystem
{
    [Dependency] private readonly SharedStatusEffectsSystem _statusEffectsSystem = default!;

    // For code in shared... I imagine we ain't getting accent prediction anytime soon so let's not bother.
    public virtual void DoStutter(EntityUid uid, TimeSpan time, bool refresh, StatusEffectsComponent? status = null)
    {
    }

    public virtual void DoRemoveStutterTime(EntityUid uid, double timeRemoved)
    {
        _statusEffectsSystem.ApplyEffect(uid, "Stutter", 0, null, -TimeSpan.FromSeconds(timeRemoved), StatusEffectApplicationType.Add);
    }

    public void DoRemoveStutter(EntityUid uid, double timeRemoved)
    {
        _statusEffectsSystem.ApplyEffect(uid, "Stutter", 0, 0, null, StatusEffectApplicationType.Override);
    }
}
