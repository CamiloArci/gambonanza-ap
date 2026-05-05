using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GambonanzaAP
{
    public class ArchipelagoClient
    {
        private ArchipelagoSession session;
        public HashSet<string> ReceivedItems = new HashSet<string>();
        public bool IsConnected => session != null && session.Socket.Connected;

        public void Connect(string server, string user, string password = null)
        {
            session = ArchipelagoSessionFactory.CreateSession(server);
            
            var result = session.TryConnectAndLogin("Gambonanza", user, ItemsHandlingFlags.AllItems, password: password);

            if (result is LoginSuccessful success)
            {
                session.Items.ItemReceived += OnItemReceived;
                // Sync current items
                foreach (var item in session.Items.AllItemsReceived)
                {
                    ReceivedItems.Add(session.Items.GetItemName(item.Item));
                }
                Plugin.Log.LogInfo("Connected to Archipelago!");
            }
            else
            {
                Plugin.Log.LogError("Connection failed!");
            }
        }

        private void OnItemReceived(ReceivedItemsHelper helper)
        {
            var item = helper.DequeueItem();
            string itemName = session.Items.GetItemName(item.Item);
            ReceivedItems.Add(itemName);
            Plugin.Log.LogInfo($"Received: {itemName}");
        }

        public void SendLocation(string locationName)
        {
            if (!IsConnected) return;
            long locationId = session.Locations.GetLocationIdFromName("Gambonanza", locationName);
            session.Locations.CompleteLocationChecks(locationId);
        }
    }
}
