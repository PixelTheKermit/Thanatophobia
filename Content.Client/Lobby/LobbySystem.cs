using System.Linq;
using Content.Client.LateJoin;
using Content.Client.Thanatophobia.LateJoin;
using Content.Shared.LateJoin;

namespace Content.Client.Lobby;
public sealed partial class LobbySystem : EntitySystem
{
    [Dependency] private readonly ShipLateJoinSystem _shipLateJoin = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<GiveLateJoinTypeUIMessage>(GiveLateJoinMessage);
    }

    private void GiveLateJoinMessage(GiveLateJoinTypeUIMessage msg)
    {
        if (msg.LateType == LateJoinType.SpawnShips && msg.Data.Any(x => x.Key == "ShipPool") && msg.Data["ShipPool"] is string)
            _shipLateJoin.ToggleUI((string) msg.Data["ShipPool"]);
        else
            new LateJoinGui().OpenCentered();
    }
}
