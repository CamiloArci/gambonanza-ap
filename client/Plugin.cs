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

        private bool showUI = false;
        private string uiServer = "";
        private string uiSlot = "";
        private string uiPassword = "";
        private string connectionError = "";
        private bool isConnecting = false;
        private UnityEngine.Rect windowRect;

        private void Awake()
        {
            Log = Logger;
            AP = new ArchipelagoClient();

            ConfigServer = Config.Bind("Archipelago", "Server", "", "Archipelago server address");
            ConfigUser = Config.Bind("Archipelago", "SlotName", "", "Your slot name");
            ConfigPass = Config.Bind("Archipelago", "Password", "", "Server password");

            uiServer = ConfigServer.Value;
            uiSlot = ConfigUser.Value;
            uiPassword = ConfigPass.Value;

            Harmony harmony = new Harmony("com.arci.gambonanza.ap");
            harmony.PatchAll();

            Log.LogInfo("Gambonanza Archipelago Loaded!");
            
            // Auto-connect on startup only if config has valid entries
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
                Log.LogWarning("Archipelago Config is blank. Opening connection UI.");
                showUI = true;
                windowRect = new UnityEngine.Rect((UnityEngine.Screen.width - 400) / 2, (UnityEngine.Screen.height - 300) / 5, 400, 300);
                return;
            }

            Log.LogInfo($"Connecting to {ConfigServer.Value} as {ConfigUser.Value}...");
            isConnecting = true;
            try
            {
                if (!AP.Connect(ConfigServer.Value, ConfigUser.Value, ConfigPass.Value))
                {
                    Log.LogError("CONNECTION FAILED! Opening connection UI.");
                    connectionError = "Connection failed. Please verify credentials/server status.";
                    showUI = true;
                    windowRect = new UnityEngine.Rect((UnityEngine.Screen.width - 400) / 2, (UnityEngine.Screen.height - 300) / 5, 400, 300);
                    isConnecting = false;
                }
                else
                {
                    showUI = false;
                    connectionError = "";
                    isConnecting = false;
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Exception during startup connection: {ex.Message}");
                connectionError = $"Error: {ex.Message}";
                showUI = true;
                windowRect = new UnityEngine.Rect((UnityEngine.Screen.width - 400) / 2, (UnityEngine.Screen.height - 300) / 5, 400, 300);
                isConnecting = false;
            }
        }

        private void ConnectFromUI()
        {
            if (string.IsNullOrEmpty(uiServer) || string.IsNullOrEmpty(uiSlot))
            {
                connectionError = "Server and Slot name cannot be empty.";
                return;
            }

            connectionError = "";
            isConnecting = true;
            Log.LogInfo($"Connecting to {uiServer} as {uiSlot}...");
            try
            {
                if (AP.Connect(uiServer, uiSlot, uiPassword))
                {
                    ConfigServer.Value = uiServer;
                    ConfigUser.Value = uiSlot;
                    ConfigPass.Value = uiPassword;
                    Config.Save();

                    showUI = false;
                    isConnecting = false;
                }
                else
                {
                    connectionError = "Connection failed. Please check logs.";
                    isConnecting = false;
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Exception during UI connection: {ex.Message}");
                connectionError = $"Error: {ex.Message}";
                isConnecting = false;
            }
        }

        private void OnGUI()
        {
            if (!showUI) return;

            // Enforce cursor visibility and interaction when UI is active
            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.None;

            windowRect = UnityEngine.GUI.Window(9999, windowRect, DrawWindow, "Archipelago Connection Settings");
        }

        private void DrawWindow(int windowID)
        {
            UnityEngine.GUI.DragWindow(new UnityEngine.Rect(0, 0, 400, 20));

            UnityEngine.GUILayout.BeginArea(new UnityEngine.Rect(20, 30, 360, 250));

            UnityEngine.GUILayout.Label("Server Address (IP:Port):");
            uiServer = UnityEngine.GUILayout.TextField(uiServer);

            UnityEngine.GUILayout.Space(10);
            UnityEngine.GUILayout.Label("Slot Name (User):");
            uiSlot = UnityEngine.GUILayout.TextField(uiSlot);

            UnityEngine.GUILayout.Space(10);
            UnityEngine.GUILayout.Label("Password (Optional):");
            uiPassword = UnityEngine.GUILayout.PasswordField(uiPassword, '*');

            UnityEngine.GUILayout.Space(15);

            if (!string.IsNullOrEmpty(connectionError))
            {
                UnityEngine.GUI.color = UnityEngine.Color.red;
                UnityEngine.GUILayout.Label(connectionError);
                UnityEngine.GUI.color = UnityEngine.Color.white;
            }

            UnityEngine.GUILayout.FlexibleSpace();

            if (isConnecting)
            {
                UnityEngine.GUILayout.Label("Connecting...");
            }
            else
            {
                if (UnityEngine.GUILayout.Button("Connect") || 
                    (UnityEngine.Event.current.type == UnityEngine.EventType.KeyDown && UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.Return))
                {
                    ConnectFromUI();
                }
            }

            UnityEngine.GUILayout.EndArea();
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F8))
            {
                showUI = !showUI;
                if (showUI)
                {
                    windowRect = new UnityEngine.Rect((UnityEngine.Screen.width - 400) / 2, (UnityEngine.Screen.height - 300) / 5, 400, 300);
                }
            }

            AP.Update();
        }

        [HarmonyPatch(typeof(WinCanvas), "ButtonClick")]
        public static class WinCanvasPatch
        {
            [HarmonyPrefix]
            static void Prefix()
            {
                int wave = SingletonMonoBehaviour<ChessDataManager>.Instance.CurrentWave;
                int stage = (wave - 1) / 5 + 1;
                int game = (wave - 1) % 5 + 1;
                
                string locName = $"Stage {stage} Game {game}";
                Log.LogInfo($"Level completed: {locName}. Sending check...");
                AP.SendLocation(locName);
            }
        }

        [HarmonyPatch(typeof(FinalBoss_Behaviour), "Die")]
        public static class FinalBossPatch
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                Log.LogInfo("Final Boss defeated! Completing goal...");
                AP.MarkGoalAsReached();
            }
        }

        [HarmonyPatch(typeof(GambitToBuy), "Initialize")]
        public static class ShopVisualPatch
        {
            [HarmonyPrefix]
            static void Prefix(GambitToBuy __instance, SO_Gambit gambit, ref bool locked)
            {
                // Lock the gambit if it hasn't been received from AP yet
                if (!AP.IsGambitUnlocked(gambit.GambitName))
                {
                    locked = true;
                }
            }

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
                        
                        // Ensure the AP Check is never locked
                        AccessTools.Field(typeof(GambitToBuy), "m_LockButton").GetValue(__instance).GetType().GetMethod("Initialize").Invoke(
                            AccessTools.Field(typeof(GambitToBuy), "m_LockButton").GetValue(__instance),
                            new object[] { "ap_check", false }
                        );
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PieceToBuyButton), "Initialize")]
        public static class PieceShopPatch
        {
            [HarmonyPrefix]
            static void Prefix(PieceToBuyButton __instance, PieceType pieceType, ref bool locked)
            {
                // Lock the piece if it hasn't been received from AP yet
                if (!AP.IsPieceUnlocked(pieceType))
                {
                    locked = true;
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
