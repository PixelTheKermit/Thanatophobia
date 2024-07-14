using Content.Shared.DragDrop;
using Content.Shared.Thanatophobia.DungeonGate;
using Robust.Client.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Client.Thanatophobia.DungeonGate;

public sealed partial class DungeonGateSystem : SharedDungeonGateSystem
{
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DungeonGateComponent, AfterAutoHandleStateEvent>(DungeonGateStateHandling);
        SubscribeLocalEvent<DungeonGateComponent, ComponentStartup>(DungeonGateStateHandling);

        SubscribeLocalEvent<DungeonGateComponent, CanDropTargetEvent>(OnCanDragDropOn);
    }

    private void DungeonGateStateHandling<TArgs>(EntityUid uid, DungeonGateComponent comp, ref TArgs args)
    {
        if (!TryComp<SpriteComponent>(uid, out var spriteComp))
            return;

        if (comp.GateReadySprite != null && spriteComp.LayerMapTryGet(comp.GateReadySprite, out var readyLayer))
            spriteComp[readyLayer].Visible = comp.GateState == DungeonGateState.Ready;

        if (comp.GateOngoingSprite != null && spriteComp.LayerMapTryGet(comp.GateOngoingSprite, out var ongoingLayer))
            spriteComp[ongoingLayer].Visible = comp.GateState == DungeonGateState.InProgress;

        if (comp.GateBrokenSprite != null && spriteComp.LayerMapTryGet(comp.GateBrokenSprite, out var brokenLayer))
            spriteComp[brokenLayer].Visible = comp.GateState == DungeonGateState.Broken;
    }
}
