using Rewired.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using TootTally.Graphics;
using TootTally.Utils.APIServices;
using UnityEngine;

namespace TootTally.Multiplayer.MultiplayerPanels
{
    public class MultiplayerLobbyPanel : MultiplayerPanelBase
    {
        public GameObject lobbyUserContainer, rightPanelContainer, rightPanelContainerBox;
        public GameObject bottomPanelContainer;

        private List<GameObject> _userRowsList;

        private CustomButton _backButton, _selectSongButton;
        public MultiplayerLobbyPanel(GameObject canvas, MultiplayerController controller) : base(canvas, controller, "LobbyPanel")
        {
            lobbyUserContainer = panelFG.transform.Find("TopMain/LeftPanel/LeftPanelContainer").gameObject;
            rightPanelContainer = panelFG.transform.Find("TopMain/RightPanel/RightPanelContainer").gameObject;
            rightPanelContainerBox = rightPanelContainer.transform.Find("ContainerBoxVertical").gameObject;
            bottomPanelContainer = panelFG.transform.Find("BottomMain/BottomMainContainer").gameObject;

            _userRowsList = new List<GameObject>();

            _backButton = GameObjectFactory.CreateCustomButton(bottomPanelContainer.transform, Vector2.zero, new Vector2(150, 75), "Back", "LobbyBackButton", OnBackButtonClick);
            _selectSongButton = GameObjectFactory.CreateCustomButton(bottomPanelContainer.transform, Vector2.zero, new Vector2(150, 75), "SelectSong", "SelectSongButton", OnSelectSongButtonClick);
        }

        public void DIsplayAllUserInfo(List<SerializableClass.MultiplayerUserInfo> users) => users.ForEach(DisplayUserInfo);

        public void DisplayUserInfo(SerializableClass.MultiplayerUserInfo user)
        {
            var lobbyInfoContainer = GameObject.Instantiate(MultiplayerAssetManager.GetPrefab("containerboxhorizontal"), lobbyUserContainer.transform);
            _userRowsList.Add(lobbyInfoContainer);
            lobbyInfoContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 75);

            var t1 = GameObjectFactory.CreateSingleText(lobbyInfoContainer.transform, $"Lobby{user.username}Name", $"{user.username}", Color.white);
            t1.alignment = TextAlignmentOptions.Left;
            var t2 = GameObjectFactory.CreateSingleText(lobbyInfoContainer.transform, $"Lobby{user.username}Rank", $"#{user.rank}", Color.white);
            t2.alignment = TextAlignmentOptions.Right;
        }

        public void ClearAllUserRows()
        {
            _userRowsList.ForEach(GameObject.DestroyImmediate);
            _userRowsList.Clear();
        }

        public void OnBackButtonClick()
        {
            ClearAllUserRows();
            controller.ReturnToLastPanel();
        }

        public void OnSelectSongButtonClick()
        {
            controller.TransitionToSongSelection();
        }
    }
}
