using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.Thanatophobia.ShipGuns;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShipGunConsoleComponent : Component
{
    [DataField("maxRange")]
    public float MaxRange = 256f;
    public int CurGroup = 0;

    [DataField("gunPorts", required: true, customTypeSerializer: typeof(PrototypeIdListSerializer<SourcePortPrototype>))]
    public List<string> GunPorts = new();
}
