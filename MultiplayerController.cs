﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using TootTally.Graphics;
using TootTally.Graphics.Animation;
using TootTally.Utils;
using TootTally.Utils.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TootTally.Multiplayer
{
    public class MultiplayerController
    {
        private static PlaytestAnims _currentInstance;
        private static GameObject _mainPanel, _mainPanelFg, _mainPanelBorder, _acceptButton, _declineButton, _topBar;
        private static GameObject _activeLobbyPanel, _titlePanel, _lobbyInfoPanel, _buttonsPanel, _createLobbyPanel;

        private static GameObject _lobbyTitleInputHolder, _lobbyPasswordInputHolder;
        private static TMP_Text _lobbyNameTitle, _lobbyPasswordText, _lobbyMaxPlayerText;
        private static int _createLobbyMaxPlayerCount;

        private static CanvasGroup _acceptButtonCanvasGroup, _topBarCanvasGroup, _mainTextCanvasGroup, _declineButtonCanvasGroup;
        private static List<SerializableClass.MultiplayerLobbyInfo> _lobbyInfoList;
        private static List<GameObject> _lobbyInfoRowsList;

        private static SerializableClass.MultiplayerLobbyInfo _createLobbyInfo;
        private static List<TMP_Text> _lobbyInfoTextList;
        private static CustomButton _connectButton, _createLobbyButton;

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

            _mainPanel = GameObjectFactory.CreateMultiplayerMainPanel(panelTransform, "MultiPanel");

            _mainPanelFg = _mainPanel.transform.Find("panelfg").gameObject;
            _mainPanelFg.AddComponent<Mask>();

            _mainPanelBorder = _mainPanel.transform.Find("Panelbg1").gameObject;

            _topBar = _mainPanel.transform.Find("top").gameObject;
            _topBarCanvasGroup = _topBar.GetComponent<CanvasGroup>();
            _mainTextCanvasGroup = _mainPanelFg.transform.Find("FactText").GetComponent<CanvasGroup>();

            _lobbyInfoList = new List<SerializableClass.MultiplayerLobbyInfo>();
            _lobbyInfoRowsList = new List<GameObject>();
            _lobbyInfoTextList = new List<TMP_Text>();

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

        public void AddAcceptDeclineButtonsToPanelFG()
        {
            _acceptButton = GameObjectFactory.CreateCustomButton(_mainPanelFg.transform, new Vector2(-80, -340), new Vector2(200, 50), "Accept", "AcceptButton", OnAcceptButtonClick).gameObject;
            _acceptButtonCanvasGroup = _acceptButton.AddComponent<CanvasGroup>();
            _declineButton = GameObjectFactory.CreateCustomButton(_mainPanelFg.transform, new Vector2(-320, -340), new Vector2(200, 50), "Decline", "DeclineButton", OnDeclineButtonClick).gameObject;
            _declineButtonCanvasGroup = _declineButton.AddComponent<CanvasGroup>();
        }

        public void OnLoadPanelsScreensState()
        {
            DestroyFactTextTopBarAndAcceptDeclineButtons();
            AddHomeScreenPanelsToMainPanel();
        }

        public void RefreshAllLobbyInfo()
        {
            _lobbyInfoRowsList.ForEach(row => GameObject.DestroyImmediate(row));
            _lobbyInfoRowsList.Clear();

            GameObject leftPanelFG = _activeLobbyPanel.transform.Find("panelfg").gameObject;

            foreach (SerializableClass.MultiplayerLobbyInfo multiLobbyInfo in _lobbyInfoList)
                _lobbyInfoRowsList.Add(GameObjectFactory.CreateLobbyInfoRow(leftPanelFG.transform, $"{multiLobbyInfo.name}Lobby", multiLobbyInfo, delegate { DisplayLobbyInfo(multiLobbyInfo); }));
        }

        public void AddHomeScreenPanelsToMainPanel()
        {
            #region TitlePanel
            _titlePanel = GameObjectFactory.CreateEmptyMultiplayerPanel(_mainPanelFg.transform, "TitlePanel", new Vector2(1230, 50), new Vector2(0, 284));
            _titlePanel.GetComponent<RectTransform>().localScale = Vector2.zero;
            HorizontalLayoutGroup topPanelLayoutGroup = _titlePanel.transform.Find("panelfg").gameObject.AddComponent<HorizontalLayoutGroup>();
            topPanelLayoutGroup.padding = new RectOffset(8, 8, 8, 8);
            TMP_Text lobbyText = GameObjectFactory.CreateSingleText(_titlePanel.transform.Find("panelfg"), "TitleText", "TootTally Multiplayer Lobbies", Color.white);
            lobbyText.alignment = TextAlignmentOptions.MidlineLeft;
            TMP_Text serverText = GameObjectFactory.CreateSingleText(_titlePanel.transform.Find("panelfg"), "ServerText", "Current Server: localHost", Color.white);
            serverText.alignment = TextAlignmentOptions.MidlineLeft;
            #endregion

            #region ActiveLobbyPanel
            _activeLobbyPanel = GameObjectFactory.CreateEmptyMultiplayerPanel(_mainPanelFg.transform, "ActiveLobbyPanel", new Vector2(750, 564), new Vector2(-240, -28));
            _activeLobbyPanel.GetComponent<RectTransform>().localScale = Vector2.zero;
            VerticalLayoutGroup leftPanelLayoutGroup = _activeLobbyPanel.transform.Find("panelfg").gameObject.AddComponent<VerticalLayoutGroup>();
            leftPanelLayoutGroup.childForceExpandHeight = leftPanelLayoutGroup.childScaleHeight = leftPanelLayoutGroup.childControlHeight = false;
            leftPanelLayoutGroup.padding = new RectOffset(8, 8, 8, 8);
            GetLobbyInfo();
            RefreshAllLobbyInfo();
            #endregion

            #region LobbyInfoPanel
            _lobbyInfoPanel = GameObjectFactory.CreateEmptyMultiplayerPanel(_mainPanelFg.transform, "LobbyInfoPanel", new Vector2(426, 280), new Vector2(402, -170));
            _lobbyInfoPanel.GetComponent<RectTransform>().localScale = Vector2.zero;
            VerticalLayoutGroup lobbyInfoLayoutGroup = _lobbyInfoPanel.transform.Find("panelfg").gameObject.AddComponent<VerticalLayoutGroup>();
            lobbyInfoLayoutGroup.childForceExpandHeight = lobbyInfoLayoutGroup.childScaleHeight = lobbyInfoLayoutGroup.childControlHeight = false;
            lobbyInfoLayoutGroup.padding = new RectOffset(8, 8, 8, 8);
            #endregion

            #region ButtonsPanel
            _buttonsPanel = GameObjectFactory.CreateEmptyMultiplayerPanel(_mainPanelFg.transform, "ButtonsPanel", new Vector2(426, 280), new Vector2(402, 114));
            _buttonsPanel.GetComponent<RectTransform>().localScale = Vector2.zero;
            GameObjectFactory.CreateCustomButton(_buttonsPanel.transform, Vector2.one, new Vector2(190, 60), "Host Lobby", "HostLobbyButton", OnHostLobbyButtonClick);
            #endregion

            #region CreateLobbyPanel
            _createLobbyPanel = GameObjectFactory.CreateEmptyMultiplayerPanel(_mainPanelFg.transform, "CreateLobbyPanel", new Vector2(750, 564), new Vector2(1041, -28));
            GameObject lobbyPanelFG = _createLobbyPanel.transform.Find("panelfg").gameObject;
            VerticalLayoutGroup lobbyPanelLayoutGroup = lobbyPanelFG.AddComponent<VerticalLayoutGroup>();

            //Lobby Name Input Field
            _lobbyNameTitle = GameObjectFactory.CreateSingleText(lobbyPanelFG.transform, "LobbyNameText", $"{TootTally.Plugin.userInfo.username}'s Lobby", Color.white);
            _lobbyTitleInputHolder = GameObject.Instantiate(_lobbyNameTitle.gameObject, _lobbyNameTitle.transform);
            _lobbyTitleInputHolder.name = "LobbyNameInput";
            GameObject.DestroyImmediate(_lobbyTitleInputHolder.GetComponent<Text>());

            TMP_InputField lobbyNameInputField = _lobbyTitleInputHolder.AddComponent<TMP_InputField>();
            lobbyNameInputField.textComponent = _lobbyNameTitle;
            lobbyNameInputField.textComponent.alignment = TextAlignmentOptions.MidlineLeft;
            lobbyNameInputField.image = _lobbyTitleInputHolder.AddComponent<Image>();
            lobbyNameInputField.image.color = GameTheme.themeColors.leaderboard.rowEntry;
            lobbyNameInputField.text = $"{TootTally.Plugin.userInfo.username}'s Lobby";
            lobbyNameInputField.onValueChanged.AddListener((sender) => UpdateCreateLobbyInfo());

            //Lobby Password Input Field
            _lobbyPasswordText = GameObjectFactory.CreateSingleText(lobbyPanelFG.transform, "LobbyPasswordText", $"Password", Color.white);
            _lobbyPasswordInputHolder = GameObject.Instantiate(_lobbyPasswordText.gameObject, _lobbyPasswordText.transform);
            _lobbyPasswordInputHolder.name = "LobbyPasswordInput";
            GameObject.DestroyImmediate(_lobbyPasswordInputHolder.GetComponent<Text>());

            TMP_InputField lobbyPasswordInputField = _lobbyPasswordInputHolder.AddComponent<TMP_InputField>();
            lobbyPasswordInputField.textComponent = _lobbyPasswordText;
            lobbyPasswordInputField.textComponent.alignment = TextAlignmentOptions.MidlineLeft;
            lobbyPasswordInputField.image = _lobbyPasswordInputHolder.AddComponent<Image>();
            lobbyPasswordInputField.image.color = GameTheme.themeColors.leaderboard.rowEntry;
            lobbyPasswordInputField.text = $"Password";
            lobbyPasswordInputField.onValueChanged.AddListener((sender) => UpdateCreateLobbyInfo());

            _createLobbyMaxPlayerCount = 8;
            _lobbyMaxPlayerText = GameObjectFactory.CreateSingleText(lobbyPanelFG.transform, "LobbyMaxPlayerText", $"Max Player: {_createLobbyMaxPlayerCount}", Color.white);
            _lobbyMaxPlayerText.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            GameObjectFactory.CreateCustomButton(_lobbyMaxPlayerText.transform, new Vector2(-250, -76), new Vector2(30, 30), "▲", "IncreaseMaxPlayerButton", delegate { OnMaxPlayerButtonPress(1); });
            GameObjectFactory.CreateCustomButton(_lobbyMaxPlayerText.transform, new Vector2(-460, -76), new Vector2(30, 30), "▼", "DecreaseMaxPlayerButton", delegate { OnMaxPlayerButtonPress(-1); });
            #endregion
        }

        public void OnMaxPlayerButtonPress(int countChange)
        {
            _createLobbyMaxPlayerCount += countChange;
            UpdateMaxPlayerText();
            UpdateLobbyInfoData();
        }

        public void UpdateMaxPlayerText()
        {
            _lobbyMaxPlayerText.text = $"Max Player: {_createLobbyMaxPlayerCount}";
        }

        public void DisplayLobbyInfo(SerializableClass.MultiplayerLobbyInfo lobbyInfo)
        {
            if (_lobbyInfoTextList.Count > 0)
                _lobbyInfoTextList.ForEach(textObj => GameObject.DestroyImmediate(textObj.gameObject));
            if (_connectButton != null)
                GameObject.DestroyImmediate(_connectButton.gameObject);
            if (_createLobbyButton != null)
                GameObject.DestroyImmediate(_createLobbyButton.gameObject);

            _lobbyInfoTextList.Clear();
            GameObject lobbyInfoPanelFG = _lobbyInfoPanel.transform.Find("panelfg").gameObject;
            lobbyInfo.users.ForEach(user =>
            {
                TMP_Text currentText = GameObjectFactory.CreateSingleText(lobbyInfoPanelFG.transform, $"{user.username}TextInfo", $"#{user.rank} {user.username} : {user.state}", Color.white);
                currentText.alignment = TextAlignmentOptions.MidlineLeft;
                currentText.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 40);
                _lobbyInfoTextList.Add(currentText);
            });

            _connectButton = GameObjectFactory.CreateCustomButton(lobbyInfoPanelFG.transform, Vector2.zero, Vector2.one * 50, "Connect", "ConnectButton", OnConnectLobbyButtonClick);
        }

        public void OnHostLobbyButtonClick()
        {
            MultiplayerManager.UpdateMultiplayerState(MultiplayerState.CreatingLobby);
            _createLobbyInfo = new SerializableClass.MultiplayerLobbyInfo()
            {
                id = _lobbyInfoList.Count,
                maxPlayerCount = 16,
                title = _lobbyNameTitle.text,
                currentState = "",
                users = new List<SerializableClass.MultiplayerUserInfo>()
            };
            UpdateCreateLobbyInfo();
        }

        public void OnConnectLobbyButtonClick()
        {
            MultiplayerManager.UpdateMultiplayerState(MultiplayerState.Lobby);
        }

        public void UpdateCreateLobbyInfo()
        {
            if (_lobbyInfoTextList.Count > 0)
            {
                _lobbyInfoTextList.ForEach(textObj => GameObject.DestroyImmediate(textObj.gameObject));
                _lobbyInfoTextList.Clear();
            }

            if (_connectButton != null)
                GameObject.DestroyImmediate(_connectButton.gameObject);

            GameObject lobbyInfoPanelFG = _lobbyInfoPanel.transform.Find("panelfg").gameObject;

            if (_createLobbyButton == null)
                _createLobbyButton = GameObjectFactory.CreateCustomButton(lobbyInfoPanelFG.transform, Vector2.zero, Vector2.one * 50, "Create Lobby", "CreateLobbyButton", delegate { CreateNewLobby(_createLobbyInfo); });

            var lobbyTitle = _lobbyTitleInputHolder.GetComponent<InputField>().text;
            var lobbyPassword = _lobbyPasswordInputHolder.GetComponent<InputField>().text;

            TMP_Text lobbyNameText = GameObjectFactory.CreateSingleText(lobbyInfoPanelFG.transform, $"CreateLobbyNameInfo", $"{lobbyTitle}", Color.white);
            lobbyNameText.alignment = TextAlignmentOptions.MidlineLeft;
            lobbyNameText.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 40);
            _lobbyInfoTextList.Add(lobbyNameText);

            TMP_Text lobbyPasswordText = GameObjectFactory.CreateSingleText(lobbyInfoPanelFG.transform, $"CreateLobbyPasswordInfo", $"{lobbyPassword}", Color.white);
            lobbyPasswordText.alignment = TextAlignmentOptions.MidlineLeft;
            lobbyPasswordText.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 40);
            _lobbyInfoTextList.Add(lobbyPasswordText);

            UpdateLobbyInfoData();
        }

        public void UpdateLobbyInfoData()
        {
            _createLobbyInfo.maxPlayerCount = _createLobbyMaxPlayerCount;
            _createLobbyInfo.title = _lobbyTitleInputHolder.GetComponent<InputField>().text;
            _createLobbyInfo.password = _lobbyPasswordInputHolder.GetComponent<InputField>().text;
            _createLobbyInfo.currentState = "Waiting for players";
        }

        public void CreateNewLobby(SerializableClass.MultiplayerLobbyInfo lobbyInfo)
        {
            _lobbyInfoList.Add(lobbyInfo);
            RefreshAllLobbyInfo();
        }

        public void AnimateHomeScreenPanels()
        {
            AnimationManager.AddNewSizeDeltaAnimation(_mainPanelFg, new Vector2(1240, 630), 0.8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            AnimationManager.AddNewSizeDeltaAnimation(_mainPanelBorder, new Vector2(1250, 640), 0.8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f), (sender) =>
            {
                AnimationManager.AddNewScaleAnimation(_titlePanel, Vector2.one, .8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
                AnimationManager.AddNewScaleAnimation(_activeLobbyPanel, Vector2.one, .8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));

                AnimationManager.AddNewScaleAnimation(_buttonsPanel, Vector2.one, .8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
                AnimationManager.AddNewScaleAnimation(_lobbyInfoPanel, Vector2.one, .8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));

                MultiplayerManager.UpdateMultiplayerState(MultiplayerState.Home);
            });
        }

        public void EnterMainPanelAnimation()
        {
            AnimationManager.AddNewPositionAnimation(_mainPanel, new Vector2(0, -20), 2f, new EasingHelper.SecondOrderDynamics(1.25f, 1f, 0f));
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
            AnimationManager.AddNewSizeDeltaAnimation(_acceptButton, Vector2.zero, 1f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            AnimationManager.AddNewSizeDeltaAnimation(_declineButton, Vector2.zero, 1f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f), (sender) => DestroyFactTextTopBarAndAcceptDeclineButtons());
            MultiplayerManager.UpdateMultiplayerState(MultiplayerState.LoadPanels);
            _currentInstance.sfx_ok.Play();
        }

        public void OnDeclineButtonClick()
        {
            MultiplayerManager.UpdateMultiplayerState(MultiplayerState.ExitScene);
            GameObject.Destroy(_mainPanel);
        }

        public void DestroyFactTextTopBarAndAcceptDeclineButtons()
        {
            GameObject.DestroyImmediate(_mainPanelFg.transform.Find("FactText").gameObject);
            GameObject.DestroyImmediate(_topBar);
            if (_acceptButton != null)
                GameObject.DestroyImmediate(_acceptButton);
            if (_declineButton != null)
                GameObject.DestroyImmediate(_declineButton);
        }

        public void OnExitAnimation()
        {
            AnimationManager.AddNewScaleAnimation(_mainPanel, Vector2.zero, 2f, new EasingHelper.SecondOrderDynamics(.75f, 1f, 0f));
        }

        public void AnimatePanelPositions(Vector2 newTitlePanelPosition, Vector2 newActiveLobbyPanelPosition, Vector2 newButtonsPanelPosition, Vector2 newLobbyInfoPanelPosition, Vector2 newCreateLobbyPanelPosition)
        {
            if (newTitlePanelPosition != null)
                AnimationManager.AddNewPositionAnimation(_titlePanel, newTitlePanelPosition, 1.2f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            if (newActiveLobbyPanelPosition != null)
                AnimationManager.AddNewPositionAnimation(_activeLobbyPanel, newActiveLobbyPanelPosition, 1.2f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            if (newButtonsPanelPosition != null)
                AnimationManager.AddNewPositionAnimation(_buttonsPanel, newButtonsPanelPosition, 1.2f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            if (newLobbyInfoPanelPosition != null)
                AnimationManager.AddNewPositionAnimation(_lobbyInfoPanel, newLobbyInfoPanelPosition, 1.2f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            if (newCreateLobbyPanelPosition != null)
                AnimationManager.AddNewPositionAnimation(_createLobbyPanel, newCreateLobbyPanelPosition, 1.2f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
        }


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
