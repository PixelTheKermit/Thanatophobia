using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared.Thanatophobia.CCVar;

[CVarDefs]
public sealed partial class TPCCVars : CVars
{
    public static readonly CVarDef<string> ShipSpawnPool = CVarDef.Create("game.tp_ship_spawn_pool", "Default", CVar.SERVER | CVar.REPLICATED);
}
