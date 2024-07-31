using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared.Thanatophobia.CCVar;

[CVarDefs]
public sealed partial class TPCCVars : CVars
{
    public static readonly CVarDef<string> ShipSpawnPool = CVarDef.Create("game.tp_ship_spawn_pool", "Default", CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<int> SecondsToRespawn = CVarDef.Create("game.tp_seconds_to_respawn", 90, CVar.SERVER | CVar.REPLICATED);
}
