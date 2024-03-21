using Content.Shared.Examine;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.IdentityManagement;
using Content.Shared.Preferences;
using Content.Shared.Verbs;
using Robust.Shared.GameObjects.Components.Localization;
using Robust.Shared.Prototypes;

namespace Content.Server.Humanoid;

public sealed partial class HumanoidAppearanceSystem : SharedHumanoidAppearanceSystem
{
    [Dependency] private readonly MarkingManager _markingManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HumanoidAppearanceComponent, HumanoidMarkingModifierMarkingSetMessage>(OnMarkingsSet);
        SubscribeLocalEvent<HumanoidAppearanceComponent, HumanoidMarkingModifierBaseLayersSetMessage>(OnBaseLayersSet);
        SubscribeLocalEvent<HumanoidAppearanceComponent, GetVerbsEvent<Verb>>(OnVerbsRequest);
        SubscribeLocalEvent<HumanoidAppearanceComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, HumanoidAppearanceComponent component, ExaminedEvent args)
    {
        var identity = Identity.Entity(uid, EntityManager);
        var species = GetSpeciesRepresentation(component.Species).ToLower();
        var age = GetAgeRepresentation(component.Species, component.Age);

        args.PushText(Loc.GetString("humanoid-appearance-component-examine", ("user", identity), ("age", age), ("species", species)));
    }

    // this was done enough times that it only made sense to do it here

    /// <summary>
    ///     Clones a humanoid's appearance to a target mob, provided they both have humanoid components.
    /// </summary>
    /// <param name="source">Source entity to fetch the original appearance from.</param>
    /// <param name="target">Target entity to apply the source entity's appearance to.</param>
    /// <param name="sourceHumanoid">Source entity's humanoid component.</param>
    /// <param name="targetHumanoid">Target entity's humanoid component.</param>
    public void CloneAppearance(EntityUid source, EntityUid target, HumanoidAppearanceComponent? sourceHumanoid = null,
        HumanoidAppearanceComponent? targetHumanoid = null)
    {
        if (!Resolve(source, ref sourceHumanoid) || !Resolve(target, ref targetHumanoid))
        {
            return;
        }

        targetHumanoid.Species = sourceHumanoid.Species;
        targetHumanoid.SkinColor = sourceHumanoid.SkinColor;
        targetHumanoid.EyeColor = sourceHumanoid.EyeColor;
        targetHumanoid.Age = sourceHumanoid.Age;
        SetSex(target, sourceHumanoid.Sex, false, targetHumanoid);
        _humanoidSystem.ReplaceMarkings(target, new(_humanoidSystem.GetMarkings(source)), false, targetHumanoid);

        targetHumanoid.Gender = sourceHumanoid.Gender;
        if (TryComp<GrammarComponent>(target, out var grammar))
        {
            grammar.Gender = sourceHumanoid.Gender;
        }

        Dirty(target, targetHumanoid);
    }

    /// <summary>
    /// Takes ID of the species prototype, returns UI-friendly name of the species.
    /// </summary>
    public string GetSpeciesRepresentation(string speciesId)
    {
        if (_prototypeManager.TryIndex<SpeciesPrototype>(speciesId, out var species))
        {
            return Loc.GetString(species.Name);
        }
        else
        {
            return Loc.GetString("humanoid-appearance-component-unknown-species");
        }
    }

    public string GetAgeRepresentation(string species, int age)
    {
        _prototypeManager.TryIndex<SpeciesPrototype>(species, out var speciesPrototype);

        if (speciesPrototype == null)
        {
            Log.Error("Tried to get age representation of species that couldn't be indexed: " + species);
            return Loc.GetString("identity-age-young");
        }

        if (age < speciesPrototype.YoungAge)
        {
            return Loc.GetString("identity-age-young");
        }

        if (age < speciesPrototype.OldAge)
        {
            return Loc.GetString("identity-age-middle-aged");
        }

        return Loc.GetString("identity-age-old");
    }
}
