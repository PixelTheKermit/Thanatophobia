using System.Linq;
using System.Numerics;
using Content.Server.Administration.Logs;
using Content.Server.Audio;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Server.Procedural;
using Content.Server.Pulling;
using Content.Shared.Administration.Logs;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.DragDrop;
using Content.Shared.Gravity;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Procedural;
using Content.Shared.Thanatophobia.DungeonGate;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Console;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Thanatophobia.DungeonGate;

public sealed partial class DungeonGateSystem : SharedDungeonGateSystem
{
    [Dependency] private readonly DungeonSystem _dungeonSystem = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly AnchorableSystem _anchorSystem = default!;
    [Dependency] private readonly SharedTransformSystem _xformSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly DoAfterSystem _doafterSystem = default!;
    [Dependency] private readonly IConsoleHost _consoleHost = default!;
    [Dependency] private readonly PullingSystem _pullingSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DungeonGateComponent, InteractedNoHandEvent>(OnGateInteract);
        SubscribeLocalEvent<DungeonGateComponent, InteractHandEvent>(OnGateInteract);

        SubscribeLocalEvent<DungeonGateComponent, DragDropTargetEvent>(OnGateDragDropInteract);
        SubscribeLocalEvent<DungeonGateComponent, CanDropTargetEvent>(OnCanDragDropOn);
        SubscribeLocalEvent<DungeonGateComponent, DungeonGateDragDropDoAfterEvent>(AfterGateDragDrop);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var queriedEnts = EntityManager.AllEntityQueryEnumerator<DungeonGateComponent>();

        var curTime = _gameTiming.CurTime;

