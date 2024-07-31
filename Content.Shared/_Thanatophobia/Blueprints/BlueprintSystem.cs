using Content.Shared.Construction;
using Content.Shared.Destructible;
using Content.Shared.Interaction;
using Content.Shared.Lathe;
using Content.Shared.Popups;
using Content.Shared.Research.Prototypes;
using Content.Shared.Sound;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared.Thanatophobia.Blueprints;

public sealed partial class BlueprintSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlueprintModuleTakerComponent, MapInitEvent>(ModuleTakerMapInit);
        SubscribeLocalEvent<BlueprintModuleTakerComponent, LatheGetRecipesEvent>(ModuleTakerGetRecipes);
        SubscribeLocalEvent<BlueprintModuleTakerComponent, InteractUsingEvent>(ModuleTakerInteract);
        SubscribeLocalEvent<BlueprintModuleTakerComponent, MachineDeconstructedEvent>(ModuleTakerDeconstruct);
        SubscribeLocalEvent<BlueprintModuleTakerComponent, DestructionEventArgs>(ModuleTakerDeconstruct);
    }

    public void ModuleTakerMapInit(EntityUid uid, BlueprintModuleTakerComponent comp, ref MapInitEvent args)
    {
        var container = _containerSystem.EnsureContainer<Container>(uid, comp.ContainerID);
        comp.BlueprintContainer = container;
    }

    public void ModuleTakerGetRecipes(EntityUid uid, BlueprintModuleTakerComponent comp, ref LatheGetRecipesEvent args)
    {
        if (comp.BlueprintContainer == null)
            return;

        foreach (var entity in comp.BlueprintContainer.ContainedEntities)
        {
            if (!TryComp<BlueprintModuleComponent>(entity, out var moduleComp))
                continue;

            foreach (var recipeID in moduleComp.Recipes)
            {
                if (!_prototypeManager.HasIndex(recipeID))
                    continue;

                args.Recipes.Add(recipeID);
            }
        }
    }

    public void ModuleTakerInteract(EntityUid uid, BlueprintModuleTakerComponent comp, ref InteractUsingEvent args)
    {
        if (!HasComp<BlueprintModuleComponent>(args.Used) ||
            !TryComp<MetaDataComponent>(args.Used, out var usedMetaData) ||
            comp.BlueprintContainer == null)
            return;

        foreach (var entity in comp.BlueprintContainer.ContainedEntities)
        {
            if (!TryComp<MetaDataComponent>(entity, out var insertMetaData))
                continue;

            if (usedMetaData.EntityPrototype == insertMetaData.EntityPrototype)
            {
                if (_netManager.IsServer)
                {
                    _popupSystem.PopupEntity(Loc.GetString("thanato-blueprint-unsuccessful-insert-already-inserted"), args.User, args.User);
                }
                return;
            }
        }

        if (_netManager.IsServer)
        {
            _popupSystem.PopupEntity(Loc.GetString("thanato-blueprint-successful-insert"), args.User, args.User);
            _containerSystem.Insert(args.Used, comp.BlueprintContainer);

            if (comp.InsertSound != null)
                _audioSystem.PlayPvs(comp.InsertSound, uid);
        }
    }

    public void ModuleTakerDeconstruct<TEvent>(EntityUid uid, BlueprintModuleTakerComponent comp, ref TEvent args)
    {
        if (_netManager.IsClient ||
            comp.BlueprintContainer == null)
            return;

        while (comp.BlueprintContainer.ContainedEntities.Count != 0)
            _containerSystem.TryRemoveFromContainer(comp.BlueprintContainer.ContainedEntities[0]);
    }
}
