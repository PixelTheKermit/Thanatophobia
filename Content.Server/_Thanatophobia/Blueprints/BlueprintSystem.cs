using System.Linq;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Shared.Construction;
using Content.Shared.Destructible;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Lathe;
using Content.Shared.Research.Prototypes;
using Content.Shared.Thanatophobia.Blueprints;
using Content.Shared.Verbs;
using Robust.Server.Audio;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Server.Thanatophobia.Blueprints;

public sealed partial class BlueprintSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UsesMemoryBlueprintsComponent, LatheGetRecipesEvent>(ModuleTakerGetRecipes);
        SubscribeLocalEvent<DigitalMemoryBlueprintComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<UsesMemoryBlueprintsComponent, GetVerbsEvent<AlternativeVerb>>(OnAlternativeUse);
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);
    }

    private void OnRoundStart(RoundStartingEvent args)
    {
        var storageEnt = Spawn(null, MapCoordinates.Nullspace);
        EnsureComp<BlueprintMemoryComponent>(storageEnt);
    }

    private void OnAlternativeUse(EntityUid uid, UsesMemoryBlueprintsComponent comp, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!_mindSystem.TryGetMind(args.User, out var _, out var mindComp))
            return;

        if (mindComp.UserId == null)
            return;

        var allStorage = EntityQuery<BlueprintMemoryComponent>().ToList();

        if (allStorage.Count <= 0)
            return;

        var storage = allStorage[0];

        args.Verbs.Add(new AlternativeVerb()
        {
            Act = () =>
            {
                if (!storage.PlayerToBlueprint.Any(x => x.Key == mindComp.UserId.Value))
                    storage.PlayerToBlueprint[mindComp.UserId.Value] = new();

                comp.Recipes = storage.PlayerToBlueprint[mindComp.UserId.Value].ToList();

                _popupSystem.PopupEntity(Loc.GetString("tp-blueprint-synchronisation-successful"), args.User, args.User);

                if (comp.SyncSound != null)
                    _audioSystem.PlayEntity(comp.SyncSound, args.User, uid);
            },
            Text = Loc.GetString("tp-synchronise-blueprints")
        });
    }

    private void OnUseInHand(EntityUid uid, DigitalMemoryBlueprintComponent comp, UseInHandEvent args)
    {
        if (!_mindSystem.TryGetMind(args.User, out var _, out var mindComp))
            return;

        if (mindComp.UserId == null)
            return;

        var allStorage = EntityQuery<BlueprintMemoryComponent>().ToList();

        if (allStorage.Count <= 0)
            return;

        var storage = allStorage[0];

        foreach (var recipe in comp.Recipes)
        {
            AddRecipe(mindComp.UserId.Value, recipe, storage);
        }

        _popupSystem.PopupEntity(Loc.GetString(comp.PopupString), args.User, args.User);

        if (comp.Sound != null)
            _audioSystem.PlayEntity(comp.Sound, args.User, args.User);

        QueueDel(uid);
    }

    public void AddRecipe(NetUserId netUserId, ProtoId<LatheRecipePrototype> recipe, BlueprintMemoryComponent? storage)
    {
        if (storage == null)
        {
            var allStorage = EntityQuery<BlueprintMemoryComponent>().ToList();

            if (allStorage.Count <= 0)
                return;

            storage = allStorage[0];
        }

        if (!storage.PlayerToBlueprint.Any(x => x.Key == netUserId))
            storage.PlayerToBlueprint[netUserId] = new();

        if (!storage.PlayerToBlueprint[netUserId].Any(x => x == recipe))
            storage.PlayerToBlueprint[netUserId].Add(recipe);
    }

    private void ModuleTakerGetRecipes(EntityUid uid, UsesMemoryBlueprintsComponent comp, ref LatheGetRecipesEvent args)
    {
        foreach (var recipeId in comp.Recipes)
        {
            if (!_prototypeManager.HasIndex(recipeId))
                continue;

            args.Recipes.Add(recipeId);
        }
    }
}
