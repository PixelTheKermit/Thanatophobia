using System.Linq;
using System.Numerics;
using Content.Client.Message;
using Content.Client.Shuttles.UI;
using Content.Client.UserInterface.Controls;
using Content.Shared.Thanatophobia.ShipGuns;
using Robust.Client.Graphics;
using Robust.Client.Timing;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Client.Thanatophobia.ShipGuns;

public sealed partial class ShipGunConsoleWindow : FancyWindow
{
    [Dependency] private readonly IClientGameTiming _gameTiming;

    public ShipGunDisplayControl Radar;
    public GridContainer GroupBox;
    public Action? RelayMousePos;
    public Angle? Rotation;
    public EntityCoordinates? Coordinates;
    public RichTextLabel TotalGunsLabel;
    private readonly ShipGunConsoleBoundUI _boundUI;
    private TimeSpan _lastUpdate;
    private readonly float _messageDelay = 1f / 10;

    public ShipGunConsoleWindow(ShipGunConsoleBoundUI bui)
    {
        _gameTiming = IoCManager.Resolve<IClientGameTiming>();

        _lastUpdate = _gameTiming.CurTime;
        _boundUI = bui;

        Title = Loc.GetString("tp-ship-gun-console-ui");

        SetSize = new Vector2(695, 515);
        MinSize = new Vector2(695, 515);

        var allContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical
        };

        var displayContainer = new GridContainer
        {
            Columns = 2,
            HorizontalAlignment = HAlignment.Stretch,
            Margin = new Thickness(5, 5, 5, 5),
        };

        var rightDisplay = new BoxContainer
        {
            MinWidth = 256,
            MaxWidth = 256,
            Align = BoxContainer.AlignMode.Center,
            VerticalAlignment = VAlignment.Top,
            HorizontalAlignment = HAlignment.Right,
            Orientation = BoxContainer.LayoutOrientation.Vertical
        };

        var radarPanel = new PanelContainer
        {
            MinSize = new Vector2(128, 128),
            HorizontalAlignment = HAlignment.Left,
            VerticalExpand = true,
            HorizontalExpand = true,
        };

        Radar = new ShipGunDisplayControl
        {
            VerticalExpand = true,
            HorizontalExpand = true,
            Margin = new Thickness(4),
            MouseFilter = MouseFilterMode.Stop,
        };

        var backGroupLabel = new StripeBack();
        var groupLabel = new RichTextLabel
        {
            HorizontalAlignment = HAlignment.Center,
        };
        groupLabel.SetMarkup(Loc.GetString("tp-ship-gun-console-ui-groups"));

        TotalGunsLabel = new RichTextLabel()
        {
            HorizontalAlignment = HAlignment.Center,
        };
        TotalGunsLabel.SetMarkup(Loc.GetString("tp-ship-gun-console-ui-guns-in-group", ("count", "???")));

        GroupBox = new GridContainer
        {
            Columns = 2,
            HorizontalAlignment = HAlignment.Center
        };

        var footerBack = new StripeBack()
        {
            VerticalAlignment = VAlignment.Bottom,
        };

        var footerLabel = new RichTextLabel
        {
            HorizontalAlignment = HAlignment.Center,
        };
        footerLabel.SetMarkup(Loc.GetString("tp-ship-gun-console-ui-footer"));

        backGroupLabel.AddChild(groupLabel);
        radarPanel.AddChild(Radar);
        rightDisplay.AddChild(backGroupLabel);
        rightDisplay.AddChild(TotalGunsLabel);
        rightDisplay.AddChild(GroupBox);

        displayContainer.AddChild(radarPanel);
        displayContainer.AddChild(rightDisplay);
        allContainer.AddChild(displayContainer);

        footerBack.AddChild(footerLabel);
        allContainer.AddChild(footerBack);
        XamlChildren.Add(allContainer);
    }
    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_lastUpdate + TimeSpan.FromSeconds(_messageDelay) > _gameTiming.CurTime)
            return;

        _lastUpdate = _gameTiming.CurTime;

        RelayMousePos?.Invoke();
    }

    public void UpdateState(ShipGunConsoleBoundUIState scc)
    {
        Radar.UpdateState(scc);

        if (GroupBox.ChildCount != scc.PortCount || !((ShipGunGroupButton) GroupBox.GetChild(scc.CurGroup)).Disabled)
        {
            GroupBox.DisposeAllChildren();

            for (var i = 0; i < scc.PortCount; i++)
            {
                var groupButton = new ShipGunGroupButton(_boundUI)
                {
                    Text = Loc.GetString("tp-ship-gun-console-ui-group-text", ("group", i + 1)),
                    MinSize = new Vector2(50, 50),
                    Index = i,
                    Disabled = i == scc.CurGroup
                };

                GroupBox.AddChild(groupButton);
            }
        }

        TotalGunsLabel.SetMarkup(Loc.GetString("tp-ship-gun-console-ui-guns-in-group", ("count", scc.GunsInfo.Count)));
    }

    public void SetMatrix(EntityCoordinates? coordinates, Angle? angle)
    {
        Coordinates = coordinates;
        Rotation = angle;
        Radar.SetMatrix(coordinates, angle);
    }
}

public sealed partial class ShipGunGroupButton : Button
{
    private readonly ShipGunConsoleBoundUI _bui;
    public int Index = 0;

    public ShipGunGroupButton(ShipGunConsoleBoundUI bui)
    {
        _bui = bui;
        OnPressed += OnButtonPressed;
    }

    private void OnButtonPressed(ButtonEventArgs args)
    {
        _bui.SendMessage(new ShipGunConsoleSetGroupMessage(Index));
    }
}
