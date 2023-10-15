using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Shared.Shuttles.Components;
using Content.Shared.Thanatophobia.ShipGuns;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Collections;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;

namespace Content.Client.Shuttles.UI;

[Virtual]
public class ShipGunDisplayControl : MapGridControl
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IEntitySystemManager _entSysManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
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
        handle.DrawCircle(new Vector2(MidPoint, MidPoint), ScaledMinimapRadius, Color.Black);

        // No data
        if (_coordinates == null || _rotation == null)
        {
            return;
        }

        var gridLines = new Color(0.08f, 0.08f, 0.08f);
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

            DrawGrid(handle, matrix, ourGrid, Color.MediumSpringGreen, true);
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
            var color = iff?.Color ?? Color.Gold;

            // Detailed view
            DrawGrid(handle, matty, grid, color, true);
            DrawGuns(handle, matty, grid);
        }
    }

    private void DrawGuns(DrawingHandleScreen handle, Matrix3 matrix, MapGridComponent grid)
    {
        const float gunScale = 2f;

        foreach (var gun in GunsInfo)
        {
            Vector2 position;
            Matrix3 rotation;
            var ammo = 0;
            var maxAmmo = 0;

            if (_entManager.EntityExists(_entManager.GetEntity(gun.Uid))) // Let the client do it: It's going to update faster.
            {
                if (!_entManager.TryGetComponent<TransformComponent>(_entManager.GetEntity(gun.Uid), out var xform)
                || xform.GridUid != grid.Owner)
                {
                    continue;
                }

                position = xform.LocalPosition;
                rotation = Matrix3.CreateRotation(xform.LocalRotation);

                var ev = new GetAmmoCountEvent();
                _entManager.EventBus.RaiseLocalEvent(_entManager.GetEntity(gun.Uid), ref ev);

                ammo = ev.Count;
                maxAmmo = ev.Capacity;
            }
            else
            {
                if (_entManager.GetEntity(gun.GridUid) != grid.Owner)
                    continue;

                position = gun.LocalPos;
                rotation = Matrix3.CreateRotation(gun.LocalRot);
            }

            var uiPosition = matrix.Transform(position);

            if (uiPosition.Length() > WorldRange - gunScale)
                continue;

            var color = Color.DeepPink;

            if (maxAmmo != 0)
                color = Color.InterpolateBetween(Color.Yellow, Color.DarkRed, (float) (maxAmmo - ammo) / maxAmmo);

            var verts = new[]
            {
                matrix.Transform(position + rotation.Transform(new Vector2(-gunScale, gunScale))),
                matrix.Transform(position + rotation.Transform(new Vector2(gunScale, gunScale))),
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

    private void DrawGrid(DrawingHandleScreen handle, Matrix3 matrix, MapGridComponent grid, Color color, bool drawInterior)
    {
        var rator = grid.GetAllTilesEnumerator();
        var edges = new ValueList<Vector2>();

        while (rator.MoveNext(out var tileRef))
        {
            // TODO: Short-circuit interior chunk nodes
            // This can be optimised a lot more if required.
            Vector2? tileVec = null;

            // Iterate edges and see which we can draw
            for (var i = 0; i < 4; i++)
            {
                var dir = (DirectionFlag) Math.Pow(2, i);
                var dirVec = dir.AsDir().ToIntVec();

                if (!grid.GetTileRef(tileRef.Value.GridIndices + dirVec).Tile.IsEmpty)
                    continue;

                Vector2 start;
                Vector2 end;
                tileVec ??= (Vector2) tileRef.Value.GridIndices * grid.TileSize;

                // Draw line
                // Could probably rotate this but this might be faster?
                switch (dir)
                {
                    case DirectionFlag.South:
                        start = tileVec.Value;
                        end = tileVec.Value + new Vector2(grid.TileSize, 0f);
                        break;
                    case DirectionFlag.East:
                        start = tileVec.Value + new Vector2(grid.TileSize, 0f);
                        end = tileVec.Value + new Vector2(grid.TileSize, grid.TileSize);
                        break;
                    case DirectionFlag.North:
                        start = tileVec.Value + new Vector2(grid.TileSize, grid.TileSize);
                        end = tileVec.Value + new Vector2(0f, grid.TileSize);
                        break;
                    case DirectionFlag.West:
                        start = tileVec.Value + new Vector2(0f, grid.TileSize);
                        end = tileVec.Value;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                var adjustedStart = matrix.Transform(start);
                var adjustedEnd = matrix.Transform(end);

                if (adjustedStart.Length() > ActualRadarRange || adjustedEnd.Length() > ActualRadarRange)
                    continue;

                start = ScalePosition(new Vector2(adjustedStart.X, -adjustedStart.Y));
                end = ScalePosition(new Vector2(adjustedEnd.X, -adjustedEnd.Y));

                edges.Add(start);
                edges.Add(end);
            }
        }

        handle.DrawPrimitives(DrawPrimitiveTopology.LineList, edges.Span, color);
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
