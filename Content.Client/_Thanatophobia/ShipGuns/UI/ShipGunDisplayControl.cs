using System.Linq;
using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Shared.Shuttles.Components;
using Content.Shared.Thanatophobia.ShipGuns;
using Content.Shared.Thanatophobia.ShipGuns.Ammo;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Collections;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Dynamics;
using Serilog;

namespace Content.Client.Shuttles.UI;

[Virtual]
public class ShipGunDisplayControl : MapGridControl
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IEntitySystemManager _entSysManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    private readonly MapSystem _mapSystem = default!;
    private readonly SharedTransformSystem _transform;

    private const float GridLinesDistance = 32f;
    public new const int UIDisplayRadius = 210;
    protected new int ScaledMinimapRadius => (int) (UIDisplayRadius * UIScale);
    protected new float MinimapScale => WorldRange != 0 ? ScaledMinimapRadius / WorldRange : 0f;
    protected new int SizeFull => (int) ((UIDisplayRadius + MinimapMargin) * 2 * UIScale);
    protected new int MidPoint => SizeFull / 2;
    protected new Vector2 MidpointVector => new(MidPoint, MidPoint);

    public List<ShipGunState> GunsInfo;

    /// <summary>
    /// Used to transform all of the radar objects. Typically is a shuttle console parented to a grid.
    /// </summary>
    private EntityCoordinates? _coordinates;

    private Angle? _rotation;

    /// <summary>
    /// Raised if the user left-clicks on the radar control with the relevant entitycoordinates.
    /// </summary>
    public Action<EntityCoordinates>? OnRadarClick;

    public ShipGunDisplayControl() : base(64f, 256f, 256f)
    {
        SetSize = new Vector2(SizeFull, SizeFull);

        _transform = _entManager.System<SharedTransformSystem>();
        _mapSystem = _entManager.System<MapSystem>();
        GunsInfo = new();
    }

    public void SetMatrix(EntityCoordinates? coordinates, Angle? angle)
    {
        _coordinates = coordinates;
        _rotation = angle;
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);

        if (_coordinates == null || _rotation == null || args.Function != EngineKeyFunctions.UIClick ||
            OnRadarClick == null)
        {
            return;
        }

        var a = InverseScalePosition(args.RelativePosition);
        var relativeWorldPos = new Vector2(a.X, -a.Y);
        relativeWorldPos = _rotation.Value.RotateVec(relativeWorldPos);
        var coords = _coordinates.Value.Offset(relativeWorldPos);
        OnRadarClick?.Invoke(coords);
    }

    /// <summary>
    /// Gets the entitycoordinates of where the mouseposition is, relative to the control.
    /// </summary>
    public EntityCoordinates GetMouseCoordinates(ScreenCoordinates screen)
    {
        if (_coordinates == null || _rotation == null)
        {
            return EntityCoordinates.Invalid;
        }

        var pos = screen.Position / UIScale - GlobalPosition;

        var a = InverseScalePosition(pos);
        var relativeWorldPos = new Vector2(a.X, -a.Y);
        relativeWorldPos = _rotation.Value.RotateVec(relativeWorldPos);
        var coords = _coordinates.Value.Offset(relativeWorldPos);
        return coords;
    }

    public void UpdateState(ShipGunConsoleBoundUIState ls)
    {
        WorldMaxRange = ls.MaxRange;

        if (WorldMaxRange < WorldRange)
        {
            ActualRadarRange = WorldMaxRange;
        }

        if (WorldMaxRange < WorldMinRange)
            WorldMinRange = WorldMaxRange;

        ActualRadarRange = Math.Clamp(ActualRadarRange, WorldMinRange, WorldMaxRange);

        GunsInfo = ls.GunsInfo;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        var fakeAA = new Color(0.08f, 0.08f, 0.08f);

        handle.DrawCircle(new Vector2(MidPoint, MidPoint), ScaledMinimapRadius + 1, fakeAA);
        handle.DrawCircle(new Vector2(MidPoint, MidPoint), ScaledMinimapRadius, Color.FromHex("#333333"));

        // No data
        if (_coordinates == null || _rotation == null)
        {
            return;
        }

        var gridLines = Color.FromHex("#555555");
        var gridLinesRadial = 8;
        var gridLinesEquatorial = (int) Math.Floor(WorldRange / GridLinesDistance);

        for (var i = 1; i < gridLinesEquatorial + 1; i++)
        {
            handle.DrawCircle(new Vector2(MidPoint, MidPoint), GridLinesDistance * MinimapScale * i, gridLines, false);
        }

        for (var i = 0; i < gridLinesRadial; i++)
        {
            Angle angle = Math.PI / gridLinesRadial * i;
            var aExtent = angle.ToVec() * ScaledMinimapRadius;
            handle.DrawLine(new Vector2(MidPoint, MidPoint) - aExtent, new Vector2(MidPoint, MidPoint) + aExtent, gridLines);
        }

        var metaQuery = _entManager.GetEntityQuery<MetaDataComponent>();
        var xformQuery = _entManager.GetEntityQuery<TransformComponent>();
        var fixturesQuery = _entManager.GetEntityQuery<FixturesComponent>();

        if (!xformQuery.TryGetComponent(_coordinates.Value.EntityId, out var xform)
            || xform.MapID == MapId.Nullspace)
        {
            return;
        }

        var (pos, rot) = _transform.GetWorldPositionRotation(xform);
        var offset = _coordinates.Value.Position;
        var offsetMatrix = Matrix3.CreateInverseTransform(pos, rot + _rotation.Value);

        // Draw our grid in detail
        var ourGridId = xform.GridUid;
        if (_entManager.TryGetComponent<MapGridComponent>(ourGridId, out var ourGrid) &&
            fixturesQuery.HasComponent(ourGridId.Value))
        {
            var ourGridMatrix = _transform.GetWorldMatrix(ourGridId.Value);
            Matrix3.Multiply(in ourGridMatrix, in offsetMatrix, out var matrix);

            DrawGrid(handle, matrix, ourGridId.Value, ourGrid, Color.MediumSpringGreen, true);
            DrawGuns(handle, matrix, ourGrid);
        }

        var invertedPosition = _coordinates.Value.Position - offset;
        invertedPosition.Y = -invertedPosition.Y;
        // Don't need to transform the InvWorldMatrix again as it's already offset to its position.

        // Draw radar position on the station
        handle.DrawCircle(ScalePosition(invertedPosition), 5f, Color.Lime);

        var shown = new HashSet<EntityUid>();

        // Draw other grids... differently
        foreach (var grid in _mapManager.FindGridsIntersecting(xform.MapID,
                     new Box2(pos - MaxRadarRangeVector, pos + MaxRadarRangeVector)))
        {
            var gUid = grid.Owner;
            if (gUid == ourGridId || !fixturesQuery.HasComponent(gUid))
                continue;

            _entManager.TryGetComponent<IFFComponent>(gUid, out var iff);

            // Hide it entirely.
            if (iff != null &&
                (iff.Flags & IFFFlags.Hide) != 0x0)
            {
                continue;
            }

            shown.Add(gUid);

            var gridMatrix = _transform.GetWorldMatrix(gUid);
            Matrix3.Multiply(in gridMatrix, in offsetMatrix, out var matty);
            var color = iff?.Color ?? IFFComponent.IFFColor;

            // Detailed view
            DrawGrid(handle, matty, gUid, grid, color, true);
            DrawGuns(handle, matty, grid);
        }
    }

    private void DrawGuns(DrawingHandleScreen handle, Matrix3 matrix, MapGridComponent grid)
    {
        const float gunScale = 2f;

        foreach (var gun in GunsInfo)
        {
            if (gun.GridUid != _entManager.GetNetEntity(grid.Owner))
                continue;

            var position = gun.LocalPos;
            var rotation = Matrix3.CreateRotation(gun.LocalRot);
            var ammo = gun.Ammo;
            var maxAmmo = gun.MaxAmmo;

            var uiPosition = matrix.Transform(position);

            if (uiPosition.Length() > WorldRange - gunScale)
                continue;

            var color = Color.DeepPink;

            if (maxAmmo != 0)
                color = Color.InterpolateBetween(Color.Yellow, Color.DarkRed, (float) (maxAmmo - ammo) / maxAmmo);

            var verts = new[]
            {
                matrix.Transform(position + rotation.Transform(new Vector2(-gunScale / 1.5f, gunScale))),
                matrix.Transform(position + rotation.Transform(new Vector2(gunScale / 1.5f, gunScale))),
                matrix.Transform(position + rotation.Transform(new Vector2(0, -gunScale)))
            };

            for (var i = 0; i < verts.Length; i++)
            {
                var vert = verts[i];
                vert.Y = -vert.Y;
                verts[i] = ScalePosition(vert);
            }

            handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, verts, color.WithAlpha(0.8f));
            handle.DrawPrimitives(DrawPrimitiveTopology.LineStrip, verts, color);
        }
    }

    private void DrawGrid(DrawingHandleScreen handle, Matrix3 matrix, EntityUid gridUid, MapGridComponent grid, Color color, bool drawInterior)
    {
        if (!_entManager.TryGetComponent<FixturesComponent>(gridUid, out var fixutreComp))
            return;

        foreach (var (_, fixture) in fixutreComp.Fixtures)
        {
            var shape = (PolygonShape) fixture.Shape;

            if (shape.VertexCount < 2)
                continue;

            var totalVerts = new List<Vector2>();

            for (var i = 0; i < shape.VertexCount; i++)
            {
                var vertex = shape.Vertices[i];
                var radarVertex = matrix.Transform(vertex);

                if (radarVertex.Length() > ActualRadarRange)
                    continue;

                radarVertex.Y = -radarVertex.Y;

                var newVertex = ScalePosition(radarVertex);

                if (i < shape.VertexCount - 1)
                {
                    var nextVertex = matrix.Transform(shape.Vertices[i + 1]);
                    nextVertex.Y = -nextVertex.Y;
                    var nextRadarVertex = ScalePosition(nextVertex);
                    handle.DrawLine(newVertex, nextRadarVertex, color);
                }
                else
                {
                    var nextVertex = matrix.Transform(shape.Vertices[0]);
                    nextVertex.Y = -nextVertex.Y;
                    var nextRadarVertex = ScalePosition(nextVertex);
                    handle.DrawLine(newVertex, nextRadarVertex, color);
                }
                totalVerts.Add(newVertex);
            }
            handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, totalVerts.ToArray(), color.WithAlpha(0.5f));
        }
    }

    private Vector2 ScalePosition(Vector2 value)
    {
        return value * MinimapScale + MidpointVector;
    }

    private Vector2 InverseScalePosition(Vector2 value)
    {
        return (value - MidpointVector) / MinimapScale;
    }
}
