using Content.Shared.Shuttles.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Shuttles.Components;

/// <summary>
/// Handles what a grid should look like on radar.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedShuttleSystem))]
public sealed partial class IFFComponent : Component
{
    /// <summary>
    /// Should we show IFF by default?
    /// </summary>
    public const bool ShowIFFDefault = true;

    /// <summary>
    /// The range required for the label to show up on displays. This is for any grid that don't have IFFRange by default.
    /// </summary>
    public const float DefaultRange = 1024f;

    /// <summary>
    /// Default color to use for IFF if no component is found.
    /// </summary>
    public static readonly Color IFFColor = Color.Aquamarine;

    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public IFFFlags Flags = IFFFlags.None;

    /// <summary>
    /// The range required for the label to show up on displays.
    /// </summary>

    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public float Range = DefaultRange;

    /// <summary>
    /// Color for this to show up on IFF.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public Color Color = IFFColor;
}

[Flags]
public enum IFFFlags : byte
{
    None = 0,

    /// <summary>
    /// Should the label for this grid be hidden at all ranges.
    /// </summary>
    HideLabel,

    /// <summary>
    /// Should the grid hide entirely (AKA full stealth).
    /// Will also hide the label if that is not set.
    /// </summary>
    Hide,

    // TODO: Need one that hides its outline, just replace it with a bunch of triangles or lines or something.
}
