using System.Linq;
using System.Numerics;
using Content.Client.Humanoid;
using Content.Client.Lobby.UI;
using Content.Client.Preferences;
using Content.Client.Stylesheets;
using Content.Client.Thanatophobia.Preferences.UI.CustomControls;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client.Thanatophobia.Preferences.UI;

public sealed partial class TPHumanoidProfileEditor : BoxContainer
{
    // Dependencies
    private readonly IPrototypeManager _prototypeManager;
    private readonly IEntityManager _entityManager;
    private readonly IClientPreferencesManager _preferencesManager;

    // Generic variables
    public HumanoidCharacterProfile? Humanoid;
    private EntityUid? _previewDummy;
    private SpriteView _spriteView;

    public Action? ProfileUpdated;
    private bool _showClothes = true;

    /// <summary>
    /// A list of all the controls used for modifying the user's character profile.
    /// </summary>
    private List<TPBaseCustomisationControl> _customisationControls;

    public TPHumanoidProfileEditor(IEntityManager entityManager,
            IClientPreferencesManager preferencesManager,
            IPrototypeManager prototypeManager)
    {
        HorizontalExpand = true;
        VerticalExpand = true;

        _prototypeManager = prototypeManager;
        _preferencesManager = preferencesManager;
        _entityManager = entityManager;

        _customisationControls = new()
        {
            new TPAppearanceCustomisation(this),
            new TPSpeciesTraitsCustomisation(this),
        };

        _spriteView = new()
        {
            Scale = new Vector2(4f, 4f),
            Margin = new Thickness(15, 15, 15, 15),
            OverrideDirection = Direction.South,
        };

        var spriteBack = new PanelContainer()
        {
            PanelOverride = new StyleBoxFlat(Color.Black.WithAlpha(0.25f)),
            HorizontalAlignment = HAlignment.Center,
            XamlChildren = { _spriteView },
            MinSize = new Vector2(160, 160),
            Margin = new Thickness(5, 15, 5, 5),
        };

        var divider = new PanelContainer()
        {
            PanelOverride = new StyleBoxFlat(StyleNano.NanoGold)
            {
                ContentMarginLeftOverride = 2,
            },
        };

        var clothingToggle = new Button()
        {
            Text = Loc.GetString("tp-humanoid-profile-editor-clothing-toggle")
        };
        clothingToggle.OnPressed += _ =>
        {
            _showClothes = !_showClothes;
            UpdateSpriteView(true);
        };

        var saveButton = new HoldButton()
        {
            Text = Loc.GetString("humanoid-profile-editor-save-button"),
            HoldTime = TimeSpan.FromSeconds(.5),
        };

        saveButton.Timeout += Save;

        var randomizeButton = new HoldButton()
        {
            Text = Loc.GetString("humanoid-profile-editor-randomize-everything-button"),
            HoldTime = TimeSpan.FromSeconds(.5),
        };

        randomizeButton.Timeout += () =>
        {
            ChangeCharacterProfile(HumanoidCharacterProfile.Random());
        };

        var characterPreviewPanel = new BoxContainer()
        {
            Orientation = LayoutOrientation.Vertical,
            MinSize = new Vector2(200, 0),

            XamlChildren =
            {
                spriteBack,
                clothingToggle,
                randomizeButton,
                saveButton
            },
        };

        var customisationPanel = new TabContainer()
        {
            Margin = new Thickness(5),
            VerticalExpand = true,
            HorizontalExpand = true,
        };
        customisationPanel.OnTabChanged += index => ((TPBaseCustomisationControl) customisationPanel.XamlChildren.ToList()[index]).RefreshControls();

        var index = 0;

        foreach (var control in _customisationControls)
        {
            customisationPanel.XamlChildren.Add(control);
            customisationPanel.SetTabTitle(index, control.TabName);
            index++;
        }

        ChangeCharacterProfile();

        AddChild(characterPreviewPanel);
        AddChild(divider);
        AddChild(customisationPanel);
    }

    /// <summary>
    /// Use this when randomizing characters, changing species or just simply changing which character is selected.
    /// </summary>
    /// <param name="profile"></param>
    public void ChangeCharacterProfile(ICharacterProfile? profile = null)
    {
        if (profile == null && _preferencesManager.Preferences != null)
            profile = _preferencesManager.Preferences.SelectedCharacter;

        Humanoid = profile as HumanoidCharacterProfile ?? HumanoidCharacterProfile.DefaultWithSpecies();

        UpdateSpriteView(true);

        foreach (var control in _customisationControls)
            control.RefreshControls();
    }

    /// <summary>
    /// Use this for anything that changes the appearance of the entity.
    /// </summary>
    /// <param name="fullUpdate">If set to true, it deletes the old entity used and creates a new one. Only use this for showing/hiding clothing or changing species.</param>
    public void UpdateSpriteView(bool fullUpdate = false)
    {
        if (Humanoid == null)
            return;

        if (fullUpdate || _previewDummy == null)
        {
            var species = Humanoid.Species ?? SharedHumanoidAppearanceSystem.DefaultSpecies;
            var dollProto = _prototypeManager.Index<SpeciesPrototype>(species).DollPrototype;

            if (_previewDummy != null)
                _entityManager.DeleteEntity(_previewDummy!.Value);

            _previewDummy = _entityManager.SpawnEntity(dollProto, MapCoordinates.Nullspace);

            if (_showClothes)
                LobbyCharacterPreviewPanel.GiveDummyJobClothes(_previewDummy.Value, Humanoid);
        }

        _entityManager.System<HumanoidAppearanceSystem>().LoadProfile(_previewDummy.Value, Humanoid);

        _spriteView.SetEntity(_previewDummy);
    }

    public void Save()
    {
        if (Humanoid != null)
            _preferencesManager.UpdateCharacter(Humanoid, _preferencesManager.Preferences!.SelectedCharacterIndex);

        ProfileUpdated!.Invoke();
    }
}
