using System;
using System.Collections.Generic;
using TootTally.Graphics.Animation;
using TootTally.Multiplayer.MultiplayerPanels;
using TootTally.Utils.APIServices;
using TootTally.Utils.Helpers;
using UnityEngine;

namespace TootTally.Multiplayer
{
    public class MultiplayerController
    {
        public PlaytestAnims GetInstance => CurrentInstance;
        public static PlaytestAnims CurrentInstance { get; private set; }

        private static List<SerializableClass.MultiplayerLobbyInfo> _lobbyInfoList;

        private MultiplayerPanelBase _currentActivePanel, _lastPanel;
        private Vector2 _lastPosition;
        private bool _isTransitioning;

        private MultiplayerMainPanel _multMainPanel;
        private MultiplayerLobbyPanel _multLobbyPanel;
        private MultiplayerCreatePanel _multCreatePanel;

        private int _currentLobbyIndex;

        #region LocalTesting
        private static readonly SerializableClass.MultiplayerUserInfo _gristUser = new SerializableClass.MultiplayerUserInfo()
        {
            id = 0,
            country = "USA",
            rank = -1,
            username = "gristCollector",
            state = "Ready"
        };

        public static readonly SerializableClass.MultiplayerUserInfo _electroUser = new SerializableClass.MultiplayerUserInfo() //Temporary public for testing
        {
            id = 1,
            country = "CAD",
            rank = 2,
            username = "Electrostats",
            state = "Not Ready"
        };

        private static readonly SerializableClass.MultiplayerUserInfo _gloomhonkUser = new SerializableClass.MultiplayerUserInfo()
        {
            id = 2,
            country = "AUS",
            rank = 20,
            username = "GloomHonk",
            state = "Memeing"
        };
        private static readonly SerializableClass.MultiplayerUserInfo _lumpytfUser = new SerializableClass.MultiplayerUserInfo()
        {
            id = 3,
            country = "MOM",
            rank = 250000,
            username = "Lumpytf",
            state = "AFK"
        };
        private static readonly SerializableClass.MultiplayerUserInfo _jampotUser = new SerializableClass.MultiplayerUserInfo()
        {
            id = 4,
            country = "DAD",
            rank = 1,
            username = "Jampot",
            state = "Host (Song Select)"
        };
        private static readonly SerializableClass.MultiplayerUserInfo _betaUser = new SerializableClass.MultiplayerUserInfo()
        {
            id = 5,
            country = "SANS",
            rank = 69,
            username = "SierraBeta",
            state = "Sansing"
        };
        private static readonly SerializableClass.MultiplayerUserInfo _runUser = new SerializableClass.MultiplayerUserInfo()
        {
            id = 6,
            country = "TF2",
            rank = 420,
            username = "RunDomRun",
            state = "Singing"
        };
        #endregion

        public MultiplayerController(PlaytestAnims __instance)
        {
            CurrentInstance = __instance;
            CurrentInstance.factpanel.gameObject.SetActive(false);

            GameObject canvasWindow = GameObject.Find("Canvas-Window").gameObject;
            Transform panelTransform = canvasWindow.transform.Find("Panel");

            var canvas = GameObject.Instantiate(MultiplayerAssetManager.GetPrefab("multiplayercanvas"));

            try
            {
                _multMainPanel = new MultiplayerMainPanel(canvas, this);
            }
            catch (Exception)
            {
                Plugin.Instance.LogError("Couldn't init main panel");
            }
            try
            {
                _multLobbyPanel = new MultiplayerLobbyPanel(canvas, this);
            }
            catch (Exception)
            {
                Plugin.Instance.LogError("Couldn't init lobby panel");
            }
            try
            {
                _multCreatePanel = new MultiplayerCreatePanel(canvas, this);
            }
            catch (Exception)
            {
                Plugin.Instance.LogError("Couldn't init create panel");
            }

            if (_lobbyInfoList == null)
            {
                _lobbyInfoList = new List<SerializableClass.MultiplayerLobbyInfo>();
                _currentLobbyIndex = 0;
                AddLocalLobbyData();
            }

            _currentActivePanel = _multMainPanel;

            AnimationManager.AddNewScaleAnimation(_multMainPanel.panel, Vector3.one, 1f, GetSecondDegreeAnimation(1.5f), (sender) => UpdateLobbyInfo(true));
        }

