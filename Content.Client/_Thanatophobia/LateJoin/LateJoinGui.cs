using System.Linq;
using System.Numerics;
using Content.Client.Message;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Shared.Thanatophobia.CCVar;
using Content.Shared.Thanatophobia.LateJoin;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Client.Thanatophobia.LateJoin;

public sealed class TPLateJoinGui : FancyWindow
{
    private readonly IEntityNetworkManager _entityNetworkManager;
    private readonly IPrototypeManager _protoManager;
    private readonly IConfigurationManager _cfgManager;
    private readonly IEntityManager _entityManager;
    public string SelectedShip = "";
    public bool LobbyHidden = false;

    public Control LobbySelectMenu;
    public Control LobbyPatienceMenu;
    public LineEdit CodeInputBar;
    public Button EnterButton;
    public Button RefreshButton;
    public BoxContainer LobbyContainer;

    public Label PlayerCountLabel;
    public Label RoomCodeLabel;
    public BoxContainer ShipList;
    public BoxContainer PlayerList;
    public RichTextLabel ShipDescription;
    public Button StartLobbyButton;
    public Button PrivateLobbyButton;

    public TPLateJoinGui()
    {
        MinSize = new Vector2(500, 600);
        MaxSize = new Vector2(500, 600);
        Resizable = false;
        _entityNetworkManager = IoCManager.Resolve<IEntityNetworkManager>();
        _entityManager = IoCManager.Resolve<IEntityManager>();
        _protoManager = IoCManager.Resolve<IPrototypeManager>();
        _cfgManager = IoCManager.Resolve<IConfigurationManager>();

        Title = Loc.GetString("tp-ship-lobby-window");

        #region Lobby Select

        RefreshButton = new()
        {
            Margin = new Thickness(0, 2, 5, 2),
            Text = Loc.GetString("tp-refresh-lobby"),
        };
        RefreshButton.StyleClasses.Add(StyleNano.ButtonOpenLeft);
        RefreshButton.OnPressed += _ => _entityNetworkManager.SendSystemNetworkMessage(new RoundStartGetShipLobbiesUIMessage());

        CodeInputBar = new()
        {
            IgnoreNext = true,
            PlaceHolder = Loc.GetString("tp-get-room-code"),
            HorizontalExpand = true,
            Margin = new Thickness(5, 2, 0, 2),
        };

        EnterButton = new()
        {
            Text = Loc.GetString("tp-join-lobby"),
            Margin = new Thickness(0, 2, 0, 2),
        };
        EnterButton.StyleClasses.Add(StyleNano.ButtonOpenBoth);
        EnterButton.OnPressed += _ =>
        {
            _entityNetworkManager.SendSystemNetworkMessage(new RoundStartShipTryJoinLobbyUIMessage(CodeInputBar.Text));
            _entityNetworkManager.SendSystemNetworkMessage(new RoundStartGetShipLobbiesUIMessage());
        };

        var lobbySelectBox = new BoxContainer()
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            XamlChildren =
            {
                new BoxContainer()
                {
                    Orientation = BoxContainer.LayoutOrientation.Horizontal,
                    XamlChildren = { CodeInputBar, EnterButton, RefreshButton }
                }
            }
        };