        while (queriedEnts.MoveNext(out var uid, out var comp))
        {
            if (comp.GateState == DungeonGateState.InProgress &&
                comp.DeathTime <= curTime)
            {
                comp.GateState = DungeonGateState.Broken;
                _audioSystem.PlayPvs(comp.GateBreakSound, uid);
                Dirty(uid, comp);

                if (HasComp<DeleteMapOnGateBreakComponent>(uid))
                    _mapManager.DeleteMap(Transform(uid).MapID);
            }
        }
    }

    private void OnGateInteract(EntityUid uid, DungeonGateComponent component, InteractedNoHandEvent args)
    {
        OnGateInteract(uid, component, args.User);
    }

    private void OnGateInteract(EntityUid uid, DungeonGateComponent component, InteractHandEvent args)
    {
        OnGateInteract(uid, component, args.User);
    }

    private async void OnGateInteract(EntityUid gateUid, DungeonGateComponent gateComp, EntityUid userUid)
    {
        switch (gateComp.GateState)
        {
            case DungeonGateState.Ready:
                var curTime = _gameTiming.CurTime;

                if (!_protoManager.TryIndex(gateComp.DungeonConfig, out var dungeonConfig))
                {
                    Log.Warning($"Dungeon prototype is invalid! No such prototype named {gateComp.DungeonConfig.Id} found!");
                    return;
                }

                // We want somewhere to nest the dungeon.
                var newMapId = _mapManager.CreateMap();
                _mapManager.SetMapPaused(newMapId, true);
                var gridEnt = _mapManager.CreateGridEntity(newMapId);
                var gravity = EnsureComp<GravityComponent>(gridEnt);
                gravity.Inherent = true;
                gravity.Enabled = true;

                _popupSystem.PopupEntity(Loc.GetString(gateComp.GateOpenPopup), userUid, userUid);
                _audioSystem.PlayPvs(gateComp.GateOpenSound, gateUid);

                // We don't want this gate to start making 500 dungeons.
                gateComp.DeathTime = curTime + gateComp.TimeToEnter;
                gateComp.GateState = DungeonGateState.InProgress;

                await _dungeonSystem.GenerateDungeonAsync(dungeonConfig, gridEnt, gridEnt.Comp, Vector2i.Zero, _random.Next());

                _mapManager.SetMapPaused(newMapId, false);

                // For spawning the exit.
                var availableTiles = _mapSystem.GetAllTiles(gridEnt, gridEnt.Comp).ToList();
                _random.Shuffle(availableTiles);

                // Realistically, there shouldn't be a case where there is no valid tile... So a tile *will* be found.
                // The real question is: Is that tile really safe? (idfk lol.)
                foreach (var tile in availableTiles)
                {
                    if (_anchorSystem.TileFree(gridEnt.Comp, tile.GridIndices, (int) CollisionGroup.MachineLayer, (int) CollisionGroup.MachineLayer))
                    {
                        var otherPortal = Spawn(gateComp.ExitGateProto, new EntityCoordinates(gridEnt, tile.GridIndices));
                        gateComp.LeadsToEntity = otherPortal;

                        // We want the player to leave, no?
                        var otherPortalComp = EnsureComp<DungeonGateComponent>(otherPortal);
                        // We REALLY want the player to leave, no?
                        EnsureComp<DeleteMapOnGateBreakComponent>(otherPortal);

                        otherPortalComp.LeadsToEntity = gateUid;
                        otherPortalComp.GateState = DungeonGateState.InProgress;
                        otherPortalComp.FirstEnter = true;
                        // We probably want more time for anything inside the gate to exit when the main gate exits.
                        // This is so there aren't cases where someone enters a gate really late and then immediately cannot leave (tldr: it prevents fustration!)
                        otherPortalComp.DeathTime = curTime + gateComp.TimeToEnter + gateComp.TimeAddedToLeave;
                        Dirty(otherPortal, otherPortalComp);
                        break;
                    }
                }

                Dirty(gateUid, gateComp);
                break;
            case DungeonGateState.InProgress:
                if (gateComp.LeadsToEntity != null)
                {
                    // More hot spicy glue!
                    if (!_pullingSystem.IsPulling(userUid))
                    {
                        // Hot glue mess. Fucking hell.
                        if (!gateComp.FirstEnter && Transform(gateComp.LeadsToEntity.Value).GridUid != null)
                        {
                            _consoleHost.ExecuteCommand($"fixgridatmos {Transform(gateComp.LeadsToEntity.Value).GridUid!.Value.Id}");
                            gateComp.FirstEnter = true;
                        }

                        _xformSystem.SetCoordinates(userUid, Transform(gateComp.LeadsToEntity.Value).Coordinates);
                        _audioSystem.PlayPvs(gateComp.GateEnterSound, gateUid);
                        _audioSystem.PlayPvs(gateComp.GateEnterSound, gateComp.LeadsToEntity.Value);
                    }
                    else
                    {
                        var doAfterArgs = new DoAfterArgs(EntityManager, userUid, gateComp.DragDropTime, new DungeonGateDragDropDoAfterEvent(), gateUid, target: _pullingSystem.GetPulled(userUid), used: gateUid)
                        {
                            BreakOnTargetMove = true,
                            BreakOnUserMove = true,
                            BreakOnDamage = true,
                            NeedHand = true,
                            BreakOnHandChange = true
                        };

                        _doafterSystem.TryStartDoAfter(doAfterArgs);
                    }
                }
                break;
            case DungeonGateState.Broken:
                _popupSystem.PopupEntity(Loc.GetString(gateComp.GateBrokenPopup), userUid, userUid, Shared.Popups.PopupType.SmallCaution);
                break;
            default:
                Log.Warning("DungeonGateState has no code attributed to this current state.");
                break;
        }
    }

    private void OnGateDragDropInteract(EntityUid uid, DungeonGateComponent gateComp, DragDropTargetEvent args)
    {
        if (gateComp.GateState == DungeonGateState.InProgress)
        {
            if (gateComp.LeadsToEntity != null)
            {
                var doAfterArgs = new DoAfterArgs(EntityManager, args.User, gateComp.DragDropTime, new DungeonGateDragDropDoAfterEvent(), uid, target: args.Dragged, used: uid)
                {
                    BreakOnTargetMove = true,
                    BreakOnUserMove = true,
                    BreakOnDamage = true,
                    NeedHand = true,
                    BreakOnHandChange = true
                };

                _doafterSystem.TryStartDoAfter(doAfterArgs);
            }
        }
    }

    private void AfterGateDragDrop(EntityUid uid, DungeonGateComponent gateComp, DungeonGateDragDropDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target == null)
            return;

        if (gateComp.GateState == DungeonGateState.InProgress)
        {
            if (gateComp.LeadsToEntity != null)
            {
                // Hot glue mess. Fucking hell.
                if (!gateComp.FirstEnter && Transform(gateComp.LeadsToEntity.Value).GridUid != null)
                {
                    _consoleHost.ExecuteCommand($"fixgridatmos {Transform(gateComp.LeadsToEntity.Value).GridUid!.Value.Id}");
                    gateComp.FirstEnter = true;
                }

                _xformSystem.SetCoordinates(args.Target.Value, Transform(gateComp.LeadsToEntity.Value).Coordinates);
                _audioSystem.PlayPvs(gateComp.GateEnterSound, uid);
                _audioSystem.PlayPvs(gateComp.GateEnterSound, gateComp.LeadsToEntity.Value);
            }
        }
    }
}
