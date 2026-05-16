using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using Blukulele.CHE;
using Blukulele.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GambonanzaAP
{
    [BepInPlugin("com.arci.gambonanza.ap", "Gambonanza Archipelago", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        public static ArchipelagoClient AP;

        public static ConfigEntry<string> ConfigServer;
        public static ConfigEntry<string> ConfigUser;
        public static ConfigEntry<string> ConfigPass;

        private void Awake()
        {
            Log = Logger;
            AP = new ArchipelagoClient();

            ConfigServer = Config.Bind("Archipelago", "Server", "archipelago.gg:38281", "Archipelago server address");
            ConfigUser = Config.Bind("Archipelago", "SlotName", "Player1", "Your slot name");
            ConfigPass = Config.Bind("Archipelago", "Password", "", "Server password");

            Harmony harmony = new Harmony("com.arci.gambonanza.ap");
            harmony.PatchAll();

            Log.LogInfo("Gambonanza Archipelago Loaded!");
            
            // Auto-connect on startup
            TryConnect();
        }

        private void Start()
        {
            // Cleanup any potentially corrupted save data from previous versions
            if (ChessDataManager.Instance != null && ChessDataManager.Instance.ShopLockedGambit != null)
            {
                for (int i = 0; i < ChessDataManager.Instance.ShopLockedGambit.Length; i++)
                {
                    if (ChessDataManager.Instance.ShopLockedGambit[i] != null && ChessDataManager.Instance.ShopLockedGambit[i].StartsWith("AP_CHECK_"))
                    {
                        ChessDataManager.Instance.ShopLockedGambit[i] = string.Empty;
                    }
                }
            }
        }

        private void TryConnect()
        {
            if (string.IsNullOrEmpty(ConfigServer.Value) || string.IsNullOrEmpty(ConfigUser.Value))
            {
                Log.LogWarning("Archipelago Config not set. Skipping auto-connect.");
                return;
            }

            Log.LogInfo($"Connecting to {ConfigServer.Value} as {ConfigUser.Value}...");
            if (!AP.Connect(ConfigServer.Value, ConfigUser.Value, ConfigPass.Value))
            {
                Log.LogFatal("CONNECTION FAILED! Closing game as requested.");
                UnityEngine.Application.Quit();
            }
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F8))
            {
                Log.LogInfo("Manual reconnection triggered via F8...");
                TryConnect();
            }

            AP.Update();
        }

        // We'll hijack the first gambit slot visual
        [HarmonyPatch(typeof(GambitToBuy), "Initialize")]
        public static class ShopVisualPatch
        {
            [HarmonyPostfix]
            static void Postfix(GambitToBuy __instance, SO_Gambit gambit)
            {
                // Access the private m_Index field to see if this is the first slot
                int index = (int)AccessTools.Field(typeof(GambitToBuy), "m_Index").GetValue(__instance);
                
                if (index == 0)
                {
                    int nextCheckId = AP.GetNextAvailableCheckId();
                    if (nextCheckId != -1)
                    {
                        var title = AccessTools.Field(typeof(GambitToBuy), "m_Title").GetValue(__instance) as TMPro.TMP_Text;
                        var desc = AccessTools.Field(typeof(GambitToBuy), "m_Description").GetValue(__instance) as TMPro.TMP_Text;
                        var rarity = AccessTools.Field(typeof(GambitToBuy), "m_Rarity").GetValue(__instance) as TMPro.TMP_Text;

                        if (title != null) title.text = "Archipelago Check";
                        if (desc != null) desc.text = $"Sends check #{nextCheckId} to the multiworld.";
                        if (rarity != null) rarity.text = "ARCHIPELAGO";
                        
                        __instance.gameObject.name = "AP_CHECK_SLOT";
                    }
                }
            }
        }

        [HarmonyPatch(typeof(GambitToBuy), "OnClick")]
        public static class ShopBuyPatch
        {
            [HarmonyPrefix]
            static bool Prefix(GambitToBuy __instance)
            {
                if (__instance.gameObject.name == "AP_CHECK_SLOT")
                {
                    int nextCheckId = AP.GetNextAvailableCheckId();
                    if (nextCheckId == -1) return true; // Fallback to normal behavior if no checks left

                    int price = (int)AccessTools.Field(typeof(GambitToBuy), "m_Price").GetValue(__instance);
                    if (price < 0) price = 10; // Safety fallback for price

                    if (SingletonMonoBehaviour<ChessDataManager>.Instance.Coins >= price)
                    {
                        AP.SendLocation($"Shop Check {nextCheckId}");
                        SingletonMonoBehaviour<ChessDataManager>.Instance.DecreaseCoin(price);
                        SingletonMonoBehaviour<ChessDataManager>.Instance.HasBuySomething();
                        
                        // Set 'm_bought' and 'm_Used' via reflection to match internal state
                        AccessTools.Field(typeof(GambitToBuy), "m_bought").SetValue(__instance, true);
                        AccessTools.Field(typeof(GambitToBuy), "m_Used").SetValue(__instance, true);
                        
                        __instance.Hide(); // Visually disable the card
                        Log.LogInfo($"Check {nextCheckId} purchased and sent!");
                    }

                    return false; // STOP original logic
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ChessDataManager), "LoadData")]
        public static class BoardLimitPatch
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                int upgrades = AP.ReceivedItems.Count(x => x == "Board Upgrade");
                ChessDataManager.Instance.MaxPieceOnBoard = ChessDataManager.Instance.MaxPieceOnBoardAtStart + upgrades;
            }
        }
    }
}
