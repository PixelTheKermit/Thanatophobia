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
    [Dependency] private readonly SharedHandsSystem _sharedHandsSystem = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearanceSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    // When the player is spawned in, add all trait components selected during character creation
    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        // Validate the traits.
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

            foreach (var function in traitPrototype.Functions)
            {
                function.AddTrait(args.Mob, traitPrototype, _prototypeManager, EntityManager);
            }

            # endregion End Thanatophobia Edits
        }
    }
}
