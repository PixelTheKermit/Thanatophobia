using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.StatusEffect;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ReduceEffectTimeOnInteractComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan TimeToDecrease = TimeSpan.FromSeconds(5f);

    [DataField]
    public SoundSpecifier? InteractSound = new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg");

    [DataField]
    public string? InteractString = null;

    [DataField]
    public bool InteractWithSelf = false;
}
