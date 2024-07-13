using Content.Shared.Thanatophobia.LateJoin;

namespace Content.Client.Thanatophobia.LateJoin;
public sealed partial class ShipLateJoinSystem : EntitySystem
{
    private TPLateJoinGui? _lateJoinUI = null;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<RoundStartShipListLobbiesUIMessage>(UpdateLobbyUI);
    }

    private void UpdateLobbyUI(RoundStartShipListLobbiesUIMessage msg)
    {
        if (_lateJoinUI == null)
            return;

        _lateJoinUI.UpdateLobbyList(msg);
    }

    public void ToggleUI()
    {
        if (_lateJoinUI == null)
        {
            _lateJoinUI = new();
            _lateJoinUI.OpenCentered();
            _lateJoinUI.OnClose += () => CloseUI();
        }
        else
        {
            _lateJoinUI.Close();
            _lateJoinUI = null;
        }
    }

    public void CloseUI()
    {
        if (_lateJoinUI != null)
        {
            _lateJoinUI.Close();
            _lateJoinUI = null;
        }
    }
}
