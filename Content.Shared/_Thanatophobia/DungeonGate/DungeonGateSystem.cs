using Content.Shared.DoAfter;
using Content.Shared.DragDrop;
using Robust.Shared.Serialization;

namespace Content.Shared.Thanatophobia.DungeonGate;

public abstract class SharedDungeonGateSystem : EntitySystem
{
    protected void OnCanDragDropOn(EntityUid uid, DungeonGateComponent comp, ref CanDropTargetEvent args)
    {
        if (args.Handled)
            return;

        args.CanDrop = comp.GateState == DungeonGateState.InProgress;
        args.Handled = comp.GateState == DungeonGateState.InProgress;
    }
}

[Serializable, NetSerializable]
public sealed partial class DungeonGateDragDropDoAfterEvent : SimpleDoAfterEvent
{
}
