﻿using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using TootTally.Utils;

namespace TootTally.Multiplayer
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin, ITootTallyModule
    {
        public static Plugin Instance;
        public string Name { get => PluginInfo.PLUGIN_NAME; set => Name = value; }
        public bool IsConfigInitialized { get; set; }
        public ConfigEntry<bool> ModuleConfigEnabled { get; set; }

        public void LogInfo(string msg) => Logger.LogInfo(msg);
        public void LogError(string msg) => Logger.LogError(msg);


        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;

            ModuleConfigEnabled = TootTally.Plugin.Instance.Config.Bind("Modules", "Multiplayer", true, "Enable TootTally's Multiplayer Module");
            OptionalTrombSettings.Add(TootTally.Plugin.Instance.moduleSettings, ModuleConfigEnabled);
            TootTally.Plugin.AddModule(this);
        }

        public void Update() { }

        public void LoadModule()
        {
            Harmony.CreateAndPatchAll(typeof(MultiplayerManager), PluginInfo.PLUGIN_GUID);
            LogInfo($"Module loaded!");
        }

        public void UnloadModule()
        {
            Harmony.UnpatchID(PluginInfo.PLUGIN_GUID);
            LogInfo($"Module unloaded!");
        }

   
    }
}
