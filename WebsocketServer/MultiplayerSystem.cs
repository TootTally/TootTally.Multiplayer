using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TootTally.Spectating;
using TootTally.Utils;

namespace TootTally.Multiplayer.WebsocketServer
{
    public class MultiplayerSystem : WebsocketManager
    {
        public Action OnWebSocketOpenCallback;

        public string GetServerID => _id;

        public MultiplayerSystem(string serverID, bool isHost) : base(serverID, "wss://spec.toottally.com/mp/join/", "1.0.0")
        {
            ConnectionPending = true;
            ConnectToWebSocketServer(_url + serverID, isHost);
        }

        protected override void OnWebSocketOpen(object sender, EventArgs e)
        {
            PopUpNotifManager.DisplayNotif($"Connected to multiplayer server.");
            OnWebSocketOpenCallback?.Invoke();
            base.OnWebSocketOpen(sender, e);
        }

        public void Disconnect()
        {
            if (!IsHost)
                PopUpNotifManager.DisplayNotif($"Disconnected from multiplayer server.");
            if (IsConnected)
                CloseWebsocket();
        }
    }
}