        LobbyContainer = new()
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical
        };

        var createLobbyButton = new Button()
        {
            Text = Loc.GetString("tp-create-lobby"),
            Margin = new Thickness(5, 2, 5, 2),
        };
        createLobbyButton.OnPressed += _ =>
        {
            _entityNetworkManager.SendSystemNetworkMessage(new RoundStartShipTryCreateLobbyUIMessage());
            _entityNetworkManager.SendSystemNetworkMessage(new RoundStartGetShipLobbiesUIMessage());
        };

        var dimLobbyBackground = new PanelContainer()
        {
            PanelOverride = new StyleBoxFlat() { BackgroundColor = Color.Black.WithAlpha(0.25f) },
            VerticalExpand = true,
            Margin = new Thickness(5, 0, 5, 0),
            XamlChildren = { new ScrollContainer()
            {
                XamlChildren = { LobbyContainer },
            }}
        };

        LobbySelectMenu = new()
        {
            Visible = false,
            XamlChildren =
            {
                new BoxContainer()
                {
                    Orientation = BoxContainer.LayoutOrientation.Vertical,
                    XamlChildren =
                    {
                        lobbySelectBox,
                        dimLobbyBackground,
                        createLobbyButton
                    }
                }
            }
        };

        #endregion

        #region Lobby

        PlayerCountLabel = new()
        {
            Text = Loc.GetString("tp-lobby-player-count", ("current", "?"), ("max", "?")),
            HorizontalExpand = true,
        };

        RoomCodeLabel = new()
        {
            Text = Loc.GetString("tp-current-lobby-code", ("code", "?")),
            HorizontalAlignment = HAlignment.Right,
        };

        PrivateLobbyButton = new()
        {
            Text = Loc.GetString("tp-lobby-private-button")
        };
        PrivateLobbyButton.OnPressed += _ => _entityNetworkManager.SendSystemNetworkMessage(new RoundStartShipTryChangeLobbyInfoUIMessage(!LobbyHidden, SelectedShip));

        PlayerList = new()
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
        };

        ShipList = new()
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
        };

        ShipDescription = new()
        {
            VerticalAlignment = VAlignment.Stretch,
        };

        StartLobbyButton = new()
        {
            HorizontalExpand = true,
            Text = Loc.GetString("tp-start-lobby")
        };
        StartLobbyButton.OnPressed += _ => _entityNetworkManager.SendSystemNetworkMessage(new RoundStartShipTryStartLobbyUIMessage());

        var leaveLobbyButton = new Button()
        {
            HorizontalExpand = true,
            Text = Loc.GetString("tp-leave-lobby")
        };
        leaveLobbyButton.OnPressed += _ => _entityNetworkManager.SendSystemNetworkMessage(new RoundStartShipTryLeaveLobbyUIMessage());

        var lobbySmallInfoContainer = new BoxContainer()
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 5),
            HorizontalExpand = true,
            XamlChildren =
            {
                PlayerCountLabel,
                PrivateLobbyButton,
                RoomCodeLabel
            }
        };

        var lobbyButtons = new BoxContainer()
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            XamlChildren =
            {
                leaveLobbyButton,
                StartLobbyButton
            }
        };

        var dimPlayerListBackground = new PanelContainer()
        {
            PanelOverride = new StyleBoxFlat() { BackgroundColor = Color.Black.WithAlpha(0.25f) },
            VerticalExpand = true,
            Margin = new Thickness(0, 0, 0, 5),
            XamlChildren = { new ScrollContainer()
            {
                XamlChildren = { PlayerList },
            }}
        };

        var shuttleConfig = new BoxContainer()
        {
            Margin = new Thickness(0, 0, 0, 5),
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            VerticalExpand = true,
            XamlChildren =
            {
                new PanelContainer()
                {
                    PanelOverride = new StyleBoxFlat() { BackgroundColor = Color.Black.WithAlpha(0.25f) },
                    Margin = new Thickness(0, 0, 5, 0),
                    VerticalExpand = true,
                    HorizontalExpand = true,
                    XamlChildren = { new ScrollContainer()
                    {
                        HScrollEnabled = false,
                        XamlChildren = { ShipList },
                    }}
                },
                new PanelContainer()
                {
                    PanelOverride = new StyleBoxFlat() { BackgroundColor = Color.Black.WithAlpha(0.25f) },
                    VerticalExpand = true,
                    HorizontalExpand = true,
                    XamlChildren = { new ScrollContainer()
                    {
                        HScrollEnabled = false,
                        XamlChildren = { ShipDescription },
                    }}
                }
            }
        };

        LobbyPatienceMenu = new()
        {
            Visible = false,
            XamlChildren =
            {
                new BoxContainer()
                {
                    Margin = new Thickness(5, 5, 5, 5),
                    Orientation = BoxContainer.LayoutOrientation.Vertical,
                    XamlChildren =
                    {
                        lobbySmallInfoContainer,
                        dimPlayerListBackground,
                        shuttleConfig,
                        lobbyButtons
                    }
                }
            }
        };

        #endregion

        XamlChildren.Add(LobbySelectMenu);
        XamlChildren.Add(LobbyPatienceMenu);
        _entityNetworkManager.SendSystemNetworkMessage(new RoundStartGetShipLobbiesUIMessage());
    }
    public void UpdateLobbyList(RoundStartShipListLobbiesUIMessage lobbyEv)
    {
        LobbyContainer.XamlChildren.Clear();
        PlayerList.XamlChildren.Clear();
        ShipList.XamlChildren.Clear();

        if (lobbyEv.IsPlayerInLobby)
        {
            LobbySelectMenu.Visible = false;
            LobbyPatienceMenu.Visible = true;

            StartLobbyButton.Visible = lobbyEv.IsLobbyOwner;

            SelectedShip = lobbyEv.CurrentShip;
            LobbyHidden = lobbyEv.IsHidden;

            if (LobbyHidden)
                PrivateLobbyButton.Text = Loc.GetString("tp-lobby-private-button");
            else
                PrivateLobbyButton.Text = Loc.GetString("tp-lobby-public-button");

            PlayerCountLabel.Text = Loc.GetString("tp-lobby-player-count", ("current", lobbyEv.LobbyPlayerCount), ("max", lobbyEv.LobbyPlayerMax));
            RoomCodeLabel.Text = Loc.GetString("tp-current-lobby-code", ("code", lobbyEv.LobbyCode));
            ShipDescription.SetMarkup(Loc.GetString($"tp-ui-ship-desc-{SelectedShip}"));

            // Shows all the players in the lobby.
            for (var i = 0; i < lobbyEv.PlayerNames.Count; i++)
            {
                var plrContainer = new BoxContainer()
                {
                    HorizontalExpand = true,
                    VerticalAlignment = VAlignment.Top,
                    Margin = new Thickness(5),
                    XamlChildren =
                    {
                        new Label()
                        {
                            Text = lobbyEv.PlayerNames[i],
                            HorizontalExpand = true
                        }
                    }
                };

                if (i == 0)
                {
                    plrContainer.XamlChildren.Add(new Label()
                    {
                        Text = Loc.GetString("tp-lobby-owner-title"),
                    });
                }
                else if (lobbyEv.IsLobbyOwner)
                {
                    var kickButton = new Button()
                    {
                        Text = Loc.GetString("tp-lobby-kick-player")
                    };
                    var plrIndex = i;
                    kickButton.OnPressed += _ => _entityNetworkManager.SendSystemNetworkMessage(new RoundStartShipTryKickPlayerUIMessage(plrIndex));
                    plrContainer.XamlChildren.Add(kickButton);
                }
                PlayerList.XamlChildren.Add(plrContainer);
            }

            var poolStr = _cfgManager.GetCVar(TPCCVars.ShipSpawnPool);

            if (!_protoManager.TryIndex<ShipSpawnPoolPrototype>(poolStr, out var poolProto))
                return;

            foreach (var mapId in poolProto.Ships)
            {
                var shipButton = new Button()
                {
                    Text = Loc.GetString($"tp-ui-ship-name-{mapId.Key}")
                };
                shipButton.OnPressed += _ => _entityNetworkManager.SendSystemNetworkMessage(new RoundStartShipTryChangeLobbyInfoUIMessage(LobbyHidden, mapId.Key));

                ShipList.XamlChildren.Add(shipButton);
            }
        }
        else
        {
            LobbySelectMenu.Visible = true;
            LobbyPatienceMenu.Visible = false;

            var lobbies = lobbyEv.Lobbies.OrderBy(x => x.Players);

            foreach (var lobby in lobbies)
                LobbyContainer.XamlChildren.Add(new TPLobbyPanel(lobby.OwnerName, lobby.Code, lobby.ShuttleName, lobby.Players, lobby.Max, _entityNetworkManager));
        }
    }

    private sealed partial class TPLobbyPanel : PanelContainer
    {
        /// <summary>
        /// Used for sorting.
        /// </summary>
        public int SlotsLeft;
        /// <summary>
        /// Used for joining rooms.
        /// </summary>
        public string LobbyCode;
        public TPLobbyPanel(
            string lobbyOwner,
            string lobbyCode,
            string currentShip,
            int playerCount,
            int maxCount,
            IEntityNetworkManager entityNetMan)
        {
            SlotsLeft = maxCount - playerCount;
            LobbyCode = lobbyCode;

            Margin = new Thickness(5);
            StyleClasses.Add(StyleNano.StyleClassBorderedWindowPanel);

            var joinButton = new Button()
            {
                Text = Loc.GetString("tp-join-lobby"),
                HorizontalAlignment = HAlignment.Right
            };
            joinButton.OnPressed += _ =>
            {
                entityNetMan.SendSystemNetworkMessage(new RoundStartShipTryJoinLobbyUIMessage(lobbyCode));
                entityNetMan.SendSystemNetworkMessage(new RoundStartGetShipLobbiesUIMessage());
            };

            var vContainer = new BoxContainer()
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical,
                XamlChildren =
                {
                    new Label()
                    {
                        Text = Loc.GetString("tp-lobby-owner-name", ("owner", lobbyOwner)),
                        Margin = new Thickness(2)
                    },
                    new Label()
                    {
                        Text = Loc.GetString("tp-lobby-ship-name", ("ship", currentShip)),
                        Margin = new Thickness(2)
                    },
                    new BoxContainer()
                    {
                        Orientation = BoxContainer.LayoutOrientation.Horizontal,
                        XamlChildren =
                        {
                            new Label()
                            {
                                Text = Loc.GetString("tp-lobby-player-count", ("current", playerCount), ("max", maxCount)),
                                Margin = new Thickness(2),
                                HorizontalExpand = true,
                            },
                            joinButton
                        }
                    }
                }
            };
            XamlChildren.Add(vContainer);
        }
    }

    // _entityNetworkManager.SendSystemNetworkMessage(new RoundStartTrySpawnShipUIMessage(SelectedShip));
}
