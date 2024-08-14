using Robust.Shared.Map;

[RegisterComponent]
public sealed partial class WorldShipSpawnerComponent : Component
{
    [DataField]
    public bool NeedsSetup = true;

    [DataField]
    public int SpawnArea = 16;

    [DataField]
    public List<MapCoordinates> FreeCoordinates = new();
}
