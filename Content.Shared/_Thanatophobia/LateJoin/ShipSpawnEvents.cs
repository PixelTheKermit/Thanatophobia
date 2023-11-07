using Robust.Shared.Serialization;

namespace Content.Shared.Thanatophobia.LateJoin;
[Serializable, NetSerializable]
public sealed class RoundStartTrySpawnShipUIMessage : EntityEventArgs
{
    public string ShipToSpawn;

    public RoundStartTrySpawnShipUIMessage(string shipToSpawn)
    {
        ShipToSpawn = shipToSpawn;
    }
}
