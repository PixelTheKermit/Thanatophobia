using System.Numerics;
using Content.Shared.Shuttles.BUIStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Thanatophobia.ShipGuns;

[Serializable, NetSerializable]
[Virtual]
public sealed class ShipGunConsoleBoundUIState : BoundUserInterfaceState
{
    public float MaxRange;
    public NetCoordinates? Coordinates;
    public Angle? Angle;
    public int CurGroup;
    public List<ShipGunState> GunsInfo;
    public int PortCount;
    public ShipGunConsoleBoundUIState(
        float maxRange,
        NetCoordinates? netCoordinates,
        Angle? angle,
        int curGroup,
        List<ShipGunState> gunsInfo,
        int portCount)
    {
        MaxRange = maxRange;
        Coordinates = netCoordinates;
        Angle = angle;
        CurGroup = curGroup;
        GunsInfo = gunsInfo;
        PortCount = portCount;
    }
}

[Serializable, NetSerializable]
public sealed class ShipGunState
{
    public NetEntity Uid;
}

[Serializable, NetSerializable]
public sealed class ShipGunConsolePosMessage : BoundUserInterfaceMessage
{
    public NetCoordinates MousePos;

    public ShipGunConsolePosMessage(NetCoordinates mousePos)
    {
        MousePos = mousePos;
    }
}

[Serializable, NetSerializable]
public sealed class ShipGunConsoleShootMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class ShipGunConsoleUnshootMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class ShipGunConsoleSetGroupMessage : BoundUserInterfaceMessage
{
    public int Group;

    public ShipGunConsoleSetGroupMessage(int group)
    {
        Group = group;
    }
}
