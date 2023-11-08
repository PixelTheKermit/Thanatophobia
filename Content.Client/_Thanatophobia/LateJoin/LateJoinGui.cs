using System.Numerics;
using Content.Client.Message;
using Content.Client.UserInterface.Controls;
using Content.Shared.Thanatophobia.CCVar;
using Content.Shared.Thanatophobia.LateJoin;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Serilog;

namespace Content.Client.Thanatophobia.LateJoin;

public sealed class TPLateJoinGui : FancyWindow
{
    private readonly IEntityNetworkManager _entityNetworkManager;
    private readonly IPrototypeManager _protoManager;
    private readonly IConfigurationManager _cfgManager;

    public GridContainer ShuttleContainer;
    public RichTextLabel ShuttleInfo;
    public string SelectedShip = "";

    public TPLateJoinGui()
    {
        _entityNetworkManager = IoCManager.Resolve<IEntityNetworkManager>();
        _protoManager = IoCManager.Resolve<IPrototypeManager>();
        _cfgManager = IoCManager.Resolve<IConfigurationManager>();

        MinSize = new Vector2(675, 450);
        MaxSize = new Vector2(675, 450);
        Resizable = false;

        Title = Loc.GetString("tp-late-join-ui-title");

        var purchaseButton = new Button
        {
            HorizontalExpand = true,
            Text = Loc.GetString("tp-purchase-ship-button"),
        };

        purchaseButton.OnPressed += PurchaseButtonPressed;

        ShuttleContainer = new GridContainer();

        var scrollContainer = new ScrollContainer
        {
            VerticalExpand = true,
            HorizontalExpand = true,
            Children = { ShuttleContainer }
        };

        var poolStr = _cfgManager.GetCVar(TPCCVars.ShipSpawnPool);

        if (_protoManager.TryIndex<ShipSpawnPoolPrototype>(poolStr, out var poolProto))
        {
            foreach (var ship in poolProto.Ships)
            {
                var shipButton = new Button
                {
                    Text = Loc.GetString($"tp-ui-ship-name-{ship.Key}"),
                    HorizontalExpand = true,
                };

                shipButton.OnPressed += _ => ShuttleSelectPressed(ship.Key);

                ShuttleContainer.AddChild(shipButton);
            }
        }

        ShuttleInfo = new RichTextLabel
        {
            VerticalAlignment = VAlignment.Top,
            MinWidth = 380,
            MaxWidth = 380,
        };
        ShuttleInfo.SetMarkup(Loc.GetString("tp-ui-request-ship-select"));

        var textScrollContainer = new ScrollContainer
        {
            HScrollEnabled = false,
            Children = { ShuttleInfo }
        };

        var leftDisplay = new BoxContainer()
        {
            MinWidth = 256,
            MaxWidth = 256,
            VerticalExpand = true,
            Children = { scrollContainer },
        };

        var textScrollBG = new PanelContainer()
        {
            VerticalExpand = true,
            PanelOverride = new StyleBoxFlat(Color.Black.WithAlpha(0.25f)),
            Children = { textScrollContainer },
        };

        var rightDisplay = new BoxContainer()
        {
            MinWidth = 400,
            MaxWidth = 400,
            VerticalExpand = true,
            HorizontalExpand = true,
            Children =
            {
                textScrollBG,
                purchaseButton
            },
            Orientation = BoxContainer.LayoutOrientation.Vertical
        };

        var displayContainer = new GridContainer()
        {
            Columns = 2,
            HorizontalAlignment = HAlignment.Stretch,
            Margin = new Thickness(5, 5, 5, 5),
            VSeparationOverride = 10,
            Children =
            {
                leftDisplay,
                rightDisplay
            }
        };

        XamlChildren.Add(displayContainer);
    }

    private void PurchaseButtonPressed(BaseButton.ButtonEventArgs args)
    {
        _entityNetworkManager.SendSystemNetworkMessage(new RoundStartTrySpawnShipUIMessage(SelectedShip));
    }

    private void ShuttleSelectPressed(string shuttleName)
    {
        SelectedShip = shuttleName;
        ShuttleInfo.SetMarkup(Loc.GetString($"tp-ui-ship-desc-{shuttleName}"));
    }
}
