using System;
using System.Collections.Generic;

namespace TootTally.Multiplayer.APIService
{
    public static class MultSerializableClasses
    {
        [Serializable]
        public class APIMultiplayerInfo
        {
            public int count { get; set; }
            public List<MultiplayerLobbyInfo> lobbies { get; set; }
        }

        [Serializable]
        public class MultiplayerLobbyInfo
        {
            public string id;
            public string name;
            public string title;
            public string password;
            public int maxPlayerCount;
            public string currentState;
            public float ping;
            public List<MultiplayerUserInfo> users;
        }

        [Serializable]
        public class MultiplayerUserInfo
        {
            public int id;
            public string username;
            public string country;
            public int rank;
            public int state; // states like "spectator" "ready" "not ready" etc.
        }

    }
}
