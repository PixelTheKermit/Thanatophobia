using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

// don't worry about it

namespace Content.Shared.Traits
{
    /// <summary>
    ///     Describes a trait.
    /// </summary>
    [Prototype("trait")]
    public sealed partial class TraitPrototype : IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = default!;

        /// <summary>
        ///     The name of this trait.
        /// </summary>
        [DataField("name")]
        public string Name { get; private set; } = "";

        /// <summary>
        ///     The description of this trait.
        /// </summary>
        [DataField("description")]
        public string? Description { get; private set; }

        /// <summary>
        ///     Don't apply this trait to entities this whitelist IS NOT valid for.
        /// </summary>
        [DataField("whitelist")]
        public EntityWhitelist? Whitelist;

        /// <summary>
        ///     Don't apply this trait to entities this whitelist IS valid for. (hence, a blacklist)
        /// </summary>
        [DataField("blacklist")]
        public EntityWhitelist? Blacklist;

        /// <summary>
        ///     The components that get added to the player, when they pick this trait.
        /// </summary>
        [DataField("components")]
        public ComponentRegistry Components { get; private set; } = default!;

        /// <summary>
        ///     Gear that is given to the player, when they pick this trait.
        /// </summary>
        [DataField("traitGear", required: false, customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? TraitGear;

        # region Thanatophobia edits start here.

        /// <summary>
        /// The overall cost of the trait. Traits with positive costs will be positive. Traits with negative costs will give points yet be negative.
        /// Traits with 0 cost will be considered "Neutral/RP traits", and will not take a trait slot upon equipping.
        /// </summary>
        [DataField("cost")]
        public int Cost = 0;

        /// <summary>
        /// What "tags" does the species prototype need to use this trait?
        /// </summary>
        /// <returns></returns>
        [DataField]
        public List<string> Allowed = new()
        {
            "Organic", // Stuff organic in here for now. Will probably make things easier in the long term.
        };

        /// <summary>
        /// For mutually exclusive traits. (For example: 2 speed modifier traits.)
        /// </summary>
        /// <returns></returns>
        [DataField]
        public List<string> Exclusive = new();

        # endregion Thanatophobia edits end here.


    }
}
