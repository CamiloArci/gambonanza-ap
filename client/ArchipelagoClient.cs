using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Blukulele.CHE;
using Blukulele.Core;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GambonanzaAP
{
    public class ArchipelagoClient
    {
        private ArchipelagoSession session;
        public HashSet<string> ReceivedItems = new HashSet<string>();
        public List<string> ItemQueue = new List<string>();
        public bool IsConnected => session != null && session.Socket.Connected;

        public bool Connect(string server, string user, string password = null)
        {
            try {
                Plugin.Log.LogInfo($"Attempting to login as '{user}' on game 'Gambonanza'...");
                session = ArchipelagoSessionFactory.CreateSession(server);
                var version = new System.Version(1, 0, 0);
                var result = session.TryConnectAndLogin("Gambonanza", user, ItemsHandlingFlags.AllItems, version: version, password: password);

                if (result is LoginSuccessful success)
                {
                    session.Items.ItemReceived += OnItemReceived;
                    foreach (var item in session.Items.AllItemsReceived)
                    {
                        string name = session.Items.GetItemName(item.ItemId);
                        ReceivedItems.Add(name);
                        ItemQueue.Add(name);
                    }
                    Plugin.Log.LogInfo($"Successfully connected as {user}!");
                    return true;
                }
                else if (result is LoginFailure failure)
                {
                    string errors = string.Join(", ", failure.Errors);
                    Plugin.Log.LogError($"Login Failed: {errors}");
                    return false;
                }
                return false;
            } catch (Exception e) {
                Plugin.Log.LogError($"Critical Error connecting: {e.Message}");
                return false;
            }
        }

        private void OnItemReceived(ReceivedItemsHelper helper)
        {
            var item = helper.DequeueItem();
            string itemName = session.Items.GetItemName(item.ItemId);
            ReceivedItems.Add(itemName);
            ItemQueue.Add(itemName);
            Plugin.Log.LogInfo($"Received: {itemName}");
        }

        public void SendLocation(string locationName)
        {
            if (!IsConnected) return;
            long locationId = session.Locations.GetLocationIdFromName("Gambonanza", locationName);
            session.Locations.CompleteLocationChecks(locationId);
        }

        public int GetNextAvailableCheckId()
        {
            if (!IsConnected) return -1;
            
            // Check IDs 1 to 31
            for (int i = 1; i <= 31; i++)
            {
                string locName = $"Shop Check {i}";
                long id = session.Locations.GetLocationIdFromName("Gambonanza", locName);
                if (!session.Locations.AllLocationsChecked.Contains(id))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Update()
        {
            if (ItemQueue.Count > 0 && GameManager.Instance.CurrentState == State.INGAME)
            {
                string item = ItemQueue[0];
                ItemQueue.RemoveAt(0);
                ApplyItem(item);
            }
        }

        private static readonly Dictionary<string, string> ItemToInternalName = new Dictionary<string, string>
        {
            { "Squirrel's Gambit", "squirrel_name" },
            { "Ant's Gambit", "ant_name" },
            { "Bee's Gambit", "bee_name" },
            { "Slime's Gambit", "slime_name" },
            { "Butterfly's Gambit", "butterfly_name" },
            { "Spider's Gambit", "spider_name" },
            { "Cobra's Gambit", "cobra_name" },
            { "Scorpion's Gambit", "scorpion_name" },
            { "Falcon's Gambit", "falcon_name" },
            { "Wolf's Gambit", "wolf_name" },
            { "Bear's Gambit", "bear_name" },
            { "Lion's Gambit", "lion_name" },
            { "Elephant's Gambit", "elephant_name" },
            { "Dragon's Gambit", "dragon_name" },
            { "Phoenix's Gambit", "phoenix_name" },
            { "Unicorn's Gambit", "unicorn_name" },
            { "Kraken's Gambit", "kraken_name" },
            { "Hydra's Gambit", "hydra_name" },
            { "Chimera's Gambit", "chimera_name" },
            { "Griffon's Gambit", "griffon_name" },
            { "Pegasus's Gambit", "pegasus_name" },
            { "Basilisk's Gambit", "basilisk_name" },
            { "Minotaur's Gambit", "minotaur_name" },
            { "Centaur's Gambit", "centaur_name" },
            { "Cyclops's Gambit", "cyclops_name" },
            { "Titan's Gambit", "titan_name" },
            { "Greed Demon's Gambit", "demon_name" },
            { "Bribe's Gambit", "bribe_name" }
        };

        private void ApplyItem(string itemName)
        {
            Plugin.Log.LogInfo($"Applying item: {itemName}");
            
            // Handle Board Upgrades
            if (itemName == "Board Upgrade")
            {
                int upgrades = ReceivedItems.Count(x => x == "Board Upgrade");
                ChessDataManager.Instance.MaxPieceOnBoard = ChessDataManager.Instance.MaxPieceOnBoardAtStart + upgrades;
                Plugin.Log.LogInfo($"Board Upgrade applied. New limit: {ChessDataManager.Instance.MaxPieceOnBoard}");
                return;
            }

            // Handle Pieces (Piece names are just the name + " Piece")
            if (itemName.EndsWith(" Piece"))
            {
                string pieceName = itemName.Replace(" Piece", "").ToUpper();
                if (Enum.TryParse<PieceType>(pieceName, out PieceType pType))
                {
                    Plugin.Log.LogInfo($"Granting Piece: {pType}");
                    SingletonMonoBehaviour<StockManager>.Instance.AddPiece(pType, Vector3.zero, false, false, false, !SingletonMonoBehaviour<StockManager>.Instance.RoomAvailable());
                    return;
                }
            }
            
            // Handle Gambits using the map
            if (ItemToInternalName.TryGetValue(itemName, out string internalName))
            {
                var gambitInfo = SingletonMonoBehaviour<GambitLibrary>.Instance.GambitsInfo.FirstOrDefault(x => x.GambitName.Equals(internalName, StringComparison.OrdinalIgnoreCase));
                if (gambitInfo != null)
                {
                    Plugin.Log.LogInfo($"Granting Gambit: {internalName}");
                    var gambitPlace = SingletonMonoBehaviour<GambitManager>.Instance.GetGambitPlace();
                    if (gambitPlace != null)
                    {
                        var prefab = SingletonMonoBehaviour<GambitLibrary>.Instance.Gambits[SingletonMonoBehaviour<GambitLibrary>.Instance.GambitsInfo.IndexOf(gambitInfo)];
                        var gambitObj = UnityEngine.Object.Instantiate<GambitBehaviour>(prefab, gambitPlace.GambitParent);
                        gambitPlace.CurrentGambit = gambitObj;
                        
                        // Necessary for internal game logic to recognize the new gambit
                        SingletonMonoBehaviour<SaveManager>.Instance.SaveProgressionShop();
                    }
                    return;
                }
            }

            Plugin.Log.LogWarning($"Item {itemName} could not be mapped to any game action.");
        }
    }
}
