using System.Linq;
using System.Numerics;
using Content.Client.Administration.UI.CustomControls;
using Content.Client.Message;
using Content.Client.Stylesheets;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Content.Shared.Traits;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

namespace Content.Client.Thanatophobia.Preferences.UI.CustomControls;

public sealed partial class TPSpeciesTraitsCustomisation : TPBaseCustomisationControl
{
    // Dependencies
    private readonly IPrototypeManager _protoManager;

    // Controls
    RichTextLabel _speciesDescription;
    private OptionButton _speciesButton;
    private Label _traitsCountLabel;
    private Label _traitsPointsLabel;

    // Trait Box Controls
    private TraitBox _posUsedTraitBox;
    private TraitBox _posUnusedTraitBox;
    private TraitBox _rpUsedTraitBox;
    private TraitBox _rpUnusedTraitBox;
    private TraitBox _negUsedTraitBox;
    private TraitBox _negUnusedTraitBox;

    // Variables
    private List<SpeciesPrototype> _speciesList;

    public TPSpeciesTraitsCustomisation(TPHumanoidProfileEditor profileEditor) : base(profileEditor)
    {
        _protoManager = IoCManager.Resolve<IPrototypeManager>();

        TabName = Loc.GetString("tp-humanoid-profile-editor-species-traits-tab");

        var speciesEditLabel = new Label()
        {
            Text = Loc.GetString("humanoid-profile-editor-species-label"),
            HorizontalAlignment = HAlignment.Right
        };

        _speciesButton = new();
        _speciesButton.OnItemSelected += args => ChangeSpecies(args.Id);

        _speciesList = _protoManager.EnumeratePrototypes<SpeciesPrototype>().Where(proto => proto.RoundStart).ToList();
        for (var i = 0; i < _speciesList.Count; i++)
        {
            var name = Loc.GetString(_speciesList[i].Name);
            _speciesButton.AddItem(name, i);
        };

        var speciesButtonContainer = new BoxContainer()
        {
            XamlChildren = { speciesEditLabel, _speciesButton },
        };

        _speciesDescription = new()
        {
            Margin = new Thickness(5),
            VerticalAlignment = VAlignment.Top,
            HorizontalAlignment = HAlignment.Left,
        };

        _speciesDescription.SetMarkup("There should be a description about a species here. If this message appears, please fix the UI.");

        var descriptionScroll = new ScrollContainer()
        {
            HScrollEnabled = false,
            XamlChildren = { _speciesDescription }
        };

        var descriptionBackground = new PanelContainer()
        {
            VerticalExpand = true,
            HorizontalExpand = true,
            PanelOverride = new StyleBoxFlat(Color.Black.WithAlpha(0.25f)),
            XamlChildren = { descriptionScroll },
        };

        _traitsCountLabel = new()
        {
            Text = Loc.GetString("tp-humanoid-profile-editor-traits-count", ("count", "???"), ("actual-count", "???"), ("max", "???")),
            HorizontalAlignment = HAlignment.Right,
            HorizontalExpand = true,
        };

        _traitsPointsLabel = new()
        {
            Text = Loc.GetString("tp-humanoid-profile-editor-traits-points", ("total", "???"), ("pos", "???"), ("neg", "???")),
            HorizontalAlignment = HAlignment.Left,
        };

        var leftContainer = new BoxContainer()
        {
            MinSize = new Vector2(300, 0),
            MaxWidth = 300,
            HorizontalAlignment = HAlignment.Left,
            Margin = new Thickness(5),
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            XamlChildren =
            {
                speciesButtonContainer,
                descriptionBackground,
                new BoxContainer()
                {
                    XamlChildren = { _traitsPointsLabel, _traitsCountLabel },
                    VerticalAlignment = VAlignment.Bottom
                }
            }
        };

        // Traits!

        var usedTraitsLabel = new Label()
        {
            Text = Loc.GetString("tp-character-editor-used-traits"),
        };

        var unusedTraitsLabel = new Label()
        {
            Text = Loc.GetString("tp-character-editor-unused-traits"),
        };

        _posUsedTraitBox = new();
        _posUnusedTraitBox = new();
        _rpUsedTraitBox = new();
        _rpUnusedTraitBox = new();
        _negUsedTraitBox = new();
        _negUnusedTraitBox = new();

        var traitGridContainer = new GridContainer()
        {
            Columns = 2,
            XamlChildren = {
                unusedTraitsLabel, usedTraitsLabel,
                _posUnusedTraitBox, _posUsedTraitBox,
                _rpUnusedTraitBox, _rpUsedTraitBox,
                _negUnusedTraitBox, _negUsedTraitBox }
        };

        var rightContainer = new ScrollContainer()
        {
            HorizontalExpand = true,
            XamlChildren =
            {
                new BoxContainer()
                {
                    HorizontalAlignment = HAlignment.Left,
                    Orientation = BoxContainer.LayoutOrientation.Vertical,
                    Margin = new Thickness(5),
                    XamlChildren =
                    {
                        traitGridContainer
                    }
                }
            }
        };

        var controlContainer = new BoxContainer()
        {
            XamlChildren =
            {
                leftContainer,
                new VSeparator(),
                rightContainer,
            }
        };

        AddChild(controlContainer);

        RefreshControls();
    }

