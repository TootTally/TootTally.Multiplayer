using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using TootTally.Graphics;
using TootTally.Graphics.Animation;
using TootTally.Utils;
using TootTally.Utils.APIServices;
using TootTally.Utils.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static TootTally.Multiplayer.MultiplayerController;

namespace TootTally.Multiplayer.MultiplayerPanels
{
    public class MultiplayerMainPanel : MultiplayerPanelBase
    {
        public GameObject topPanelContainer;
        public GameObject lobbyListContainer, lobbyInfoContainer, lobbyConnectContainer;
        private List<GameObject> _lobbyInfoRowsList;

        private Slider _slider;
        private ScrollableSliderHandler _scrollingHandler;

        private static EventTrigger.Entry _pointerExitLobbyContainerEvent;

        private TMP_Text _lobbyPlayerListText;

        private static CustomButton _connectButton, _createLobbyButton;
        private static CustomAnimation _connectButtonScaleAnimation;

        private static SerializableClass.MultiplayerLobbyInfo _selectedLobby;
        private static GameObject _selectedLobbyContainer;

        public MultiplayerMainPanel(GameObject canvas, MultiplayerController controller) : base(canvas, controller, "MainPanel")
        {
            topPanelContainer = panelFG.transform.Find("TopMain/TopMainContainer").gameObject;
            lobbyListContainer = panelFG.transform.Find("BottomMain/LeftPanel/LeftPanelContainer").gameObject;
            lobbyInfoContainer = panelFG.transform.Find("BottomMain/RightPanel/TopContainer").gameObject;
            lobbyConnectContainer = panelFG.transform.Find("BottomMain/RightPanel/BottomContainer").gameObject;

            panel.transform.localScale = Vector2.zero;

            _lobbyInfoRowsList = new List<GameObject>();

            var connectLayout = lobbyConnectContainer.GetComponent<VerticalLayoutGroup>();
            connectLayout.childControlHeight = connectLayout.childControlWidth = false;
            connectLayout.childAlignment = TextAnchor.MiddleCenter;

            var titleText = GameObjectFactory.CreateSingleText(topPanelContainer.transform, "TitleText", "TootTally Multiplayer", Color.white);
            titleText.enableAutoSizing = true;
            titleText.alignment = TextAlignmentOptions.Left;
            var serverText = GameObjectFactory.CreateSingleText(topPanelContainer.transform, "ServerText", "Server: Toronto", Color.white);
            serverText.enableAutoSizing = true;
            serverText.alignment = TextAlignmentOptions.Right;

            _slider = new GameObject("ContainerSlider", typeof(Slider)).GetComponent<Slider>();
            _slider.gameObject.SetActive(true);
            _slider.onValueChanged.AddListener((value) => controller.OnSliderValueChangeScrollContainer(lobbyListContainer, value));
            _scrollingHandler = _slider.gameObject.AddComponent<ScrollableSliderHandler>();
            _scrollingHandler.enabled = false;

            _lobbyPlayerListText = GameObjectFactory.CreateSingleText(lobbyInfoContainer.transform, "LobbyDetailInfoText", "PlaceHolder", Color.white);
            _lobbyPlayerListText.enableAutoSizing = true;
            _lobbyPlayerListText.fontSizeMax = 42;
            _lobbyPlayerListText.alignment = TextAlignmentOptions.TopLeft;

            _pointerExitLobbyContainerEvent = new EventTrigger.Entry();
            _pointerExitLobbyContainerEvent.eventID = EventTriggerType.PointerExit;
            _pointerExitLobbyContainerEvent.callback.AddListener((data) => OnMouseExitClearLobbyDetails());

            _createLobbyButton = GameObjectFactory.CreateCustomButton(lobbyConnectContainer.transform, Vector2.zero, new Vector2(150, 75), "Create", "LobbyCreateButton", OnCreateLobbyButtonClick);

            _connectButton = GameObjectFactory.CreateCustomButton(lobbyConnectContainer.transform, Vector2.zero, new Vector2(150, 75), "Connect", "LobbyConnectButton", OnConnectButtonClick);
            _connectButton.gameObject.SetActive(false);
        }

