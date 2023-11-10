using System.Linq.Expressions;
using Content.Shared.DeviceLinking;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

namespace Content.Shared.Thanatophobia.ShipGuns.Ammo;

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class LinkedAmmoProviderComponent : Component
{
    [DataField("port", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<SourcePortPrototype>))]
    public string Port = string.Empty;

    [DataField("whitelist", required: true)]
    public EntityWhitelist Whitelist = new();

    [AutoNetworkedField]
    public List<NetEntity> Outputs = new();
}
