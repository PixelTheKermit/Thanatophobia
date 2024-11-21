using Content.Server.StatusEffect;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.StatusEffect;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.Chemistry.ReagentEffects.StatusEffects
{
    /// <summary>
    ///     Adds a generic status effect to the entity,
    ///     not worrying about things like how to affect the time it lasts for
    ///     or component fields or anything. Just adds a component to an entity
    ///     for a given time. Easy.
    /// </summary>
    /// <remarks>
    ///     Can be used for things like adding accents or something. I don't know. Go wild.
    /// </remarks>
    [UsedImplicitly]
    public sealed partial class GenericStatusEffect : ReagentEffect
    {
        [DataField(required: true)]
        public string EffectId = default!;

        [DataField]
        public int? Strength = null;

        [DataField]
        public TimeSpan? Length = null;

        [DataField]
        public StatusEffectApplicationType Type = StatusEffectApplicationType.UseStrongest;

        public override void Effect(ReagentEffectArgs args)
        {
            var statusSys = args.EntityManager.EntitySysManager.GetEntitySystem<StatusEffectsSystem>();

            var strength = (Strength != null) ? (int?) Math.Ceiling(Strength.Value * args.Scale) : null;
            var time = Length * args.Scale ?? null;

            statusSys.ApplyEffect(args.SolutionEntity, EffectId, 0, strength, time, Type);
        }

        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) => Loc.GetString(
            "reagent-effect-guidebook-status-effect",
            ("chance", Probability),
            ("type", Type),
            ("time", (Length != null) ? Length.Value.Seconds : 0),
            ("key", $"reagent-effect-status-effect-{EffectId}"));
    }
}
