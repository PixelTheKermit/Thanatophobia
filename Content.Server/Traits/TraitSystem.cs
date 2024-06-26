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
    [Dependency] private readonly ILogManager _log = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearanceSystem = default!;
    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        _sawmill = _log.GetSawmill("Traits");
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
                _sawmill.Warning($"No trait found with ID {traitId}!");
                continue;
            }

            if (traitPrototype.Whitelist != null && !traitPrototype.Whitelist.IsValid(args.Mob))
                continue;

            if (traitPrototype.Blacklist != null && traitPrototype.Blacklist.IsValid(args.Mob))
                continue;

            foreach (var function in traitPrototype.Functions)
                function.AddTrait(args.Mob, traitPrototype, _prototypeManager, EntityManager);
        }
    }
}
