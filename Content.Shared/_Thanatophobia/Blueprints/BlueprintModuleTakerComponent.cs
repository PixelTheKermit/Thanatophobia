using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Shared.Thanatophobia.Blueprints;

[RegisterComponent]
[NetworkedComponent]
public sealed partial class BlueprintModuleTakerComponent : Component
{
    [DataField]
    public string ContainerID = "Blueprints";

    [DataField]
    public Container? BlueprintContainer = null;

    [DataField]
    public SoundSpecifier? InsertSound = null;
}