    private void ChangeSpecies(int index)
    {
        if (ProfileEditor.Humanoid == null)
            return;

        ProfileEditor.Humanoid = ProfileEditor.Humanoid.WithSpecies(_speciesList[index].ID);
        RefreshSkinColour();
        ProfileEditor.UpdateSpriteView(true);
        _speciesButton.Select(index);
        _speciesDescription.SetMarkup(Loc.GetString($"tp-species-description-{ProfileEditor.Humanoid.Species}"));
    }

    private void ToggleTrait(string traitId, bool add = true)
    {
        if (ProfileEditor.Humanoid == null)
            return;

        ProfileEditor.Humanoid = ProfileEditor.Humanoid.WithTraitPreference(traitId, add);
        RefreshTraitControls();
    }

    private void RefreshSkinColour()
    {
        if (ProfileEditor.Humanoid == null)
            return;

        var skinTone = _protoManager.Index<SpeciesPrototype>(ProfileEditor.Humanoid.Species).SkinColoration;

        var color = skinTone switch
        {
            HumanoidSkinColor.HumanToned => SkinColor.HumanSkinTone((int) SkinColor.HumanSkinToneFromColor(ProfileEditor.Humanoid.Appearance.SkinColor)),
            HumanoidSkinColor.Hues => ProfileEditor.Humanoid.Appearance.SkinColor,
            HumanoidSkinColor.TintedHues => SkinColor.TintedHues(ProfileEditor.Humanoid.Appearance.SkinColor),
            _ => ProfileEditor.Humanoid.Appearance.SkinColor,
        };

        ProfileEditor.Humanoid = ProfileEditor.Humanoid.WithCharacterAppearance(ProfileEditor.Humanoid.Appearance.WithSkinColor(color));
    }

    private void RefreshTraitControls()
    {
        if (ProfileEditor.Humanoid == null)
            return;

        if (!_protoManager.TryIndex<SpeciesPrototype>(ProfileEditor.Humanoid.Species, out var speciesProto)) // Species prototype required for max traits.
            speciesProto = _protoManager.Index<SpeciesPrototype>(SharedHumanoidAppearanceSystem.DefaultSpecies);

        // First we clear them all.
        _posUsedTraitBox.XamlChildren.Clear();
        _posUnusedTraitBox.XamlChildren.Clear();
        _negUsedTraitBox.XamlChildren.Clear();
        _negUnusedTraitBox.XamlChildren.Clear();
        _rpUnusedTraitBox.XamlChildren.Clear();
        _rpUsedTraitBox.XamlChildren.Clear();

        // Then we get every single trait that exists.
        var traitProtos = _protoManager.EnumeratePrototypes<TraitPrototype>();

        var usedCount = 0;
        var rpUsedCount = 0;

        var posPoints = 0;
        var negPoints = 0;

        // And now we fill all the controls. I haven't even coded this yet but I know I have sinned.
        foreach (var traitProto in traitProtos)
        {
            var isTraitValid = true;
            foreach (var tag in traitProto.Allowed)
            {
                if (!speciesProto.AllowedTraits.Any(i => i == tag))
                {
                    isTraitValid = false;
                    break;
                }
            }

            if (!isTraitValid)
                continue;

            var traitControl = new TraitButton(Loc.GetString(traitProto.Name), Loc.GetString(traitProto.Description ?? ""), traitProto.Cost.ToString());
            traitControl.TraitAdded += () => ToggleTrait(traitProto.ID, true);
            traitControl.TraitRemoved += () => ToggleTrait(traitProto.ID, false);

            var used = ProfileEditor.Humanoid.TraitPreferences.Any(i => i == traitProto.ID);
            if (traitProto.Cost > 0)
            {
                if (used)
                {
                    traitControl.ToggleUsed(true);
                    _posUsedTraitBox.XamlChildren.Add(traitControl);
                    negPoints += traitProto.Cost;
                    usedCount++;
                }
                else
                {
                    _posUnusedTraitBox.XamlChildren.Add(traitControl);
                }
            }
            else if (traitProto.Cost < 0)
            {
                if (used)
                {
                    traitControl.ToggleUsed(true);
                    _negUsedTraitBox.XamlChildren.Add(traitControl);
                    posPoints -= traitProto.Cost;
                    usedCount++;
                }
                else
                {
                    _negUnusedTraitBox.XamlChildren.Add(traitControl);
                }
            }
            else
            {
                if (used)
                {
                    traitControl.ToggleUsed(true);
                    _rpUsedTraitBox.XamlChildren.Add(traitControl);
                }
                else
                {
                    _rpUnusedTraitBox.XamlChildren.Add(traitControl);
                }
            }

            if (used)
                rpUsedCount++;
        }
        _traitsCountLabel.Text = Loc.GetString("tp-humanoid-profile-editor-traits-count", ("count", usedCount), ("actual-count", rpUsedCount), ("max", speciesProto.MaxTraits));
        _traitsPointsLabel.Text = Loc.GetString("tp-humanoid-profile-editor-traits-points", ("total", posPoints - negPoints), ("pos", posPoints), ("neg", negPoints));
    }
    public override void RefreshControls()
    {
        if (ProfileEditor.Humanoid == null)
            return;

        if (!_protoManager.TryIndex<SpeciesPrototype>(ProfileEditor.Humanoid.Species, out var species))
            species = _protoManager.Index<SpeciesPrototype>(HumanoidCharacterProfile.Random().Species);

        ChangeSpecies(_speciesList.IndexOf(species));
        RefreshTraitControls();
    }

