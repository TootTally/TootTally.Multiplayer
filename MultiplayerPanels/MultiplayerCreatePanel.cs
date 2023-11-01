using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using TootTally.Graphics;
using TootTally.Utils.APIServices;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Multiplayer.MultiplayerPanels
{
    public class MultiplayerCreatePanel : MultiplayerPanelBase
    {
        public GameObject leftPanelContainer, leftPanelContainerBox, rightPanelContainer, rightPanelContainerBox;

        private TMP_InputField _lobbyName, _lobbyDescription, _lobbyPassword, _lobbyMaxPlayer;

        private CustomButton _backButton, _createLobbyButton;
        public MultiplayerCreatePanel(GameObject canvas, MultiplayerController controller) : base(canvas, controller, "CreatePanel")
        {
            leftPanelContainer = panelFG.transform.Find("Main/LeftPanel/LeftPanelContainer").gameObject;
            leftPanelContainerBox = leftPanelContainer.transform.Find("ContainerBoxVertical").gameObject;
            rightPanelContainer = panelFG.transform.Find("Main/RightPanel/RightPanelContainer").gameObject;
            rightPanelContainerBox = rightPanelContainer.transform.Find("ContainerBoxVertical").gameObject;

            var leftLayout = leftPanelContainerBox.GetComponent<VerticalLayoutGroup>();
            leftLayout.childControlHeight = leftLayout.childControlWidth = false;
            leftLayout.childAlignment = TextAnchor.LowerCenter;

            var rightLayout = rightPanelContainerBox.GetComponent<VerticalLayoutGroup>();
            rightLayout.childControlHeight = rightLayout.childControlWidth = false;
            rightLayout.childAlignment = TextAnchor.LowerCenter;

            _lobbyName = MultiplayerGameObjectFactory.CreateInputField(rightPanelContainerBox.transform, "LobbyNameInputField", new Vector2(300, 30), 24, "TestName", false);
            _lobbyDescription = MultiplayerGameObjectFactory.CreateInputField(rightPanelContainerBox.transform, "LobbyDescriptionInputField", new Vector2(300, 30), 24, "TestDescription", false);
            _lobbyPassword = MultiplayerGameObjectFactory.CreateInputField(rightPanelContainerBox.transform, "LobbyPasswordInputField", new Vector2(300, 30), 24, "TestPassword", false);
            _lobbyMaxPlayer = MultiplayerGameObjectFactory.CreateInputField(rightPanelContainerBox.transform, "LobbyMaxPlayerInputField", new Vector2(300,30), 24, "TestMaxPlayer", false);

            _backButton = GameObjectFactory.CreateCustomButton(leftPanelContainerBox.transform, Vector2.zero, new Vector2(150, 75), "Back", "CreateBackButton", OnBackButtonClick);
            _createLobbyButton = GameObjectFactory.CreateCustomButton(rightPanelContainerBox.transform, Vector2.zero, new Vector2(150, 75), "Create", "CreateLobbyButton", OnCreateButtonClick);
        }

        private void OnBackButtonClick()
        {
            controller.MoveToMain();
        }

        private void OnCreateButtonClick()
        {
            controller.CreateNewLobby(new SerializableClass.MultiplayerLobbyInfo()
            {
                name = _lobbyName.text,
                title = _lobbyDescription.text,
                password = _lobbyPassword.text,
                maxPlayerCount = int.Parse(_lobbyMaxPlayer.text),
                currentState = "Selecting Song",
                ping = 69f,
                users = new List<SerializableClass.MultiplayerUserInfo> { MultiplayerController._electroUser }
            });
            controller.RefreshAllLobbyInfo();
            controller.MoveToLobby();
        }
    }
}
