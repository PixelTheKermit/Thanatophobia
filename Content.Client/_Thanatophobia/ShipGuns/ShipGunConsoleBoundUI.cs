using System.Linq;
using System.Numerics;
using Content.Client.Shuttles.UI;
using Content.Client.UserInterface.Controls;
using Content.Shared.Thanatophobia.ShipGuns;
using FastAccessors;
using Robust.Client.GameObjects;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Client.Thanatophobia.ShipGuns;

public sealed partial class ShipGunConsoleBoundUI : BoundUserInterface
{
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IEntitySystemManager _entSysManager = default!;

    private bool _rightClickHeld = false;
    private bool _leftClickHeld = false;

    private ShipGunConsoleWindow? _consoleUI;

    public ShipGunConsoleBoundUI(EntityUid uid, Enum uiKey) : base(uid, uiKey)
    {

    }

    protected override void Open()
    {
        base.Open();

        _consoleUI = new(this);
        _consoleUI.OpenCentered();

        _consoleUI.Radar.OnMouseEntered += OnRadarMouseEntered;
        _consoleUI.Radar.OnMouseExited += OnRadarMouseExited;
        _consoleUI.Radar.OnKeyBindDown += OnKeyPressed;
        _consoleUI.Radar.OnKeyBindUp += OnKeyReleased;
        _consoleUI.RelayMousePos += UpdateMousePos;

        _consoleUI.OnClose += OnWindowClose;
    }

    private void OnWindowClose()
    {
        Close();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _rightClickHeld = false;
        _consoleUI?.Dispose();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_consoleUI == null)
            return;

        if (state is not ShipGunConsoleBoundUIState cState)
            return;

        _consoleUI.SetMatrix(EntMan.GetCoordinates(cState.Coordinates), cState.Angle);
        _consoleUI.UpdateState(cState);
    }

    private void OnRadarMouseEntered(GUIMouseHoverEventArgs args)
    {
        if (_consoleUI == null)
            return;
    }

    private void OnRadarMouseExited(GUIMouseHoverEventArgs args)
    {
        if (_consoleUI == null)
            return;
    }

    private void UpdateMousePos()
    {
        if (_consoleUI == null)
            return;


        if (_rightClickHeld)
        {
            var coords = RadarToWorldCoords(_consoleUI, _inputManager.MouseScreenPosition.Position / _consoleUI.Radar.UIScale - _consoleUI.Radar.GlobalPosition);
            SendPredictedMessage(new ShipGunConsolePosMessage(EntMan.GetNetCoordinates(coords)));
        }
        if (_leftClickHeld)
        {
            SendPredictedMessage(new ShipGunConsoleShootMessage());
        }
    }

    private EntityCoordinates RadarToWorldCoords(ShipGunConsoleWindow consoleUI, Vector2 mousePos)
    {
        var sizeFull = (int) ((MapGridControl.UIDisplayRadius + 4) * 2 * consoleUI.Radar.UIScale);
        var midPoint = sizeFull / 2;
        Vector2 midpointVector = new(midPoint, midPoint);

        var scaledMinimapRadius = (int) (MapGridControl.UIDisplayRadius * consoleUI.Radar.UIScale);
        var minimapScale = consoleUI.Radar.WorldRange != 0 ? scaledMinimapRadius / consoleUI.Radar.WorldRange : 0f;

        var a = (mousePos - midpointVector) / minimapScale;
        var relativeWorldPos = new Vector2(a.X, -a.Y);
        relativeWorldPos = consoleUI.Rotation!.Value.RotateVec(relativeWorldPos);
        var coords = consoleUI.Coordinates!.Value.Offset(relativeWorldPos);

        return coords;
    }

    private void OnKeyPressed(GUIBoundKeyEventArgs args)
    {
        if (_consoleUI == null)
            return;

        if (args.Function == EngineKeyFunctions.UIRightClick)
        {
            _rightClickHeld = true;
            var coords = RadarToWorldCoords(_consoleUI, args.RelativePosition);
            SendPredictedMessage(new ShipGunConsolePosMessage(EntMan.GetNetCoordinates(coords)));
        }
        if (args.Function == EngineKeyFunctions.UIClick)
        {
            SendPredictedMessage(new ShipGunConsoleShootMessage());
            _leftClickHeld = true;
        }
    }

    private void OnKeyReleased(GUIBoundKeyEventArgs args)
    {
        if (_consoleUI == null)
            return;

        if (args.Function == EngineKeyFunctions.UIClick)
        {
            SendPredictedMessage(new ShipGunConsoleUnshootMessage());
            _leftClickHeld = false;
        }
        if (args.Function == EngineKeyFunctions.UIRightClick)
        {
            _rightClickHeld = false;
        }
    }
}

