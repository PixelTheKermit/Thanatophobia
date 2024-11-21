using Content.Shared.Speech.EntitySystems;
using Content.Shared.StatusEffect;
using Content.Shared.Traits.Assorted;
using Robust.Shared.Timing;

namespace Content.Shared.Drunk;

public abstract class SharedDrunkSystem : EntitySystem
{
    [Dependency] private readonly SharedStatusEffectsSystem _statusSystem = default!;
    [Dependency] private readonly IGameTiming _gameTimer = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LightweightDrunkComponent, OwnerStatusEffectModifyEvent>(StatusModify);
    }

    private void StatusModify(EntityUid uid, LightweightDrunkComponent comp, ref OwnerStatusEffectModifyEvent args)
    {
        if (args.Length != null && args.Length > TimeSpan.Zero)
            args.Length = args.Length.Value * comp.BoozeStrengthMultiplier;
    }
}
