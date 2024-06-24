using Content.Shared.Procedural;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Thanatophobia.DungeonGate;

[RegisterComponent, AutoGenerateComponentState(true), NetworkedComponent]
public sealed partial class DungeonGateComponent : Component
{

    /// <summary>
    /// The current state of the gate. See the enum for more details.
    /// </summary>
    [DataField, AutoNetworkedField]
    public DungeonGateState GateState = DungeonGateState.Ready;

    /// <summary>
    /// The config that the dungeon generated will use.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<DungeonConfigPrototype> DungeonConfig = default!;

    /// <summary>
    /// What prototype will the exit gate be?
    /// </summary>
    [DataField(required: true)]
    public ProtoId<EntityPrototype> ExitGateProto = default!;

    /// <summary>
    /// What entity does this gate lead to? This is usually another gate. DO NOT SET THIS!
    /// </summary>
    [DataField]
    public EntityUid? LeadsToEntity = null;

    /// <summary>
    /// How much time do you have till the gate closes?
    /// </summary>
    [DataField]
    public TimeSpan TimeToEnter = TimeSpan.FromMinutes(10);

    /// <summary>
    /// How much more time should be added to the exit gate?
    /// </summary>
    [DataField]
    public TimeSpan TimeAddedToLeave = TimeSpan.FromMinutes(5);

    /// <summary>
    /// When will the gate stop working?
    /// </summary>
    /// <returns></returns>
    [DataField]
    public TimeSpan DeathTime = TimeSpan.Zero;

    // Popup strings

    [DataField]
    public string GateOpenPopup = "tp-dungeon-gate-open";

    [DataField]
    public string GateBrokenPopup = "tp-dungeon-gate-broken";

    // Sprite layers

    [DataField]
    public string? GateReadySprite;

    [DataField]
    public string? GateOngoingSprite;

    [DataField]
    public string? GateBrokenSprite;

    // Sound effects
    [DataField]
    public SoundPathSpecifier GateOpenSound = new("/Audio/Effects/squeak2.ogg");

    [DataField]
    public SoundPathSpecifier GateEnterSound = new("/Audio/Effects/waterswirl.ogg");

    [DataField]
    public SoundPathSpecifier GateBreakSound = new("/Audio/Effects/glass_break4.ogg");
}

/// <summary>
/// The current state of the gate, ready means it's unused, in progress means it has generated a dungeon successfully and is now
/// </summary>
public enum DungeonGateState : byte
{
    Ready,
    InProgress,
    Broken,
}
