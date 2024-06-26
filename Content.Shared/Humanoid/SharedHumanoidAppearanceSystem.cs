using System.Linq;
using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Inventory;
using Content.Shared.Preferences;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects.Components.Localization;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Humanoid;

/// <summary>
///     HumanoidSystem. Primarily deals with the appearance and visual data
///     of a humanoid entity. HumanoidVisualizer is what deals with actually
///     organizing the sprites and setting up the sprite component's layers.
///
///     This is a shared system, because while it is server authoritative,
///     you still need a local copy so that players can set up their
///     characters.
/// </summary>
public abstract class SharedHumanoidAppearanceSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly MarkingManager _markingManager = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;

    [ValidatePrototypeId<SpeciesPrototype>]
    public const string DefaultSpecies = "Human";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HumanoidAppearanceComponent, OnBodyFinishInit>(OnInit);
        SubscribeLocalEvent<HumanoidAppearanceComponent, BodyPartAddedEvent>(EvUpdateLayers);
        SubscribeLocalEvent<HumanoidAppearanceComponent, BodyPartRemovedEvent>(EvUpdateLayers);

        SubscribeLocalEvent<BodyPartVisualiserComponent, GetBodyPartVisualEvent>(OnGetPartVisual);
        SubscribeLocalEvent<BodyPartVisualiserComponent, GetMarkingVisualEvent>(OnGetMarkingVisual);
        SubscribeLocalEvent<BodyPartVisualiserComponent, ClearCustomPartsEvent>(OnClearCustomParts);
        SubscribeLocalEvent<BodyPartVisualiserComponent, ClearPartMarkingsEvent>(OnClearMarkings);
        SubscribeLocalEvent<BodyPartVisualiserComponent, GetPartMarkingsEvent>(OnGetPartMarkings);

        SubscribeLocalEvent<GenderedBodyPartComponent, UpdateGenderedBodyPartEvent>(OnGenderUpdate);
    }

    private void EvUpdateLayers<TEvent>(EntityUid uid, HumanoidAppearanceComponent comp, TEvent args)
    {
        UpdatePartVisuals(uid, comp);
    }

    private void OnInit(EntityUid uid, HumanoidAppearanceComponent humanoid, OnBodyFinishInit args)
    {
        if (string.IsNullOrEmpty(humanoid.Species) || _netManager.IsClient && !IsClientSide(uid))
        {
            return;
        }

        if (string.IsNullOrEmpty(humanoid.Initial)
            || !_prototypeManager.TryIndex(humanoid.Initial, out HumanoidProfilePrototype? startingSet))
        {
            LoadProfile(uid, HumanoidCharacterProfile.DefaultWithSpecies(humanoid.Species), humanoid);
        }
        else
        {
            LoadProfile(uid, startingSet.Profile, humanoid);
        }

        foreach (var marking in humanoid.QueuedMarkings)
        {
            AddMarking(uid, marking.MarkingId, marking.MarkingColors, humanoid: humanoid);
        }
    }

    private void OnGenderUpdate(EntityUid uid, GenderedBodyPartComponent comp, UpdateGenderedBodyPartEvent args)
    {
        if (TryComp<BodyPartVisualiserComponent>(uid, out var partVisualiser) &&
            comp.Sprites.Any(x => x.Key == args.Sex))
        {
            partVisualiser.Sprites = comp.Sprites[args.Sex];
        }
    }

    private void OnGetPartVisual(EntityUid uid, BodyPartVisualiserComponent comp, ref GetBodyPartVisualEvent args)
    {
        if (_netManager.IsClient && !IsClientSide(uid))
            return;

        var sprites = comp.CustomSprites ?? comp.Sprites;

        args.Sprites.Add(sprites);

        foreach (var (bodyPart, visual) in sprites.Sprites)
        {
            for (var i = 0; i < sprites.Sprites.Count; i++)
            {
                if (sprites.DefaultColouring.Count < i && (args.OverrideColours || sprites.Colours.Count < i))
                    sprites.Colours[i] = sprites.DefaultColouring[i].GetColour(args.SkinColour, args.EyeColour);

                if (sprites.Colours.Count < i)
                    sprites.Colours[i] = Color.White;
            }
        }
    }

    private void OnGetMarkingVisual(EntityUid uid, BodyPartVisualiserComponent comp, ref GetMarkingVisualEvent args)
    {
        if (_netManager.IsClient && !IsClientSide(uid))
            return;

        foreach (var markings in comp.Markings.Markings.Values.ToList())
        {
            foreach (var marking in markings)
            {
                if (!_prototypeManager.TryIndex<MarkingPrototype>(marking.MarkingId, out var markingProto))
                    continue;

                var visuals = new BodyPartVisualiserSet()
                {
                    Colours = marking.MarkingColors
                };

                foreach (var (layer, sprites) in markingProto.Function.GetSprites())
                {
                    visuals.Sprites[layer] = sprites;
                }
            }
        }
    }

    private void OnGetPartMarkings(EntityUid uid, BodyPartVisualiserComponent comp, GetPartMarkingsEvent args)
    {
        foreach (var (category, markings) in comp.Markings.Markings)
        {
            foreach (var marking in markings)
            {
                args.Markings.AddBack(category, marking);
            }
        }
    }

    private void OnClearCustomParts(EntityUid uid, BodyPartVisualiserComponent comp, ClearCustomPartsEvent args)
    {
        comp.CustomSprites = null;
    }

    private void OnClearMarkings(EntityUid uid, BodyPartVisualiserComponent comp, ClearPartMarkingsEvent args)
    {
        comp.Markings.Clear();
    }

    private List<EntityUid> QuickGetAllParts(EntityUid uid)
    {
        if (!TryComp<BodyComponent>(uid, out var bodyComp))
            return new();

        if (bodyComp.RootContainer == null)
            return new();

        // Holy fuck this looks ugly. At least it works I guess.
        return _bodySystem.GetBodyChildren(uid, bodyComp).ToDictionary().Keys.ToList().Concat(_bodySystem.GetBodyOrgans(uid, bodyComp).ToDictionary().Keys.ToList()).ToList();
    }

    public void UpdatePartVisuals(EntityUid uid, HumanoidAppearanceComponent component, List<EntityUid>? bodyParts = null, bool overrideColours = false)
    {
        if (_netManager.IsClient && !IsClientSide(uid))
            return;

        component.Parts = new();

        bodyParts ??= QuickGetAllParts(uid);

        var ev = new GetBodyPartVisualEvent(component.SkinColor, component.EyeColor, overrideColours)
        {
            Sprites = component.Parts
        };

        foreach (var part in bodyParts)
            RaiseLocalEvent(part, ref ev);

        // Get marking visuals after the body part visuals.

        var markingEv = new GetMarkingVisualEvent(component.SkinColor, component.EyeColor, overrideColours)
        {
            Sprites = component.Parts
        };

        foreach (var part in bodyParts)
            RaiseLocalEvent(part, ref markingEv);

        Dirty(uid, component);
    }

    /// <summary>
    ///     Toggles a humanoid's sprite layer visibility.
    /// </summary>
    /// <param name="uid">Humanoid mob's UID</param>
    /// <param name="layer">Layer to toggle visibility for</param>
    /// <param name="humanoid">Humanoid component of the entity</param>
    public void SetLayerVisibility(EntityUid uid,
        string layer,
        bool visible,
        bool permanent = false,
        HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid, false))
            return;

        var dirty = false;
        SetLayerVisibility(uid, humanoid, layer, visible, permanent, ref dirty);

        if (dirty)
            Dirty(uid, humanoid);
    }

    /// <summary>
    ///     Sets the visibility for multiple layers at once on a humanoid's sprite.
    /// </summary>
    /// <param name="uid">Humanoid mob's UID</param>
    /// <param name="layers">An enumerable of all sprite layers that are going to have their visibility set</param>
    /// <param name="visible">The visibility state of the layers given</param>
    /// <param name="permanent">If this is a permanent change, or temporary. Permanent layers are stored in their own hash set.</param>
    /// <param name="humanoid">Humanoid component of the entity</param>
    public void SetLayersVisibility(EntityUid uid, IEnumerable<string> layers, bool visible, bool permanent = false,
        HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid))
            return;

        var dirty = false;

        foreach (var layer in layers)
        {
            SetLayerVisibility(uid, humanoid, layer, visible, permanent, ref dirty);
        }

        if (dirty)
            Dirty(uid, humanoid);
    }

    protected virtual void SetLayerVisibility(
        EntityUid uid,
        HumanoidAppearanceComponent humanoid,
        string layer,
        bool visible,
        bool permanent,
        ref bool dirty)
    {
        if (visible)
        {
            if (permanent)
                dirty |= humanoid.PermanentlyHidden.Remove(layer);

            dirty |= humanoid.HiddenLayers.Remove(layer);
        }
        else
        {
            if (permanent)
                dirty |= humanoid.PermanentlyHidden.Add(layer);

            dirty |= humanoid.HiddenLayers.Add(layer);
        }
    }

    /// <summary>
    ///     Set a humanoid mob's species. This will change their base sprites, as well as their current
    ///     set of markings to fit against the mob's new species.
    /// </summary>
    /// <param name="uid">The humanoid mob's UID.</param>
    /// <param name="species">The species to set the mob to. Will return if the species prototype was invalid.</param>
    /// <param name="sync">Whether to immediately synchronize this to the humanoid mob, or not.</param>
    /// <param name="humanoid">Humanoid component of the entity</param>
    public void SetSpecies(EntityUid uid, string species, bool sync = true, HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid) || !_prototypeManager.TryIndex<SpeciesPrototype>(species, out var _))
            return;

        humanoid.Species = species;

        if (sync)
            Dirty(uid, humanoid);
    }

    /// <summary>
    ///     Sets the skin color of this humanoid mob. Will only affect base layers that are not custom,
    ///     custom base layers should use <see cref="SetBaseLayerColor"/> instead.
    /// </summary>
    /// <param name="uid">The humanoid mob's UID.</param>
    /// <param name="skinColor">Skin color to set on the humanoid mob.</param>
    /// <param name="sync">Whether to synchronize this to the humanoid mob, or not.</param>
    /// <param name="verify">Whether to verify the skin color can be set on this humanoid or not</param>
    /// <param name="humanoid">Humanoid component of the entity</param>
    public virtual void SetSkinColor(EntityUid uid, Color skinColor, bool sync = true, bool verify = true, HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid))
            return;

        if (!_prototypeManager.TryIndex(humanoid.Species, out var species))
        {
            return;
        }

        if (verify && !SkinColor.VerifySkinColor(species.SkinColoration, skinColor))
        {
            skinColor = SkinColor.ValidSkinTone(species.SkinColoration, skinColor);
        }

        humanoid.SkinColor = skinColor;

        if (sync)
            Dirty(uid, humanoid);
    }

    /// <summary>
    ///     Sets the color of this humanoid mob's base layer. See <see cref="SetBaseLayerId"/> for a
    ///     description of how base layers work.
    /// </summary>
    /// <param name="uid">The humanoid mob's UID.</param>
    /// <param name="layer">The layer to target on this humanoid mob.</param>
    /// <param name="color">The color to set this base layer to.</param>
    public void SetBaseLayerColor(EntityUid uid, string layer, Color? color, bool sync = true, HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid))
            return;
    }

    /// <summary>
    ///     Set a humanoid mob's sex. This will not change their gender.
    /// </summary>
    /// <param name="uid">The humanoid mob's UID.</param>
    /// <param name="sex">The sex to set the mob to.</param>
    /// <param name="sync">Whether to immediately synchronize this to the humanoid mob, or not.</param>
    /// <param name="humanoid">Humanoid component of the entity</param>
    public void SetSex(EntityUid uid, Sex sex, bool sync = true, HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid) || humanoid.Sex == sex)
            return;

        var oldSex = humanoid.Sex;
        humanoid.Sex = sex;
        RaiseLocalEvent(uid, new SexChangedEvent(oldSex, sex));

        if (sync)
            Dirty(uid, humanoid);
    }

    public void ClearMarkings(EntityUid uid, HumanoidAppearanceComponent humanoid, List<EntityUid>? bodyParts = null, bool dirty = true)
    {
        bodyParts ??= _bodySystem.GetBodyChildren(uid).ToDictionary().Keys.ToList();

        foreach (var part in bodyParts)
            RaiseLocalEvent(part, new ClearPartMarkingsEvent());

        if (dirty)
            Dirty(uid, humanoid);
    }

    public MarkingSet GetMarkings(EntityUid uid, List<EntityUid>? bodyParts = null)
    {
        bodyParts ??= _bodySystem.GetBodyChildren(uid).ToDictionary().Keys.ToList();

        var ev = new GetPartMarkingsEvent();

        foreach (var part in bodyParts)
            RaiseLocalEvent(part, ref ev);


        return ev.Markings;
    }

    /// <summary>
    ///     Loads a humanoid character profile directly onto this humanoid mob.
    /// </summary>
    /// <param name="uid">The mob's entity UID.</param>
    /// <param name="profile">The character profile to load.</param>
    /// <param name="humanoid">Humanoid component of the entity</param>
    public virtual void LoadProfile(EntityUid uid, HumanoidCharacterProfile profile, HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid))
            return;

        var bodyParts = QuickGetAllParts(uid);

        // be mature, be mature, be mature, be mature...
        var sexEv = new UpdateGenderedBodyPartEvent(profile.Sex);

        foreach (var part in bodyParts)
        {
            RaiseLocalEvent(part, new ClearCustomPartsEvent());
            RaiseLocalEvent(part, sexEv);
        }

        ClearMarkings(uid, humanoid, bodyParts, false);

        SetSpecies(uid, profile.Species, false, humanoid);
        SetSex(uid, profile.Sex, false, humanoid);
        humanoid.EyeColor = profile.Appearance.EyeColor;

        SetSkinColor(uid, profile.Appearance.SkinColor, false);

        // Add markings that doesn't need coloring. We store them until we add all other markings that doesn't need it.
        var markingFColored = new Dictionary<Marking, MarkingPrototype>();
        foreach (var marking in profile.Appearance.Markings)
        {
            if (_markingManager.TryGetMarking(marking, out var prototype))
            {
                if (!prototype.ForcedColoring)
                {
                    AddMarking(uid, marking.MarkingId, marking.MarkingColors, false);
                }
                else
                {
                    markingFColored.Add(marking, prototype);
                }
            }
        }

        // Hair/facial hair - this may eventually be deprecated.
        // We need to ensure hair before applying it or coloring can try depend on markings that can be invalid
        var hairColor = _markingManager.MustMatchSkin(profile.Species, "Hair", out var hairAlpha, _prototypeManager) // Hardcoded... AUGH!
            ? profile.Appearance.SkinColor.WithAlpha(hairAlpha)
            : profile.Appearance.HairColor;

        var facialHairColor = _markingManager.MustMatchSkin(profile.Species, "FacialHair", out var facialHairAlpha, _prototypeManager)
            ? profile.Appearance.SkinColor.WithAlpha(facialHairAlpha)
            : profile.Appearance.FacialHairColor;

        if (_markingManager.Markings.TryGetValue(profile.Appearance.HairStyleId, out var hairPrototype) &&
            _markingManager.CanBeApplied(profile.Species, profile.Sex, hairPrototype, _prototypeManager))
        {
            AddMarking(uid, profile.Appearance.HairStyleId, hairColor, false);
        }

        if (_markingManager.Markings.TryGetValue(profile.Appearance.FacialHairStyleId, out var facialHairPrototype) &&
            _markingManager.CanBeApplied(profile.Species, profile.Sex, facialHairPrototype, _prototypeManager))
        {
            AddMarking(uid, profile.Appearance.FacialHairStyleId, facialHairColor, false);
        }

        // Finally adding marking with forced colors
        foreach (var (marking, prototype) in markingFColored)
        {
            var markingColors = MarkingColoring.GetMarkingLayerColors(
                prototype,
                profile.Appearance.SkinColor,
                profile.Appearance.EyeColor,
                GetMarkings(uid)
            );
            AddMarking(uid, marking.MarkingId, markingColors, false);
        }

        // And then add default markings.
        EnsureDefaultMarkings(uid, bodyParts, humanoid);

        humanoid.Gender = profile.Gender;
        if (TryComp<GrammarComponent>(uid, out var grammar))
        {
            grammar.Gender = profile.Gender;
        }

        humanoid.Age = profile.Age;

        UpdatePartVisuals(uid, humanoid, bodyParts, true);

        Dirty(uid, humanoid);
    }

    /// <summary>
    ///     Adds a marking to this humanoid with a single color.
    /// </summary>
    /// <param name="uid">Humanoid mob's UID</param>
    /// <param name="marking">Marking ID to use</param>
    /// <param name="color">Color to apply to all marking layers of this marking</param>
    /// <param name="sync">Whether to immediately sync this marking or not</param>
    /// <param name="forced">If this marking was forced (ignores marking points)</param>
    /// <param name="humanoid">Humanoid component of the entity</param>
    public void AddMarking(
        EntityUid uid,
        string marking,
        Color? color = null,
        bool sync = true,
        bool forced = false,
        HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid) ||
            !_markingManager.Markings.TryGetValue(marking, out var prototype))
        {
            return;
        }

        var markingObject = prototype.AsMarking();
        markingObject.Forced = forced;

        if (color != null)
        {
            for (var i = 0; i < prototype.Function.GetSpriteCount(); i++)
            {
                markingObject.SetColor(i, color.Value);
            }
        }

        if (TryComp<BodyComponent>(uid, out var bodyComp) && bodyComp.RootContainer != null)
        {
            var bodyPartContainers = _bodySystem.GetBodyWithOrganContainers(uid);

            prototype.Function.AddMarking(uid, markingObject, bodyPartContainers, _prototypeManager, EntityManager);

            if (sync)
                Dirty(uid, humanoid);
        }
        else // Queue markings for when bodycomp is ready.
        {
            humanoid.QueuedMarkings.Add(markingObject);
        }
    }

    public void ReplaceMarkings(EntityUid uid, MarkingSet markingSet, bool dirty = true, HumanoidAppearanceComponent? component = null, List<EntityUid>? bodyParts = null)
    {
        if (!Resolve(uid, ref component))
            return;

        bodyParts ??= QuickGetAllParts(uid);

        ClearMarkings(uid, component, bodyParts, false);

        foreach (var markings in markingSet.Markings.Values)
        {
            foreach (var marking in markings)
            {
                AddMarking(uid, marking.MarkingId, marking.MarkingColors, false, marking.Forced, component);
            }
        }

        if (dirty)
        {
            Dirty(uid, component);
            UpdatePartVisuals(uid, component);
        }
    }

    private void EnsureDefaultMarkings(EntityUid uid, List<EntityUid> bodyParts, HumanoidAppearanceComponent? humanoid)
    {
        if (!Resolve(uid, ref humanoid))
        {
            return;
        }

        if (!_prototypeManager.TryIndex(humanoid.Species, out var speciesProto) ||
            !_prototypeManager.TryIndex<MarkingPointsPrototype>(speciesProto.MarkingPoints, out var markingPoints))
            return;

        var markingSet = GetMarkings(uid, bodyParts);

        foreach (var (category, points) in markingPoints.Points)
        {
            if (points.DefaultMarkings.Count == 0)
                continue;

            if (markingSet.TryGetCategory(category, out var currentMarkings) &&
                currentMarkings.Count != 0)
                continue;

            foreach (var marking in points.DefaultMarkings)
            {
                if (!_prototypeManager.TryIndex<MarkingPrototype>(marking, out var markingProto))
                    continue;

                var colors = MarkingColoring.GetMarkingLayerColors(
                    markingProto,
                    humanoid.SkinColor,
                    humanoid.EyeColor,
                    markingSet
                );

                AddMarking(uid, marking, colors, sync: false);
            }
        }
    }

    /// <summary>
    /// Adds a marking to this mob with multiple colours.
    /// </summary>
    /// <param name="uid">Humanoid mob's UID</param>
    /// <param name="marking">Marking ID to use</param>
    /// <param name="colors">Colors to apply against this marking's set of sprites.</param>
    /// <param name="sync">Whether to immediately sync this marking or not</param>
    /// <param name="forced">If this marking was forced (ignores marking points)</param>
    /// <param name="humanoid">Humanoid component of the entity</param>
    public void AddMarking(
        EntityUid uid,
        string marking,
        IReadOnlyList<Color> colors,
        bool sync = true,
        bool forced = false,
        HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid)
            || !_markingManager.Markings.TryGetValue(marking, out var prototype))
        {
            return;
        }

        var markingObject = new Marking(marking, colors);
        markingObject.Forced = forced;

        if (TryComp<BodyComponent>(uid, out var bodyComp) && bodyComp.RootContainer != null)
        {
            var bodyPartContainers = _bodySystem.GetBodyWithOrganContainers(uid);
            prototype.Function.AddMarking(uid, markingObject, bodyPartContainers, _prototypeManager, EntityManager);

            if (sync)
            {
                UpdatePartVisuals(uid, humanoid);
                Dirty(uid, humanoid);
            }
        }
        else // Queue markings for when bodycomp is ready.
        {
            humanoid.QueuedMarkings.Add(markingObject);
        }
    }
}