    private sealed class TraitBox : PanelContainer // Changing each individual one every time I want it updated sounds like complete aids I don't want to deal with.
    {
        public TraitBox()
        {
            HorizontalExpand = true;
            var controlContainer = new BoxContainer()
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical,
            };

            var controlScroll = new ScrollContainer()
            {
                HScrollEnabled = false,
                XamlChildren = { controlContainer }
            };

            Margin = new Thickness(5);
            MinSize = new Vector2(400, 200);
            PanelOverride = new StyleBoxFlat(Color.Black.WithAlpha(0.25f));

            Children.Add(controlScroll);
            XamlChildren = controlContainer.Children;
        }
    }

    private sealed class TraitButton : Control
    {
        public PanelContainer Background;
        private Button _addButton;
        private Button _removeButton;
        public Action? TraitAdded;
        public Action? TraitRemoved;


        public TraitButton(string traitName, string traitDesc, string cost)
        {
            Margin = new Thickness(2);

            Background = new()
            {
                ModulateSelfOverride = Color.FromHex("#CCCCCC"),
                PanelOverride = new StyleBoxFlat(StyleNano.ButtonColorDefault),
            };

            var nameLabel = new Label()
            {
                Text = traitName
            };

            var pointsLabel = new Label()
            {
                Text = cost,
                HorizontalAlignment = HAlignment.Right,
                HorizontalExpand = true
            };

            var descLabel = new RichTextLabel()
            {
                Margin = new Thickness(5),
                VerticalAlignment = VAlignment.Top,
                HorizontalAlignment = HAlignment.Left,
            };

            descLabel.SetMarkup(traitDesc);

            _addButton = new()
            {
                ModulateSelfOverride = StyleNano.ButtonColorGoodDefault,
                Text = "+",
                Visible = true,
            };
            _addButton.OnPressed += _ => TraitAdded?.Invoke();

            _removeButton = new()
            {
                ModulateSelfOverride = StyleNano.ButtonColorDefaultRed,
                Text = "-",
                Visible = false,
            };
            _removeButton.OnPressed += _ => TraitRemoved?.Invoke();

            var showDesc = new Button()
            {
                Margin = new Thickness(2, 0, 2, 0),
                Text = "▼",
            };

            var hideDesc = new Button()
            {
                Margin = new Thickness(2, 0, 2, 0),
                Text = "▲",
                Visible = false,
            };

            var descriptionScroll = new ScrollContainer()
            {
                HScrollEnabled = false,
                XamlChildren = { descLabel }
            };

            var descriptionBackground = new PanelContainer()
            {
                Margin = new Thickness(5),
                MinSize = new Vector2(0, 100),
                MaxWidth = 380,
                PanelOverride = new StyleBoxFlat(Color.Black.WithAlpha(0.25f)),
                XamlChildren = { descriptionScroll },
                Visible = false
            };

            showDesc.OnPressed += args =>
            {
                showDesc.Visible = false;
                hideDesc.Visible = true;
                descriptionBackground.Visible = true;
            };

            hideDesc.OnPressed += args =>
            {
                showDesc.Visible = true;
                hideDesc.Visible = false;
                descriptionBackground.Visible = false;
            };

            var buttonContainer = new BoxContainer()
            {
                Margin = new Thickness(5),
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                XamlChildren = { nameLabel, pointsLabel, showDesc, hideDesc, _addButton, _removeButton }
            };

            var controlContainer = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical,
                XamlChildren = { buttonContainer, descriptionBackground },
            };

            AddChild(Background);
            AddChild(controlContainer);
        }

        public void ToggleUsed(bool used)
        {
            _addButton.Visible = !used;
            _removeButton.Visible = used;
        }
    }

}
