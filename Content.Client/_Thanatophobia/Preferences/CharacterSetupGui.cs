using System.Linq;
using System.Numerics;
using Content.Client.Humanoid;
using Content.Client.Lobby.UI;
using Content.Client.Preferences;
using Content.Client.Stylesheets;
using Content.Shared.Hands.Components;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Content.Shared.Traits;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Timing;

namespace Content.Client.Thanatophobia.Preferences.UI;

public sealed partial class TPCharacterSetupGui : Control
{

    // Dependencies
    private readonly IClientPreferencesManager _preferencesManager;
    private readonly IEntityManager _entityManager;
    private readonly IPrototypeManager _prototypeManager;
    private readonly IConfigurationManager _configurationManager;

    // UI objects
    public Button CloseButton;
    public Button SaveButton;
    private TPHumanoidProfileEditor _profileEditor;
    private readonly BoxContainer _characterContainer;
    private readonly Button _newCharacterButton;

    public TPCharacterSetupGui(
            IEntityManager entityManager,
            IResourceCache resourceCache,
            IClientPreferencesManager preferencesManager,
            IPrototypeManager prototypeManager,
            IConfigurationManager configurationManager)
    {
        _preferencesManager = preferencesManager;
        _entityManager = entityManager;
        _prototypeManager = prototypeManager;
        _configurationManager = configurationManager;

        VerticalExpand = true;

        SaveButton = new()
        {
            HorizontalAlignment = HAlignment.Right,
            HorizontalExpand = true,
            StyleClasses = { StyleNano.StyleClassButtonBig },
            Text = Loc.GetString("character-setup-gui-character-setup-save-button"),
        };

        CloseButton = new()
        {
            Text = Loc.GetString("character-setup-gui-character-setup-close-button"),
            StyleClasses = { StyleNano.StyleClassButtonBig },
        };

        var topButtons = new BoxContainer()
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            MinSize = new Vector2(0, 60),
            XamlChildren = { SaveButton, CloseButton },
        };

        var titleSeperator = new PanelContainer()
        {
            PanelOverride = new StyleBoxFlat(StyleNano.NanoGold)
            {
                ContentMarginTopOverride = 2,
            },
        };

        var background = new PanelContainer()
        {
            StyleClasses = { StyleBase.ClassAngleRect },
        };

        #region Character List

        _characterContainer = new();

        _newCharacterButton = new()
        {
            StyleClasses = { StyleNano.StyleClassChatChannelSelectorButton },
            Text = Loc.GetString("character-setup-gui-create-new-character-button"),
        };

        _newCharacterButton.OnPressed += args =>
        {
            preferencesManager.CreateCharacter(HumanoidCharacterProfile.Random());
            UpdateUI();
            args.Event.Handle();
        };

        var characterScroll = new ScrollContainer()
        {
            MinSize = new Vector2(0, 125),
            Margin = new Thickness(5, 5, 5, 10),
            XamlChildren =
            {
                new BoxContainer()
                {
                    XamlChildren = { _characterContainer, _newCharacterButton }
                }
            },
            HScrollEnabled = true,
            VScrollEnabled = false
        };


        var characterSeperator = new PanelContainer()
        {
            PanelOverride = new StyleBoxFlat(StyleNano.NanoGold)
            {
                ContentMarginTopOverride = 2,
            },
        };

        _profileEditor = new(entityManager, preferencesManager, prototypeManager);

        _profileEditor.ProfileUpdated += UpdateUI;

        #endregion

