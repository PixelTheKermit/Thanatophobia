using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.StatusEffect
{
    [RegisterComponent]
    [NetworkedComponent]
    [Access(typeof(SharedStatusEffectsSystem))]
    public sealed partial class StatusEffectsComponent : Component
    {
        /// <summary>
        ///     The ID for the status effect container. Realistically doesn't need to be changed, but you have the option to do so if you *really* need it.
        /// </summary>
        [DataField]
        public string StatusContainerId = "status-effect-container";

        /// <summary>
        ///     The container of all the entity's status effects.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public Container? StatusContainer = default!;

        /// <summary>
        ///     When should the next "tick" occur?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public TimeSpan NextActivation = TimeSpan.Zero;

        /// <summary>
        ///     A whitelist, in case you only want specific effects on this entity. Uses StatusEffectCollectionPrototype.
        /// </summary>
        [DataField]
        public List<ProtoId<StatusEffectCollectionPrototype>> Whitelist = new();

        /// <summary>
        ///     A blacklist for this entity, which blocks the entity from recieving certain effects. Uses StatusEffectCollectionPrototype.
        /// </summary>
        [DataField]
        public List<ProtoId<StatusEffectCollectionPrototype>> Blacklist = new();

        public HashSet<string> CachedWhitelist = new();
        public HashSet<string> CachedBlacklist = new();
    }
}