        public void DisplayLobby(SerializableClass.MultiplayerLobbyInfo lobbyInfo)
        {
            var lobbyContainer = GameObject.Instantiate(MultiplayerAssetManager.GetPrefab("containerboxhorizontal"), lobbyListContainer.transform);
            _lobbyInfoRowsList.Add(lobbyContainer);
            var button = lobbyContainer.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerEnterEvent = new EventTrigger.Entry();
            pointerEnterEvent.eventID = EventTriggerType.PointerEnter;
            pointerEnterEvent.callback.AddListener((data) => OnMouseEnterDisplayLobbyDetails(lobbyInfo, lobbyContainer));
            button.triggers.Add(pointerEnterEvent);

            EventTrigger.Entry pointerClickEvent = new EventTrigger.Entry();
            pointerClickEvent.eventID = EventTriggerType.PointerClick;
            pointerClickEvent.callback.AddListener((data) => OnMouseClickSelectLobby(lobbyInfo, lobbyContainer));
            button.triggers.Add(pointerClickEvent);

            button.triggers.Add(_pointerExitLobbyContainerEvent);

            lobbyContainer.transform.eulerAngles = new Vector3(270, 25, 0);
            var test = GameObject.Instantiate(MultiplayerAssetManager.GetPrefab("containerboxvertical"), lobbyContainer.transform);
            var t1 = GameObjectFactory.CreateSingleText(test.transform, "LobbyName", lobbyInfo.name, Color.white);
            var t2 = GameObjectFactory.CreateSingleText(test.transform, "LobbyState", lobbyInfo.currentState, Color.white);
            t1.alignment = t2.alignment = TextAlignmentOptions.Left;
            var t5 = GameObjectFactory.CreateSingleText(lobbyContainer.transform, "LobbyTitle", $"{lobbyInfo.title}", Color.white);
            t5.alignment = TextAlignmentOptions.Right;
            var test2 = GameObject.Instantiate(MultiplayerAssetManager.GetPrefab("containerboxvertical"), lobbyContainer.transform);
            var t3 = GameObjectFactory.CreateSingleText(test2.transform, "LobbyCount", $"{lobbyInfo.users.Count}/{lobbyInfo.maxPlayerCount}", Color.white);
            var t4 = GameObjectFactory.CreateSingleText(test2.transform, "LobbyPing", $"{lobbyInfo.ping}ms", Color.white);
            t3.alignment = t4.alignment = TextAlignmentOptions.Right;
            AnimationManager.AddNewEulerAngleAnimation(lobbyContainer, new Vector3(25, 25, 0), 2f, new EasingHelper.SecondOrderDynamics(1.25f, 1f, 1f));
        }

        public void UpdateScrolling(int lobbyCount)
        {
            _scrollingHandler.enabled = lobbyCount > 7;
            _slider.value = 0;
        }

        public void OnMouseEnterDisplayLobbyDetails(SerializableClass.MultiplayerLobbyInfo lobbyInfo, GameObject lobbyContainer) //TODO: Add small outline to hovered lobby
        {
            _lobbyPlayerListText.text = "<u>Player List</u>\n";
            lobbyInfo.users.ForEach(u => _lobbyPlayerListText.text += $"{u.username}\n");
            controller.CurrentInstance.sfx_hover.Play();
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
            controller.CurrentInstance.sfx_hover.Play();
        }

        public void ClearAllLobby()
        {
            _lobbyInfoRowsList.ForEach(GameObject.DestroyImmediate);
            _lobbyInfoRowsList.Clear();
        }

        public void OnCreateLobbyButtonClick()
        {
            _scrollingHandler.enabled = false;
            controller.CreateNewLobby(new SerializableClass.MultiplayerLobbyInfo()); //TODO
        }

        public void OnLobbyBackButtonClick()
        {
            _scrollingHandler.enabled = _lobbyInfoRowsList.Count > 7;
            controller.ReturnToLobby();
        }

        public void OnConnectButtonClick()
        {
            if (_selectedLobby == null) return;

            controller.ConnectToLobby(_selectedLobby);
        }
    }
}