        public void AddLocalLobbyData()
        {
            CreateNewLobby(new SerializableClass.MultiplayerLobbyInfo()
            {
                name = "TestMulti1",
                title = "gristCollector's Lobby",
                password = "",
                maxPlayerCount = 16,
                currentState = "Playing: Never gonna give you up",
                ping = 69f,
                users = new List<SerializableClass.MultiplayerUserInfo> { _gristUser }
            },
            new SerializableClass.MultiplayerLobbyInfo()
            {
                name = "TestMulti2",
                title = "Electrostats's Lobby",
                password = "RocketLeague",
                maxPlayerCount = 32,
                currentState = "Playing: Taps",
                ping = 1f,
                users = new List<SerializableClass.MultiplayerUserInfo> { _electroUser, _jampotUser }
            },
            new SerializableClass.MultiplayerLobbyInfo()
            {
                name = "TestMulti3",
                title = "Lumpytf's private room",
                password = "",
                maxPlayerCount = 1,
                currentState = "Selecting Song",
                ping = 12f,
                users = new List<SerializableClass.MultiplayerUserInfo> { _lumpytfUser }
            },
            new SerializableClass.MultiplayerLobbyInfo()
            {
                name = "TestMulti4",
                title = "GloomHonk's Meme songs",
                password = "420blazeit",
                maxPlayerCount = 99,
                currentState = "Playing: tt is love tt is life",
                ping = 224f,
                users = new List<SerializableClass.MultiplayerUserInfo> { _gloomhonkUser }
            },
            new SerializableClass.MultiplayerLobbyInfo()
            {
                name = "TestMulti5",
                title = "SierraBeta's Undertale Songs",
                password = "dododado",
                maxPlayerCount = 69,
                currentState = "Playing: Megalovania",
                ping = -5f,
                users = new List<SerializableClass.MultiplayerUserInfo> { _betaUser, _electroUser, _jampotUser, _lumpytfUser, _gloomhonkUser, _gristUser }
            },
            new SerializableClass.MultiplayerLobbyInfo()
            {
                name = "TestMulti6",
                title = "RunDom's trolleries",
                password = "HappyBirthdayElectro",
                maxPlayerCount = 2,
                currentState = "Playing: Happy Birthday",
                ping = 2.5f,
                users = new List<SerializableClass.MultiplayerUserInfo> { _runUser, _electroUser }
            });
        }

        public void OnSliderValueChangeScrollContainer(GameObject container, float value)
        {
            var gridPanelRect = container.GetComponent<RectTransform>();
            gridPanelRect.anchoredPosition = new Vector2(gridPanelRect.anchoredPosition.x, Mathf.Min((value * (_lobbyInfoList.Count - 7f) * 105f) - 440f, ((_lobbyInfoList.Count - 8f) * 105f) + 74f - 440f)); //This is so scuffed I fucking love it
        }

        private IEnumerator<WaitForSeconds> DelayDisplayLobbyInfo(float delay, SerializableClass.MultiplayerLobbyInfo lobby, Action<SerializableClass.MultiplayerLobbyInfo> callback)
        {
            yield return new WaitForSeconds(delay);
            callback(lobby);
        }

        public void UpdateLobbyInfo(bool delay)
        {
            for (int i = 0; i < _lobbyInfoList.Count; i++)
            {
                if (delay)
                    Plugin.Instance.StartCoroutine(DelayDisplayLobbyInfo(i * .1f, _lobbyInfoList[i], _multMainPanel.DisplayLobby));
                else
                    _multMainPanel.DisplayLobby(_lobbyInfoList[i]);
            }
            _multMainPanel.UpdateScrolling(_lobbyInfoList.Count);
        }

        public void ConnectToLobby(SerializableClass.MultiplayerLobbyInfo lobby)
        {
            MultiplayerManager.UpdateMultiplayerState(MultiplayerState.Lobby);
            MoveToLobby();
            _multLobbyPanel.DIsplayAllUserInfo(lobby.users);
        }


        public void MoveToCreate()
        {
            TransitionToPanel(_multCreatePanel);
        }

        public void MoveToLobby()
        {
            TransitionToPanel(_multLobbyPanel);
        }

        public void MoveToMain()
        {
            TransitionToPanel(_multMainPanel);
        }

        public void ReturnToLastPanel()
        {
            TransitionToPanel(_lastPanel);
        }

        public void RefreshAllLobbyInfo()
        {
            _multMainPanel.ClearAllLobby();
            UpdateLobbyInfo(true);
        }

        public void CreateNewLobby(params SerializableClass.MultiplayerLobbyInfo[] lobbyInfo)
        {
            if (lobbyInfo == null || lobbyInfo.Length == 0) return;

            for (int i = 0; i < lobbyInfo.Length; i++)
            {
                lobbyInfo[i].id = _currentLobbyIndex++;
            }
            _lobbyInfoList.AddRange(lobbyInfo);
        }

        public void TransitionToPanel(MultiplayerPanelBase nextPanel)
        {
            if (_currentActivePanel == nextPanel || _isTransitioning) return;

            _isTransitioning = true;
            _lastPanel = _currentActivePanel;
            var positionOut = -nextPanel.GetPanelPosition;
            AnimationManager.AddNewPositionAnimation(_currentActivePanel.panel, positionOut, 0.9f, new EasingHelper.SecondOrderDynamics(1.5f, 0.89f, 1.1f), delegate { _lastPanel.panel.SetActive(false); _lastPanel.panel.GetComponent<RectTransform>().anchoredPosition = positionOut; });
            nextPanel.panel.SetActive(true);
            _currentActivePanel = nextPanel;
            AnimationManager.AddNewPositionAnimation(nextPanel.panel, Vector2.zero, 0.9f, new EasingHelper.SecondOrderDynamics(1.5f, 0.89f, 1.1f), (sender) => _isTransitioning = false);
        }

        public void OnDeclineButtonClick()
        {
            MultiplayerManager.UpdateMultiplayerState(MultiplayerState.ExitScene);
        }

        public void TransitionToSongSelection()
        {
            MultiplayerManager.UpdateMultiplayerState(MultiplayerState.SelectSong);
        }

        public static EasingHelper.SecondOrderDynamics GetSecondDegreeAnimation(float speedMult = 1f) => new EasingHelper.SecondOrderDynamics(speedMult, 0.75f, 1.15f);

        public enum MultiplayerState
        {
            None,
            Enter,
            FirstTimePopUp,
            LoadPanels,
            Home,
            CreatingLobby,
            Lobby,
            Hosting,
            SelectSong,
            ExitScene,
        }

    }
}
