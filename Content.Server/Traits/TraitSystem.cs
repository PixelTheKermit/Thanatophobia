using System.Linq;
using Content.Server.GameTicking;
using Content.Server.Humanoid;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Tag;
using Content.Shared.Traits;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Utility;

namespace Content.Server.Traits;

public sealed class TraitSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!;
    [Dependency] private readonly SharedHandsSystem _sharedHandsSystem = default!;

    # region Thanatophobia Dependencies
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearanceSystem = default!;
    [Dependency] private readonly MarkingManager _markingManager = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    # endregion

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    // When the player is spawned in, add all trait components selected during character creation
    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        var withTraits = args.Profile.WithTraitPreference("", true);

        foreach (var traitId in withTraits.TraitPreferences)
        {
            if (!_prototypeManager.TryIndex<TraitPrototype>(traitId, out var traitPrototype))
            {
                Logger.Warning($"No trait found with ID {traitId}!");
                return;
            }

            if (traitPrototype.Whitelist != null && !traitPrototype.Whitelist.IsValid(args.Mob))
                continue;

            if (traitPrototype.Blacklist != null && traitPrototype.Blacklist.IsValid(args.Mob))
                continue;

            // Add all components required by the prototype
            foreach (var entry in traitPrototype.Components.Values)
            {
                # region Start Thanatophobia Edits
                var comp = (Component) _serializationManager.CreateCopy(entry.Component, notNullableOverride: true);
                comp.Owner = args.Mob;
                EntityManager.AddComponent(args.Mob, comp, true);
                # endregion End Thanatophobia Edits
            }

            // Add item required by the trait
            if (traitPrototype.TraitGear != null)
            {
                if (!TryComp(args.Mob, out HandsComponent? handsComponent))
                    continue;

                var coords = Transform(args.Mob).Coordinates;
                var inhandEntity = EntityManager.SpawnEntity(traitPrototype.TraitGear, coords);
                _sharedHandsSystem.TryPickup(args.Mob, inhandEntity, checkActionBlocker: false,
                    handsComp: handsComponent);
            }

            # region Start Thanatophobia Edits

            if (traitPrototype.MarkingId != null
            && TryComp<HumanoidAppearanceComponent>(args.Mob, out var appearanceComp)
            && _prototypeManager.TryIndex<MarkingPrototype>(traitPrototype.MarkingId, out var markingProto))
            {
                var colors = MarkingColoring.GetMarkingLayerColors(markingProto, appearanceComp.SkinColor, appearanceComp.EyeColor, new MarkingSet());
                var dictColors = colors.ToDictionary(x => colors.IndexOf(x));

                for (var i = 0; i < traitPrototype.MarkingColours.Count; i++)
                    dictColors[i] = traitPrototype.MarkingColours[i];

                _humanoidAppearanceSystem.AddMarking(args.Mob, traitPrototype.MarkingId, dictColors.Values.ToList(), true, true, appearanceComp);
            }

            # endregion End Thanatophobia Edits
        }
    }
}
