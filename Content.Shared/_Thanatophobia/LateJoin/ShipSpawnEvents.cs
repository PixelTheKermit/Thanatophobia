using Robust.Shared.Serialization;

namespace Content.Shared.Thanatophobia.LateJoin;
[Serializable, NetSerializable]
[Virtual]
public sealed class RoundStartShipTryStartLobbyUIMessage : EntityEventArgs
{
    public RoundStartShipTryStartLobbyUIMessage()
    {
    }
}

[Serializable, NetSerializable]
public sealed class RoundStartShipTryChangeLobbyInfoUIMessage : EntityEventArgs
{
    public bool Private;
    public string Ship;
    public RoundStartShipTryChangeLobbyInfoUIMessage(bool isPrivate = true, string newShip = "")
    {
        Private = isPrivate;
        Ship = newShip;
    }
}

[Serializable, NetSerializable]
public sealed class RoundStartShipTryKickPlayerUIMessage : EntityEventArgs
{
    public int PlayerIndex;
    public RoundStartShipTryKickPlayerUIMessage(int playerIndex)
    {
        PlayerIndex = playerIndex;
    }
}

[Serializable, NetSerializable]
public sealed class RoundStartShipTryGetLobbyInfoUIMessage : EntityEventArgs
{
    public RoundStartShipTryGetLobbyInfoUIMessage()
    {
    }
}

[Serializable, NetSerializable]
public sealed class RoundStartShipTryCreateLobbyUIMessage : EntityEventArgs
{
    public RoundStartShipTryCreateLobbyUIMessage()
    {
    }
}

[Serializable, NetSerializable]
public sealed class RoundStartShipTryJoinLobbyUIMessage : EntityEventArgs
{
    public string LobbyCode;
    public RoundStartShipTryJoinLobbyUIMessage(string lobbyCode)
    {
        LobbyCode = lobbyCode;
    }
}

[Serializable, NetSerializable]
public sealed class RoundStartShipTryLeaveLobbyUIMessage : EntityEventArgs
{
    public RoundStartShipTryLeaveLobbyUIMessage()
    {
    }
}

[Serializable, NetSerializable]
public sealed class RoundStartGetShipLobbiesUIMessage : EntityEventArgs
{
    public RoundStartGetShipLobbiesUIMessage()
    {
    }
}

[Serializable, NetSerializable]
public sealed class RoundStartShipListLobbiesUIMessage : EntityEventArgs
{
    public bool IsPlayerInLobby = false;
    public bool IsLobbyOwner = false;
    public string LobbyCode = "";
    public string CurrentShip = "";
    public int LobbyPlayerCount = 0;
    public int LobbyPlayerMax = 0;
    public bool IsHidden = false;
    public List<string> PlayerNames = new();
    public List<ShipLobbyState> Lobbies = new();
    public RoundStartShipListLobbiesUIMessage()
    {
    }
}

[Serializable, NetSerializable]
public struct ShipLobbyState
{
    public string Code;
    public int Players;
    public int Max;
    public string OwnerName;
    public string ShuttleName;
}