        var elementContainer = new BoxContainer()
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 0,
            XamlChildren =
            {
                topButtons,
                titleSeperator,
                characterScroll,
                characterSeperator,
                _profileEditor
            },
        };

        XamlChildren.Add(background);
        XamlChildren.Add(elementContainer);

        UpdateUI();
    }

    public void UpdateUI()
    {
        var numberOfFullSlots = 0;
        _characterContainer.RemoveAllChildren();

        if (!_preferencesManager.ServerDataLoaded)
        {
            return;
        }

        foreach (var (slot, character) in _preferencesManager.Preferences!.Characters)
        {
            if (character is null)
            {
                continue;
            }

            numberOfFullSlots++;
            var characterPickerButton = new CharacterPickerButton(
                _entityManager,
                _preferencesManager,
                _prototypeManager,
                character);
            _characterContainer.AddChild(characterPickerButton);

            if (character == _preferencesManager.Preferences!.SelectedCharacter)
                _profileEditor.ChangeCharacterProfile();

            var characterIndexCopy = slot;
            characterPickerButton.OnPressed += args =>
            {
                _preferencesManager.SelectCharacter(character);
                UpdateUI();
                args.Event.Handle();
            };
        }

        _newCharacterButton.Disabled = numberOfFullSlots >= _preferencesManager.Settings!.MaxCharacterSlots;
    }

    public void Save()
    {
        _profileEditor.Save();
    }

    // Other UI elements.
    private sealed partial class CharacterPickerButton : Button
    {
        /// <summary>
        /// The entity dummy used for the appearance of the character.
        /// </summary>
        private readonly EntityUid _previewDummy;

        public CharacterPickerButton(
            IEntityManager entityManager,
            IClientPreferencesManager preferencesManager,
            IPrototypeManager prototypeManager,
            ICharacterProfile profile)
        {
            var serializationManager = IoCManager.Resolve<ISerializationManager>();
            var markingManager = IoCManager.Resolve<MarkingManager>();
            var humanoidAppearanceSystem = entityManager.System<HumanoidAppearanceSystem>();

            AddStyleClass(StyleNano.StyleClassChatChannelSelectorButton);
            ToggleMode = true;
            Margin = new Thickness(0, 0, 0, 0);

            var humanoid = profile as HumanoidCharacterProfile ?? HumanoidCharacterProfile.DefaultWithSpecies();

            var speciesProto = prototypeManager.Index<SpeciesPrototype>(humanoid.Species);

            var dummy = speciesProto.DollPrototype;
            _previewDummy = entityManager.SpawnEntity(dummy, MapCoordinates.Nullspace);

            humanoidAppearanceSystem.LoadProfile(_previewDummy, (HumanoidCharacterProfile) profile);

            var isSelectedCharacter = profile == preferencesManager.Preferences?.SelectedCharacter;

            var viewSprite = new SpriteView()
            {
                Scale = new Vector2(2.5f, 2.5f),
                Margin = new Thickness(15, 15, 15, 15),
                OverrideDirection = Direction.South,
                VerticalAlignment = VAlignment.Bottom,
            };

            LobbyCharacterPreviewPanel.GiveDummyJobClothes(_previewDummy, humanoid);

            // Add all the traits.
            foreach (var traitId in humanoid.TraitPreferences)
            {
                if (!prototypeManager.TryIndex<TraitPrototype>(traitId, out var traitPrototype))
                    continue;

                // Add all components required by the prototype
                foreach (var entry in traitPrototype.Components.Values)
                {
                    var comp = serializationManager.CreateCopy(entry.Component, notNullableOverride: true);
                    comp.Owner = _previewDummy;
                    entityManager.AddComponent(_previewDummy, comp, true);
                }

                if (traitPrototype.MarkingId != null
                && prototypeManager.TryIndex<MarkingPrototype>(traitPrototype.MarkingId, out var markingProto))
                {
                    var colors = MarkingColoring.GetMarkingLayerColors(markingProto, humanoid.Appearance.SkinColor, humanoid.Appearance.EyeColor, new MarkingSet());
                    var dictColors = colors.ToDictionary(x => colors.IndexOf(x));

                    for (var i = 0; i < traitPrototype.MarkingColours.Count; i++)
                        dictColors[i] = traitPrototype.MarkingColours[i];

                    humanoidAppearanceSystem.AddMarking(_previewDummy, traitPrototype.MarkingId, dictColors.Values.ToList(), true, true);
                }
            }

            if (entityManager.TryGetComponent<SpriteComponent>(_previewDummy, out var spriteComp)
            && entityManager.TryGetComponent<HumanoidAppearanceComponent>(_previewDummy, out var appearanceComp))
            {
                humanoidAppearanceSystem.UpdateSprite(appearanceComp, spriteComp);
            }

            viewSprite.SetEntity(_previewDummy);

            var viewControl = new Control()
            {
                MinSize = new Vector2(135, 135),
                XamlChildren = { viewSprite }
            };

            var elementContainer = new BoxContainer()
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                XamlChildren = { viewControl },
                HorizontalExpand = true,
            };

            if (isSelectedCharacter)
            {
                Pressed = true;

                var charName = profile.Name;
                var speciesName = Loc.GetString(speciesProto.Name);
                var ageString = Loc.GetString("tp-humanoid-age", ("age", humanoid.Age));

                var pronounString = humanoid.Gender switch
                {
                    Gender.Neuter => Loc.GetString("humanoid-profile-editor-pronouns-neuter-text"),
                    Gender.Epicene => Loc.GetString("humanoid-profile-editor-pronouns-epicene-text"),
                    Gender.Female => Loc.GetString("humanoid-profile-editor-pronouns-female-text"),
                    Gender.Male => Loc.GetString("humanoid-profile-editor-pronouns-male-text"),
                    _ => Loc.GetString("humanoid-profile-editor-pronouns-neuter-text"),
                };

                var nameLabel = new Label()
                {
                    Text = charName,
                    HorizontalExpand = true,
                };

                var speciesLabel = new Label()
                {
                    Text = speciesName,
                    HorizontalExpand = true,
                };

                var ageLabel = new Label()
                {
                    Text = ageString,
                    HorizontalExpand = true,
                };

                var pronounLabel = new Label()
                {
                    Text = pronounString,
                    HorizontalExpand = true,
                };

                var descriptionContainer = new BoxContainer()
                {
                    Orientation = BoxContainer.LayoutOrientation.Vertical,
                    XamlChildren = { nameLabel, speciesLabel, ageLabel, pronounLabel },
                    HorizontalExpand = true,
                };


                elementContainer.XamlChildren.Add(descriptionContainer);
            }
            else
            {
                var deleteButton = new HoldButton()
                {
                    Text = Loc.GetString("character-setup-gui-character-picker-button-delete-button"),
                    VerticalAlignment = VAlignment.Top,
                    HorizontalAlignment = HAlignment.Right,
                    ModulateSelfOverride = StyleNano.ButtonColorCautionDefault,
                    StyleClasses = { StyleNano.StyleClassChatChannelSelectorButton },
                    Margin = new Thickness(0, -2, -5, 0),
                };
                deleteButton.HoldProgress.ForegroundStyleBoxOverride = new StyleBoxFlat(StyleNano.ButtonColorCautionDefault);

                deleteButton.Timeout += () =>
                {
                    Dispose();
                    preferencesManager.DeleteCharacter(profile);
                };

                XamlChildren.Add(deleteButton);
            }

            XamlChildren.Add(elementContainer);
        }
    }
}

