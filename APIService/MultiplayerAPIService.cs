﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TootTally.Utils;
using UnityEngine.Networking;
using static TootTally.Multiplayer.APIService.MultSerializableClasses;

namespace TootTally.Multiplayer.APIService
{
    public static class MultiplayerAPIService
    {
        public const string MULTURL = "https://spec.toottally.com/mp";


        public static IEnumerator<UnityWebRequestAsyncOperation> GetServerList(Action<List<MultiplayerLobbyInfo>> callback)
        {
            string query = $"{MULTURL}/list";

            UnityWebRequest webRequest = UnityWebRequest.Get(query);

            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                var songData = JsonConvert.DeserializeObject<APIMultiplayerInfo>(webRequest.downloadHandler.text).lobbies;
                callback(songData);
            }
            else
                callback(null);
        }

        public static void CreateServer(string name, string description, string password, int maxPlayer, Action<string> callback)
        {
            Plugin.Instance.StartCoroutine(TootTallyAPIService.CreateMultiplayerServerRequest(name, description, password, maxPlayer, callback)); //Have to go through TootTallyAPI for security reasons - not exposing APIKey
        }

        private static UnityWebRequest PostUploadRequest(string query, byte[] data, string contentType = "application/json")
        {
            DownloadHandler dlHandler = new DownloadHandlerBuffer();
            UploadHandler ulHandler = new UploadHandlerRaw(data);
            ulHandler.contentType = contentType;


            UnityWebRequest webRequest = new UnityWebRequest(query, "POST", dlHandler, ulHandler);
            return webRequest;
        }

        private static bool HasError(UnityWebRequest webRequest)
        {
            return webRequest.isNetworkError || webRequest.isHttpError;
        }

        private static bool HasError(UnityWebRequest webRequest, string query)
        {
            if (webRequest.isNetworkError || webRequest.isHttpError)
                Plugin.Instance.LogError($"QUERY ERROR: {query}");
            if (webRequest.isNetworkError)
                Plugin.Instance.LogError($"NETWORK ERROR: {webRequest.error}");
            if (webRequest.isHttpError)
                Plugin.Instance.LogError($"HTTP ERROR {webRequest.error}");

            return webRequest.isNetworkError || webRequest.isHttpError;
        }
    }
}
