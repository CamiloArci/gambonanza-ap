using BepInEx;
using BepInEx.Logging;
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

        private void Awake()
        {
            Log = Logger;
            AP = new ArchipelagoClient();
            
            Harmony harmony = new Harmony("com.arci.gambonanza.ap");
            harmony.PatchAll();

            Log.LogInfo("Gambonanza Archipelago Loaded!");
        }

        [HarmonyPatch(typeof(ChessDataManager), "Behave")]
        public static class MatchWinPatch
        {
            [HarmonyPostfix]
            static void Postfix(State state)
            {
                if (state == State.WIN)
                {
                    int stage = (ChessDataManager.Instance.CurrentWave - 1) / 5 + 1;
                    int game = (ChessDataManager.Instance.CurrentWave - 1) % 5 + 1;
                    
                    string locationName = $"Match {stage}-{game}";
                    Log.LogInfo($"Victory detected! Sending check for {locationName}");
                    AP.SendLocation(locationName);

                    // Check if it was a boss (game 5 of the stage)
                    if (game == 5)
                    {
                        AP.SendLocation($"Boss {stage}");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(FinalBoss_Behaviour), "Die")]
        public static class FinalBossPatch
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                Log.LogInfo("Final Boss Defeated! Sending Game Clear.");
                AP.SendLocation("Game Clear");
            }
        }

        // Logic for Board Upgrades
        [HarmonyPatch(typeof(ChessDataManager), "LoadData")]
        public static class BoardLimitPatch
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                // Set the base piece limit plus the number of upgrades received
                int upgrades = AP.ReceivedItems.Count(x => x.StartsWith("Board Upgrade"));
                ChessDataManager.Instance.MaxPieceOnBoard = ChessDataManager.Instance.MaxPieceOnBoardAtStart + upgrades;
            }
        }
    }
}