public sealed partial class HoldButton : Button
{
    // Dependencies
    private readonly IGameTiming _gameTiming;

    // Controls
    public ProgressBar HoldProgress;

    // Variables
    private TimeSpan _lastTimeClicked = TimeSpan.Zero;
    public TimeSpan HoldTime = TimeSpan.FromSeconds(2);
    public Action? Timeout;
    private bool _isHeld;

    public HoldButton()
    {
        _gameTiming = IoCManager.Resolve<IGameTiming>();

        HoldProgress = new()
        {
            MaxValue = (float) HoldTime.TotalSeconds,
            Visible = false,
        };

        OnButtonDown += IsHeld;
        OnButtonUp += IsUnheld; // Can you tell I did terribly in English?

        AddChild(HoldProgress);
    }

    private void IsHeld(ButtonEventArgs args)
    {
        _lastTimeClicked = _gameTiming.CurTime;
        HoldProgress.SetWidth = Label.Width;
        HoldProgress.MaxHeight = Label.Height;
        HoldProgress.Visible = true;
        Label.Visible = false;
        HoldProgress.Value = 0;
        HoldProgress.MaxValue = (float) HoldTime.TotalSeconds;

        _isHeld = true;
    }

    private void IsUnheld(ButtonEventArgs args)
    {
        Label.Visible = true;
        HoldProgress.Visible = false;

        _isHeld = false;

        if (_gameTiming.CurTime - _lastTimeClicked >= HoldTime) // I would do this in FrameUpdate but I would actually die from the fury.
            Timeout?.Invoke();
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        if (_isHeld)
            HoldProgress.Value = (float) (_gameTiming.CurTime - _lastTimeClicked).TotalSeconds;
    }
}
