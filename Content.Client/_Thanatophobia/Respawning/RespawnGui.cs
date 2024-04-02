
using System.Numerics;
using Content.Client.Message;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Shared.Ghost;
using Content.Shared.Thanatophobia.Respawning;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Timing;

namespace Content.Client.Thanatophobia.Respawning;

public sealed partial class TPRespawnGui : DefaultWindow
{
    [Dependency] private readonly IEntityNetworkManager _entNetManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private readonly BoxContainer _respawnContainer;
    private readonly Button _menuRespawnButton;

    public TPRespawnGui()
    {
        IoCManager.InjectDependencies(this);

        Title = Loc.GetString("tp-respawn-gui-title");

        MinSize = new Vector2(425, 500);
        MaxSize = new Vector2(425, 500);
        Resizable = false;

        Margin = new Thickness(1);

        var scrollContainer = new ScrollContainer()
        {
            Margin = new Thickness(1),
            VerticalExpand = true,
        };

        _menuRespawnButton = new Button()
        {
            Text = Loc.GetString("tp-respawn-ui-respawn-button"),
        };

        _menuRespawnButton.OnPressed += (args) =>
        {
            _entNetManager.SendSystemNetworkMessage(new SimpleRespawnUIMessage());
        };

        _respawnContainer = new()
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(4),
            MaxWidth = 390,
        };

        scrollContainer.XamlChildren.Add(new PanelContainer()
        {
            PanelOverride = new StyleBoxFlat() { BackgroundColor = Color.Black.WithAlpha(0.25f) },
            XamlChildren = { _respawnContainer }
        });

        var controlContainer = new BoxContainer()
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            XamlChildren = {
                scrollContainer
            }
        };

        XamlChildren.Add(controlContainer);

        UpdateRespawnContainer();
    }

    public void UpdateMenuRespawnButton()
    {
        if (!_entityManager.TryGetComponent<GhostComponent>(_playerManager.LocalEntity, out var ghostComp))
        {
            _menuRespawnButton.Disabled = true;
        }
        else if (_gameTiming.CurTime - ghostComp.TimeOfDeath <= TimeSpan.FromMinutes(5)) // Hardcoded for now. In the future should be a CVAR.
        {
            _menuRespawnButton.Disabled = true;
            _menuRespawnButton.Text = Loc.GetString("tp-respawn-ui-respawn-button-cooldown", ("cooldown", (int) Math.Ceiling((TimeSpan.FromMinutes(5) - (_gameTiming.CurTime - ghostComp.TimeOfDeath)).TotalSeconds)));
        }
        else
        {
            _menuRespawnButton.Disabled = false;
            _menuRespawnButton.Text = Loc.GetString("tp-respawn-ui-respawn-button");
        }
    }
    public void UpdateRespawnContainer()
    {
        _respawnContainer.XamlChildren.Clear();

        var coolLookinBox = new StyleBoxFlat()
        {
            BackgroundColor = Color.FromHex("#25252a"), // Fuck. I would perfer to pull this from the stylesheet itself, but for now this is fine. Stylesheet refactor when?
            BorderColor = StyleNano.NanoGold,
            BorderThickness = new Thickness(2),
        };

        var menuRespawn = new BoxContainer()
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Margin = new Thickness(3),
        };

        var menuRespawnBg = new PanelContainer()
        {
            PanelOverride = coolLookinBox,
            XamlChildren = { menuRespawn },
            Margin = new Thickness(2)
        };

        var menuRespawnTitle = new Label()
        {
            Text = Loc.GetString("tp-respawn-ui-menu-respawn-title"),
            FontColorOverride = StyleNano.NanoGold,
        };
        var menuRespawnDescription = new RichTextLabel();
        var shiftMenuRespawnButtonRight = new BoxContainer()
        {
            HorizontalAlignment = HAlignment.Right,
            XamlChildren = { _menuRespawnButton }
        };

        menuRespawnDescription.SetMarkup(Loc.GetString("tp-respawn-ui-menu-respawn-description"));
        menuRespawnTitle.AddStyleClass(StyleNano.StyleClassLabelHeading);

        _respawnContainer.XamlChildren.Add(menuRespawnBg);
        menuRespawn.XamlChildren.Add(menuRespawnTitle);
        menuRespawn.XamlChildren.Add(menuRespawnDescription);
        menuRespawn.XamlChildren.Add(shiftMenuRespawnButtonRight);
    }
}
