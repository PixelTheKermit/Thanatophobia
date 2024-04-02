using Content.Shared.Ghost;
using Content.Shared.Thanatophobia.Respawning;
using Robust.Shared.Console;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server.Thanatophobia.Respawning;

public sealed partial class SimpleRespawningSystem : EntitySystem
{
    [Dependency] private readonly IConsoleHost _consoleHost = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<SimpleRespawnUIMessage>(RespawnPlayer);
    }

    private void RespawnPlayer(SimpleRespawnUIMessage msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { })
            return;

        var uid = args.SenderSession.AttachedEntity.Value;

        if (!TryComp<GhostComponent>(uid, out var ghostComp))
            return;

        if (_gameTiming.CurTime - ghostComp.TimeOfDeath <= TimeSpan.FromMinutes(5)) // Hardcoded for now. In the future should be a CVAR.
            return;

        _consoleHost.ExecuteCommand($"respawn \"{args.SenderSession.Name}\"");
    }
}
