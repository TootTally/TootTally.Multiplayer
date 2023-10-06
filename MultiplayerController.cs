using System;
using System.Collections.Generic;
using TMPro;
using TootTally.Graphics;
using TootTally.Graphics.Animation;
using TootTally.Utils;
using TootTally.Utils.APIServices;
using TootTally.Utils.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TootTally.Multiplayer
{
    public class MultiplayerController
    {
        private static PlaytestAnims _currentInstance;

        private static List<SerializableClass.MultiplayerLobbyInfo> _lobbyInfoList;
        private static List<GameObject> _lobbyInfoRowsList;

        private static GameObject _canvas, _mainPanel, _lobbyInfoContainer, _lobbyDetailContainer, _lobbyConnectContainer;
        private static ScrollableSliderHandler _scrollingHandler;

        private static TMP_Text _lobbyPlayerListText;

        private static SerializableClass.MultiplayerLobbyInfo _createLobbyInfo;
        private static CustomButton _connectButton, _createLobbyButton;
        private static CustomAnimation _connectButtonScaleAnimation;

        private static EventTrigger.Entry _pointerExitLobbyContainerEvent;

        private static SerializableClass.MultiplayerLobbyInfo _selectedLobby;
        private static GameObject _selectedLobbyContainer;

        #region LocalTesting
        private static readonly SerializableClass.MultiplayerUserInfo _gristUser = new SerializableClass.MultiplayerUserInfo()
        {
            id = 0,
            country = "USA",
            rank = -1,
            username = "gristCollector",
            state = "Ready"
        };

        private static readonly SerializableClass.MultiplayerUserInfo _electrUser = new SerializableClass.MultiplayerUserInfo()
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
        #endregion

        public MultiplayerController(PlaytestAnims __instance)
        {
            _currentInstance = __instance;
            _currentInstance.factpanel.gameObject.SetActive(false);

            GameObject canvasWindow = GameObject.Find("Canvas-Window").gameObject;
            Transform panelTransform = canvasWindow.transform.Find("Panel");

            _canvas = GameObject.Instantiate(MultiplayerAssetManager.GetPrefab("multiplayercanvas"));
            _mainPanel = _canvas.transform.Find("MainPanelBG").gameObject;
            _mainPanel.transform.localScale = Vector2.zero;

            var topPanel = _mainPanel.transform.Find("MainPanelFG/TopMain/TopMainContainer");
            var titleText = GameObjectFactory.CreateSingleText(topPanel, "TitleText", "TootTally Multiplayer", Color.white);
            titleText.enableAutoSizing = true;
            titleText.alignment = TextAlignmentOptions.Left;
            var serverText = GameObjectFactory.CreateSingleText(topPanel, "ServerText", "Server: Toronto", Color.white);
            serverText.enableAutoSizing = true;
            serverText.alignment = TextAlignmentOptions.Right;

            _lobbyInfoContainer = _mainPanel.transform.Find("MainPanelFG/BottomMain/LeftPanel/LeftPanelContainer").gameObject;
            var slider = new GameObject("ContainerSlider", typeof(Slider)).GetComponent<Slider>();
            slider.gameObject.SetActive(true);
            slider.onValueChanged.AddListener((value) => OnSliderValueChangeScrollContainer(_lobbyInfoContainer, value));
            _scrollingHandler = slider.gameObject.AddComponent<ScrollableSliderHandler>();
            _scrollingHandler.slider = slider;
            _scrollingHandler.enabled = false;

            _lobbyDetailContainer = _mainPanel.transform.Find("MainPanelFG/BottomMain/RightPanel/TopContainer").gameObject;
            _lobbyPlayerListText = GameObjectFactory.CreateSingleText(_lobbyDetailContainer.transform, "LobbyDetailInfoText", "PlaceHolder", Color.white);
            _lobbyPlayerListText.enableAutoSizing = true;
            _lobbyPlayerListText.fontSizeMax = 42;
            _lobbyPlayerListText.alignment = TextAlignmentOptions.TopLeft;

            _pointerExitLobbyContainerEvent = new EventTrigger.Entry();
            _pointerExitLobbyContainerEvent.eventID = EventTriggerType.PointerExit;
            _pointerExitLobbyContainerEvent.callback.AddListener((data) => OnMouseExitClearLobbyDetails());

            _lobbyConnectContainer = _mainPanel.transform.Find("MainPanelFG/BottomMain/RightPanel/BottomContainer").gameObject;
            var connectLayout = _lobbyConnectContainer.GetComponent<VerticalLayoutGroup>();
            connectLayout.childControlHeight = connectLayout.childControlWidth = false;
            connectLayout.childAlignment = TextAnchor.MiddleCenter;

            _connectButton = GameObjectFactory.CreateCustomButton(_lobbyConnectContainer.transform, Vector2.zero, new Vector2(150, 50), "Connect", "LobbyConnectButton");
            _connectButton.gameObject.SetActive(false);

            _lobbyInfoList = new List<SerializableClass.MultiplayerLobbyInfo>();
            _lobbyInfoRowsList = new List<GameObject>();

            GetLobbyInfo();
            AnimationManager.AddNewScaleAnimation(_mainPanel, Vector3.one, 1f, GetSecondDegreeAnimation(1.5f), (sender) => UpdateLobbyInfo(true));
        }

        public void GetLobbyInfo()
        {
            _lobbyInfoList.Add(new SerializableClass.MultiplayerLobbyInfo()
            {
                id = 1,
                name = "TestMulti1",
                title = "gristCollector's Lobby",
                password = "",
                maxPlayerCount = 16,
                currentState = "Playing: Never gonna give you up",
                ping = 69f,
                users = new List<SerializableClass.MultiplayerUserInfo> { _gristUser }
            });
            _lobbyInfoList.Add(new SerializableClass.MultiplayerLobbyInfo()
            {
                id = 2,
                name = "TestMulti2",
                title = "Electrostats's Lobby",
                password = "RocketLeague",
                maxPlayerCount = 32,
                currentState = "Playing: Taps",
                ping = 1f,
                users = new List<SerializableClass.MultiplayerUserInfo> { _electrUser, _jampotUser }
            });
            _lobbyInfoList.Add(new SerializableClass.MultiplayerLobbyInfo()
            {
                id = 3,
                name = "TestMulti3",
                title = "Lumpytf's private room",
                password = "",
                maxPlayerCount = 1,
                currentState = "Selecting Song",
                ping = 12f,
                users = new List<SerializableClass.MultiplayerUserInfo> { _lumpytfUser }
            });
            _lobbyInfoList.Add(new SerializableClass.MultiplayerLobbyInfo()
            {
                id = 4,
                name = "TestMulti4",
                title = "GloomHonk's Meme songs",
                password = "420blazeit",
                maxPlayerCount = 99,
                currentState = "Playing: tt is love tt is life",
                ping = 224f,
                users = new List<SerializableClass.MultiplayerUserInfo> { _gloomhonkUser }
            });
        }

        private static void OnSliderValueChangeScrollContainer(GameObject container, float value)
        {
            var gridPanelRect = container.GetComponent<RectTransform>();
            gridPanelRect.anchoredPosition = new Vector2(gridPanelRect.anchoredPosition.x, Mathf.Min((value * (_lobbyInfoList.Count - 7f) * 105f) - 440f, ((_lobbyInfoList.Count - 8f) * 105f) + 74f - 440f)); //This is so scuffed I fucking love it
        }

        public void RefreshAllLobbyInfo()
        {
            _lobbyInfoRowsList.ForEach(row => GameObject.DestroyImmediate(row));
            _lobbyInfoRowsList.Clear();

        }

        private IEnumerator<WaitForSeconds> DelayDisplayLobbyInfo(float delay, SerializableClass.MultiplayerLobbyInfo lobby, Action<SerializableClass.MultiplayerLobbyInfo> callback)
        {
            yield return new WaitForSeconds(delay);
            callback(lobby);
        }

        public void DisplayLobbyInfo(SerializableClass.MultiplayerLobbyInfo lobbyInfo)
        {
            var lobbyInfoContainer = GameObject.Instantiate(MultiplayerAssetManager.GetPrefab("containerboxhorizontal"), _lobbyInfoContainer.transform);
            var button = lobbyInfoContainer.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerEnterEvent = new EventTrigger.Entry();
            pointerEnterEvent.eventID = EventTriggerType.PointerEnter;
            pointerEnterEvent.callback.AddListener((data) => OnMouseEnterDisplayLobbyDetails(lobbyInfo, lobbyInfoContainer));
            button.triggers.Add(pointerEnterEvent);

            EventTrigger.Entry pointerClickEvent = new EventTrigger.Entry();
            pointerClickEvent.eventID = EventTriggerType.PointerClick;
            pointerClickEvent.callback.AddListener((data) => OnMouseClickSelectLobby(lobbyInfo, lobbyInfoContainer));
            button.triggers.Add(pointerClickEvent);

            button.triggers.Add(_pointerExitLobbyContainerEvent);

            lobbyInfoContainer.transform.eulerAngles = new Vector3(270, 25, 0);
            var test = GameObject.Instantiate(MultiplayerAssetManager.GetPrefab("containerboxvertical"), lobbyInfoContainer.transform);
            var t1 = GameObjectFactory.CreateSingleText(test.transform, "LobbyName", lobbyInfo.name, Color.white);
            var t2 = GameObjectFactory.CreateSingleText(test.transform, "LobbyState", lobbyInfo.currentState, Color.white);
            t1.alignment = t2.alignment = TextAlignmentOptions.Left;
            GameObjectFactory.CreateSingleText(lobbyInfoContainer.transform, "LobbyTitle", $"{lobbyInfo.title}", Color.white);
            var test2 = GameObject.Instantiate(MultiplayerAssetManager.GetPrefab("containerboxvertical"), lobbyInfoContainer.transform);
            var t3 = GameObjectFactory.CreateSingleText(test2.transform, "LobbyCount", $"{lobbyInfo.users.Count}/{lobbyInfo.maxPlayerCount}", Color.white);
            var t4 = GameObjectFactory.CreateSingleText(test2.transform, "LobbyPing", $"{lobbyInfo.ping}ms", Color.white);
            t3.alignment = t4.alignment = TextAlignmentOptions.Right;
            AnimationManager.AddNewEulerAngleAnimation(lobbyInfoContainer, new Vector3(25, 25, 0), 2f, new EasingHelper.SecondOrderDynamics(1.25f, 1f, 1f));
        }

        public void OnMouseEnterDisplayLobbyDetails(SerializableClass.MultiplayerLobbyInfo lobbyInfo, GameObject lobbyContainer) //TODO: Add small outline to hovered lobby
        {
            _lobbyPlayerListText.text = "<u>Player List</u>\n";
            lobbyInfo.users.ForEach(u => _lobbyPlayerListText.text += $"{u.username}\n");
        }

        public void OnMouseExitClearLobbyDetails()
        {
            if (_selectedLobby != null)
                OnMouseEnterDisplayLobbyDetails(_selectedLobby, _selectedLobbyContainer);
            else
                _lobbyPlayerListText.text = "";
        }

        public void OnMouseClickSelectLobby(SerializableClass.MultiplayerLobbyInfo lobbyInfo, GameObject lobbyContainer)
        {
            if (_selectedLobby == lobbyInfo) return;

            if (_selectedLobbyContainer != null)
                GameObject.DestroyImmediate(_selectedLobbyContainer.GetComponent<Outline>());

            _selectedLobby = lobbyInfo;
            _selectedLobbyContainer = lobbyContainer;
            var outline = _selectedLobbyContainer.AddComponent<Outline>();
            outline.effectColor = new Color(1, 0, 0);
            outline.effectDistance = Vector2.one * 5f;

            _connectButtonScaleAnimation?.Dispose();
            _connectButton.gameObject.SetActive(true);
            _connectButton.transform.localScale = Vector2.zero;
            _connectButton.gameObject.GetComponent<RectTransform>().pivot = Vector2.one / 2f;
            _connectButtonScaleAnimation = AnimationManager.AddNewScaleAnimation(_connectButton.gameObject, Vector3.one, 1f, new EasingHelper.SecondOrderDynamics(2.5f, 0.98f, 1.1f));
        }

        public void OnConnectButtonClick()
        {
            if (_selectedLobby == null) return;
        }

        public void UpdateLobbyInfo(bool delay)
        {
            for (int i = 0; i < _lobbyInfoList.Count; i++)
            {
                if (delay)
                    Plugin.Instance.StartCoroutine(DelayDisplayLobbyInfo(i * .1f, _lobbyInfoList[i], DisplayLobbyInfo));
                else
                    DisplayLobbyInfo(_lobbyInfoList[i]);
            }
            _scrollingHandler.enabled = _lobbyInfoList.Count > 7;
            _scrollingHandler.slider.value = 0;
        }

        public void OnHostLobbyButtonClick()
        {
            MultiplayerManager.UpdateMultiplayerState(MultiplayerState.CreatingLobby);

            UpdateCreateLobbyInfo();
        }

        public void OnConnectLobbyButtonClick()
        {
            MultiplayerManager.UpdateMultiplayerState(MultiplayerState.Lobby);
        }

        public void UpdateCreateLobbyInfo()
        {

            UpdateLobbyInfoData();
        }

        public void UpdateLobbyInfoData()
        {
        }

        public void CreateNewLobby(SerializableClass.MultiplayerLobbyInfo lobbyInfo)
        {
            _lobbyInfoList.Add(lobbyInfo);
            RefreshAllLobbyInfo();
        }

        public void AnimateHomeScreenPanels()
        {

        }

        public void EnterMainPanelAnimation()
        {

        }

        public void OnEnterState()
        {
            if (TootTally.Plugin.userInfo.username != "emmett" || false) //temporary
                MultiplayerManager.UpdateMultiplayerState(MultiplayerController.MultiplayerState.FirstTimePopUp);
            else
                MultiplayerManager.UpdateMultiplayerState(MultiplayerController.MultiplayerState.LoadPanels);
        }

        public void OnAcceptButtonClick()
        {

        }

        public void OnDeclineButtonClick()
        {
            MultiplayerManager.UpdateMultiplayerState(MultiplayerState.ExitScene);
        }

        public void DestroyFactTextTopBarAndAcceptDeclineButtons()
        {

        }

        public void OnExitAnimation()
        {
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
