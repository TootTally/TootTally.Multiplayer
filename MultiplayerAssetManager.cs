using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TootTally.Graphics;
using TootTally.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace TootTally.Multiplayer
{
    public static class MultiplayerAssetManager
    {
        private const string ASSET_BUNDLE_FILE_NAME = "multiplayerassetbundle";
        private static AssetBundle _assetBundle;
        private static Dictionary<string, GameObject> _prefabDict;
        private static bool _isInitialized;
        public static void Initialize()
        {
            _prefabDict = new Dictionary<string, GameObject>();

            string bundlePath = Path.Combine(Path.GetDirectoryName(Plugin.Instance.Info.Location), ASSET_BUNDLE_FILE_NAME);
            if (File.Exists(bundlePath))
                Plugin.Instance.StartCoroutine(LoadAssetBundle(bundlePath, OnAssetBundleLoaded));
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> LoadAssetBundle(string filePath, Action<AssetBundle> callback)
        {
            Plugin.Instance.LogInfo(filePath);
            UnityWebRequest webRequest = UnityWebRequestAssetBundle.GetAssetBundle(filePath);
            yield return webRequest.SendWebRequest();

            if (!webRequest.isNetworkError && !webRequest.isHttpError)
                callback(DownloadHandlerAssetBundle.GetContent(webRequest));
            else
                Plugin.Instance.LogInfo("AssetBundle failed to load.");
        }

        public static void OnAssetBundleLoaded(AssetBundle assetBundle)
        {
            if (assetBundle == null)
            {
                Plugin.Instance.LogInfo("AssetBundle was null");
                return;
            }
            _assetBundle = assetBundle;
            _assetBundle.GetAllAssetNames().ToList().ForEach(name => _prefabDict.Add(Path.GetFileNameWithoutExtension(name), _assetBundle.LoadAsset<GameObject>(name)));
            _isInitialized = true;
        }

        public static GameObject GetPrefab(string name)
        {
            if (!_isInitialized || _prefabDict == null) return _defaultGameObject;

            if (!_prefabDict.ContainsKey(name))
            {
                Plugin.Instance.LogInfo($"Couldn't find asset bundle {name}");
                return _defaultGameObject;
            }

            return _prefabDict[name];
        }

        public static void Dispose()
        {
            _assetBundle.Unload(true);
            _prefabDict.Clear();
            _prefabDict = null;
        }

        private static GameObject _defaultGameObject => GameObjectFactory.CreateImageHolder(null, Vector2.zero, new Vector2(64, 64), AssetManager.GetSprite("icon.png"), "DefaultGameObject");
    }
}
