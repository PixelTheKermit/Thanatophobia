using System.Linq;
using System.Numerics;
using Content.Client.Administration.UI.CustomControls;
using Content.Client.Humanoid;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client.Thanatophobia.Preferences.UI.CustomControls;

public sealed partial class TPAppearanceCustomisation : TPBaseCustomisationControl
{
    // Dependencies
    private readonly IPrototypeManager _protoManager;
    private readonly MarkingManager _markingManager;

    // Controls
    private LineEdit _nameEdit;
    private LineEdit _ageEdit;
    private OptionButton _sexButton;
    private OptionButton _genderButton;
    private Slider _skinColour;
    private ColorSelectorSliders _rgbSkinColour;
    private ColorSelectorSliders _eyeColour;
    private OptionButton _outfitButton;
    private OptionButton _backpackButton;
    private SingleMarkingPicker _hairPicker;
    private SingleMarkingPicker _facialHairPicker;
    private MarkingPicker _markingPicker;
    public TPAppearanceCustomisation(TPHumanoidProfileEditor profileEditor) : base(profileEditor)
    {
        _protoManager = IoCManager.Resolve<IPrototypeManager>();
        _markingManager = IoCManager.Resolve<MarkingManager>();

        TabName = Loc.GetString("humanoid-profile-editor-appearance-tab");

        var nameEditLabel = new Label()
        {
            Text = Loc.GetString("humanoid-profile-editor-name-label"),
            HorizontalAlignment = HAlignment.Right
        };

        _nameEdit = new()
        {
            MinSize = new Vector2(270, 0),
        };
        _nameEdit.OnTextChanged += SetName;

        var randomNameButton = new Button()
        {
            Text = Loc.GetString("humanoid-profile-editor-name-random-button")
        };
        randomNameButton.OnPressed += _ => RandomizeName();

        var nameEditContainer = new BoxContainer()
        {
            XamlChildren = { _nameEdit, randomNameButton },
        };

        var ageEditLabel = new Label()
        {
            Text = Loc.GetString("humanoid-profile-editor-age-label"),
            HorizontalAlignment = HAlignment.Right
        };

        _ageEdit = new()
        {
            MinSize = new Vector2(100, 0),
            HorizontalAlignment = HAlignment.Left,
        };

        _ageEdit.OnTextChanged += args =>
        {
            if (!int.TryParse(args.Text, out var newAge))
                return;

            SetAge(newAge);
        };

        var sexEditLabel = new Label()
        {
            Text = Loc.GetString("humanoid-profile-editor-sex-label"),
            HorizontalAlignment = HAlignment.Right
        };

        _sexButton = new()
        {
            MinSize = new Vector2(100, 0),
            HorizontalAlignment = HAlignment.Left,
        };
        _sexButton.OnItemSelected += args =>
        {
            SetSex((Sex) args.Id);
        };

        var genderEditLabel = new Label()
        {
            Text = Loc.GetString("humanoid-profile-editor-pronouns-label"),
            HorizontalAlignment = HAlignment.Right
        };

        _genderButton = new()
        {
            MinSize = new Vector2(100, 0),
            HorizontalAlignment = HAlignment.Left,
        };

        _genderButton.AddItem(Loc.GetString("humanoid-profile-editor-pronouns-male-text"), (int) Gender.Male);
        _genderButton.AddItem(Loc.GetString("humanoid-profile-editor-pronouns-female-text"), (int) Gender.Female);
        _genderButton.AddItem(Loc.GetString("humanoid-profile-editor-pronouns-epicene-text"), (int) Gender.Epicene);
        _genderButton.AddItem(Loc.GetString("humanoid-profile-editor-pronouns-neuter-text"), (int) Gender.Neuter);

        _genderButton.OnItemSelected += args =>
        {
            SetGender((Gender) args.Id);
        };

        var outfitEditLabel = new Label()
        {
            Text = Loc.GetString("humanoid-profile-editor-clothing-label"),
            HorizontalAlignment = HAlignment.Right
        };

        _outfitButton = new()
        {
            MinSize = new Vector2(100, 0),
            HorizontalAlignment = HAlignment.Left,
        };
        _outfitButton.AddItem(Loc.GetString("humanoid-profile-editor-preference-jumpsuit"), (int) ClothingPreference.Jumpsuit);
        _outfitButton.AddItem(Loc.GetString("humanoid-profile-editor-preference-jumpskirt"), (int) ClothingPreference.Jumpskirt);

        _outfitButton.OnItemSelected += args => SetOutfit((ClothingPreference) args.Id);

        var backpackEditLabel = new Label()
        {
            Text = Loc.GetString("humanoid-profile-editor-backpack-label"),
            HorizontalAlignment = HAlignment.Right
        };

        _backpackButton = new()
        {
            MinSize = new Vector2(100, 0),
            HorizontalAlignment = HAlignment.Left,
        };
        _backpackButton.AddItem(Loc.GetString("humanoid-profile-editor-preference-backpack"), (int) BackpackPreference.Backpack);
        _backpackButton.AddItem(Loc.GetString("humanoid-profile-editor-preference-satchel"), (int) BackpackPreference.Satchel);
        _backpackButton.AddItem(Loc.GetString("humanoid-profile-editor-preference-duffelbag"), (int) BackpackPreference.Duffelbag);

        _backpackButton.OnItemSelected += args => SetBackpack((BackpackPreference) args.Id);

        var basicInfoContainer = new GridContainer()
        {
            Columns = 2,

            XamlChildren =
            {
                nameEditLabel, nameEditContainer,
                ageEditLabel, _ageEdit,
                sexEditLabel, _sexButton,
                genderEditLabel, _genderButton,
                outfitEditLabel, _outfitButton,
                backpackEditLabel, _backpackButton
            }
        };

        var skinLabel = new Label()
        {
            Text = Loc.GetString("humanoid-profile-editor-skin-color-label"),
        };

        _skinColour = new()
        {
            Visible = false,
        };
        _skinColour.OnValueChanged += _ => SetSkinColour();

        _rgbSkinColour = new()
        {
            Visible = false,
        };
        _rgbSkinColour.OnColorChanged += _ => SetSkinColour();

        var eyeLabel = new Label()
        {
            Text = Loc.GetString("humanoid-profile-editor-eyes-label"),
        };

        _eyeColour = new();
        _eyeColour.OnColorChanged += _ => SetEyeColour();

        var leftContainer = new ScrollContainer()
        {
            MinSize = new Vector2(500, 0),
            XamlChildren =
            {
                new BoxContainer()
                {
                    HorizontalAlignment = HAlignment.Left,
                    Orientation = BoxContainer.LayoutOrientation.Vertical,
                    Margin = new Thickness(5),
                    XamlChildren =
                    {
                        basicInfoContainer,
                        skinLabel,
                        _skinColour,
                        _rgbSkinColour,
                        eyeLabel,
                        _eyeColour,
                    }
                }
            }
        };

        _hairPicker = new()
        {
            Category = MarkingCategories.Hair,
            MinWidth = 220,
            Margin = new Thickness(5),
        };

        _hairPicker.OnMarkingSelect += SetHair;
        _hairPicker.OnColorChanged += SetHairColour;
        _hairPicker.OnSlotRemove += _ => RemoveHair();
        _hairPicker.OnSlotAdd += AddHair;

        _facialHairPicker = new()
        {
            Category = MarkingCategories.FacialHair,
            MinWidth = 220,
            Margin = new Thickness(5),
        };

        _facialHairPicker.OnMarkingSelect += SetFacialHair;
        _facialHairPicker.OnColorChanged += SetFacialHairColour;
        _facialHairPicker.OnSlotRemove += _ => RemoveFacialHair();
        _facialHairPicker.OnSlotAdd += AddFacialHair;

        _markingPicker = new()
        {
            IgnoreCategories = "Hair,FacialHair",
        };
        _markingPicker.OnMarkingAdded += MarkingChanged;
        _markingPicker.OnMarkingRemoved += MarkingChanged;
        _markingPicker.OnMarkingColorChange += MarkingChanged;
        _markingPicker.OnMarkingRankChange += MarkingChanged;

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
                        new BoxContainer()
                        {
                            Margin = new Thickness(5, 0, 5, 0),
                            XamlChildren = { _hairPicker, _facialHairPicker }
                        },
                        _markingPicker
                    }
                }
            }
        };

        var controlContainer = new BoxContainer()
        {
            Margin = new Thickness(5, 5, 5, 5),
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

    private void SetName(LineEdit.LineEditEventArgs args)
    {
        if (ProfileEditor.Humanoid == null)
            return;

        ProfileEditor.Humanoid = ProfileEditor.Humanoid!.WithName(args.Text);
    }

    private void SetAge(int newAge)
    {
        if (ProfileEditor.Humanoid == null)
            return;

        ProfileEditor.Humanoid = ProfileEditor.Humanoid!.WithAge(newAge);
    }

    private void SetSex(Sex newSex)
    {
        if (ProfileEditor.Humanoid == null)
            return;

        ProfileEditor.Humanoid = ProfileEditor.Humanoid.WithSex(newSex);
        _sexButton.SelectId((int) newSex);
        UpdateMarkings();
        ProfileEditor.UpdateSpriteView();
    }

    private void SetGender(Gender newGender)
    {
        if (ProfileEditor.Humanoid == null)
            return;

        ProfileEditor.Humanoid = ProfileEditor.Humanoid.WithGender(newGender);
        _genderButton.SelectId((int) newGender);
    }

    private void RandomizeName()
    {
        if (ProfileEditor.Humanoid == null)
            return;

        ProfileEditor.Humanoid = ProfileEditor.Humanoid.WithName(HumanoidCharacterProfile.GetName(ProfileEditor.Humanoid.Species, ProfileEditor.Humanoid.Gender));
        _nameEdit.Text = ProfileEditor.Humanoid.Name;
    }

    private void SetSkinColour()
    {
        if (ProfileEditor.Humanoid == null)
            return;

        var skinTone = _protoManager.Index<SpeciesPrototype>(ProfileEditor.Humanoid.Species).SkinColoration;

        Color color = new();

        switch (skinTone)
        {
            case HumanoidSkinColor.HumanToned:
                if (!_skinColour.Visible)
                {
                    _skinColour.Visible = true;
                    _rgbSkinColour.Visible = false;
                }
                color = SkinColor.HumanSkinTone((int) _skinColour.Value);

                break;
            case HumanoidSkinColor.Hues:
                if (!_rgbSkinColour.Visible)
                {
                    _skinColour.Visible = false;
                    _rgbSkinColour.Visible = true;
                }
                color = _rgbSkinColour.Color;

                break;
            case HumanoidSkinColor.TintedHues:
                if (!_rgbSkinColour.Visible)
                {
                    _skinColour.Visible = false;
                    _rgbSkinColour.Visible = true;
                }
                color = SkinColor.TintedHues(_rgbSkinColour.Color);

                break;
        }

        ProfileEditor.Humanoid = ProfileEditor.Humanoid.WithCharacterAppearance(ProfileEditor.Humanoid.Appearance.WithSkinColor(color));
        UpdateMarkings();
        ProfileEditor.UpdateSpriteView();
    }

    private void SetEyeColour()
    {
        if (ProfileEditor.Humanoid == null)
            return;

        var color = _eyeColour.Color;

        ProfileEditor.Humanoid = ProfileEditor.Humanoid.WithCharacterAppearance(ProfileEditor.Humanoid.Appearance.WithEyeColor(color));
        UpdateMarkings();
        ProfileEditor.UpdateSpriteView();
    }

    private void SetOutfit(ClothingPreference outfit)
    {
        if (ProfileEditor.Humanoid == null)
            return;

        ProfileEditor.Humanoid = ProfileEditor.Humanoid.WithClothingPreference(outfit);
        ProfileEditor.UpdateSpriteView(true);

        _outfitButton.Select((int) outfit);
    }

    private void SetBackpack(BackpackPreference backpack)
    {
        if (ProfileEditor.Humanoid == null)
            return;

        ProfileEditor.Humanoid = ProfileEditor.Humanoid.WithBackpackPreference(backpack);
        ProfileEditor.UpdateSpriteView(true);

        _backpackButton.Select((int) backpack);
    }

    # region Hair
    private void SetHair((int slot, string id) style)
    {
        if (ProfileEditor.Humanoid == null)
            return;

        ProfileEditor.Humanoid = ProfileEditor.Humanoid.WithCharacterAppearance(ProfileEditor.Humanoid.Appearance.WithHairStyleName(style.id));
        ProfileEditor.UpdateSpriteView();
    }

    private void SetHairColour((int slot, Marking marking) style)
    {
        if (ProfileEditor.Humanoid == null)
            return;

        ProfileEditor.Humanoid = ProfileEditor.Humanoid.WithCharacterAppearance(ProfileEditor.Humanoid.Appearance.WithHairColor(style.marking.MarkingColors[0]));
        ProfileEditor.UpdateSpriteView();
    }

    private void RemoveHair()
    {
        if (ProfileEditor.Humanoid == null)
            return;

        ProfileEditor.Humanoid = ProfileEditor.Humanoid.WithCharacterAppearance(ProfileEditor.Humanoid.Appearance.WithHairStyleName(HairStyles.DefaultHairStyle));
        UpdateHair();
    }

    private void AddHair()
    {
        if (ProfileEditor.Humanoid == null)
            return;

        var hair = _markingManager.MarkingsByCategoryAndSpecies(MarkingCategories.Hair, ProfileEditor.Humanoid.Species).Keys.ToList().FirstOrDefault();

        if (string.IsNullOrEmpty(hair))
            return;

        ProfileEditor.Humanoid = ProfileEditor.Humanoid.WithCharacterAppearance(ProfileEditor.Humanoid.Appearance.WithHairStyleName(hair));
        UpdateHair();
    }

    private void UpdateHair()
    {
        if (ProfileEditor.Humanoid == null)
            return;

        if (!_protoManager.TryIndex<SpeciesPrototype>(ProfileEditor.Humanoid.Species, out var speciesProto))
            speciesProto = _protoManager.Index<SpeciesPrototype>(HumanoidCharacterProfile.RandomWithSpecies().Species);

        var hairId = ProfileEditor.Humanoid.Appearance.HairStyleId;
        var hairMarking = new List<Marking>();

        if (hairId != HairStyles.DefaultHairStyle)
        {
            hairMarking = new List<Marking>()
            {
                new(hairId, new List<Color>(){ ProfileEditor.Humanoid.Appearance.HairColor })
            };
        };

        _hairPicker.UpdateData(hairMarking, speciesProto.ID, 1);
        ProfileEditor.UpdateSpriteView();
    }

    #endregion

    #region Facial Hair

    private void SetFacialHair((int slot, string id) style)
    {
        if (ProfileEditor.Humanoid == null)
            return;

        ProfileEditor.Humanoid = ProfileEditor.Humanoid.WithCharacterAppearance(ProfileEditor.Humanoid.Appearance.WithFacialHairStyleName(style.id));
        ProfileEditor.UpdateSpriteView();
    }

    private void SetFacialHairColour((int slot, Marking marking) style)
    {
        if (ProfileEditor.Humanoid == null)
            return;

        ProfileEditor.Humanoid = ProfileEditor.Humanoid.WithCharacterAppearance(ProfileEditor.Humanoid.Appearance.WithFacialHairColor(style.marking.MarkingColors[0]));
        ProfileEditor.UpdateSpriteView();
    }

    private void RemoveFacialHair()
    {
        if (ProfileEditor.Humanoid == null)
            return;

        ProfileEditor.Humanoid = ProfileEditor.Humanoid.WithCharacterAppearance(ProfileEditor.Humanoid.Appearance.WithFacialHairStyleName(HairStyles.DefaultFacialHairStyle));
        UpdateFacialHair();
    }

    private void AddFacialHair()
    {
        if (ProfileEditor.Humanoid == null)
            return;

        var hair = _markingManager.MarkingsByCategoryAndSpecies(MarkingCategories.FacialHair, ProfileEditor.Humanoid.Species).Keys.ToList().FirstOrDefault();

        if (string.IsNullOrEmpty(hair))
            return;

        ProfileEditor.Humanoid = ProfileEditor.Humanoid.WithCharacterAppearance(ProfileEditor.Humanoid.Appearance.WithFacialHairStyleName(hair));
        UpdateFacialHair();
    }

    private void UpdateFacialHair()
    {
        if (ProfileEditor.Humanoid == null)
            return;

        if (!_protoManager.TryIndex<SpeciesPrototype>(ProfileEditor.Humanoid.Species, out var speciesProto))
            speciesProto = _protoManager.Index<SpeciesPrototype>(HumanoidCharacterProfile.RandomWithSpecies().Species);

        var facialHairId = ProfileEditor.Humanoid.Appearance.FacialHairStyleId;
        var facialHairMarking = new List<Marking>();

        if (facialHairId != HairStyles.DefaultFacialHairStyle)
        {
            facialHairMarking = new List<Marking>()
            {
                new(facialHairId, new List<Color>(){ ProfileEditor.Humanoid.Appearance.FacialHairColor })
            };
        };

        _facialHairPicker.UpdateData(facialHairMarking, speciesProto.ID, 1);
        ProfileEditor.UpdateSpriteView();
    }

    #endregion

    private void MarkingChanged(MarkingSet markings)
    {
        if (ProfileEditor.Humanoid == null)
            return;

        ProfileEditor.Humanoid = ProfileEditor.Humanoid.WithCharacterAppearance(ProfileEditor.Humanoid.Appearance.WithMarkings(markings.GetForwardEnumerator().ToList()));
        ProfileEditor.UpdateSpriteView();
    }
    private void UpdateMarkings()
    {
        if (ProfileEditor.Humanoid == null)
            return;

        _markingPicker.SetData(ProfileEditor.Humanoid.Appearance.Markings, ProfileEditor.Humanoid.Species, ProfileEditor.Humanoid.Sex, ProfileEditor.Humanoid.Appearance.SkinColor, ProfileEditor.Humanoid.Appearance.EyeColor);
    }

    /// <summary>
    /// Completely refreshes all the controls and sets them to their expected values.
    /// </summary>
    public override void RefreshControls()
    {
        if (ProfileEditor.Humanoid == null)
            return;

        if (!_protoManager.TryIndex<SpeciesPrototype>(ProfileEditor.Humanoid.Species, out var speciesProto)) // Get the species, if possible.
            speciesProto = _protoManager.Index<SpeciesPrototype>(HumanoidCharacterProfile.Random().Species); // This is for worst case senarios, which hopefully should never happen.

        _nameEdit.Text = ProfileEditor.Humanoid.Name;
        _ageEdit.Text = ProfileEditor.Humanoid.Age.ToString();

        // Add the sexes.
        _sexButton.Clear(); // Oh wait shit remove them all first.

        var sexList = new List<Sex>();

        foreach (var sex in speciesProto.Sexes)
        {
            _sexButton.AddItem(Loc.GetString($"humanoid-profile-editor-sex-{sex.ToString().ToLower()}-text"), (int) sex);
            sexList.Add(sex);
        }

        if (sexList.Contains(ProfileEditor.Humanoid.Sex))
            _sexButton.SelectId((int) ProfileEditor.Humanoid.Sex);
        else
            _sexButton.SelectId((int) sexList[0]);

        SetGender(ProfileEditor.Humanoid.Gender);

        var skinTone = speciesProto.SkinColoration;

        if (skinTone == HumanoidSkinColor.HumanToned)
        {
            _skinColour.Visible = true;
            _rgbSkinColour.Visible = false;

            _skinColour.Value = SkinColor.HumanSkinToneFromColor(ProfileEditor.Humanoid.Appearance.SkinColor);
        }
        else
        {
            _skinColour.Visible = false;
            _rgbSkinColour.Visible = true;

            _rgbSkinColour.Color = ProfileEditor.Humanoid.Appearance.SkinColor;
        }

        _eyeColour.Color = ProfileEditor.Humanoid.Appearance.EyeColor;

        SetSkinColour();
        SetEyeColour();

        SetOutfit(ProfileEditor.Humanoid.Clothing);
        SetBackpack(ProfileEditor.Humanoid.Backpack);

        UpdateHair();
        UpdateFacialHair();

        UpdateMarkings();
    }
}
