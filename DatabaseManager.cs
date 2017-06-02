using Logger = Rocket.Core.Logging.Logger;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Skills;
using SDG.Unturned;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using System.Linq;
using System;

namespace NEXIS.Vaults
{
    public class DatabaseManager
    {
        public DatabaseManager()
        {
            new I18N.West.CP1250();
            CheckSchema();
        }

        public void CheckSchema()
        {
            try
            {
                MySqlConnection MySQLConnection = CreateConnection();
                MySqlCommand MySQLCommand = MySQLConnection.CreateCommand();

                MySQLCommand.CommandText = "SHOW TABLES LIKE '" + Vault.Instance.Configuration.Instance.DatabaseTable + "'";
                MySQLConnection.Open();

                object result = MySQLCommand.ExecuteScalar();

                if (result == null)
                {
                    MySQLCommand.CommandText = "CREATE TABLE " + Vault.Instance.Configuration.Instance.DatabaseTable +
                    "(id INT(8) NOT NULL AUTO_INCREMENT," +
                    "steam_id VARCHAR(50) NOT NULL," +
                    "inventory TEXT NULL," +
                    "timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP," +
                    "PRIMARY KEY(id));";

                    MySQLCommand.ExecuteNonQuery();
                }
                MySQLConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private MySqlConnection CreateConnection()
        {
            MySqlConnection MySQLConnection = null;

            try
            {
                if (Vault.Instance.Configuration.Instance.DatabasePort == 0)
                {
                    Vault.Instance.Configuration.Instance.DatabasePort = 3306;
                }

                MySQLConnection = new MySqlConnection(string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", new object[] {
                    Vault.Instance.Configuration.Instance.DatabaseHost,
                    Vault.Instance.Configuration.Instance.DatabaseName,
                    Vault.Instance.Configuration.Instance.DatabaseUser,
                    Vault.Instance.Configuration.Instance.DatabasePass,
                    Vault.Instance.Configuration.Instance.DatabasePort
                }));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return MySQLConnection;
        }

        public void ListVaults(UnturnedPlayer player)
        {
            try
            {
                MySqlConnection MySQLConnection = CreateConnection();
                MySqlCommand MySQLCommand = MySQLConnection.CreateCommand();
                MySQLConnection.Open();

                MySQLCommand.CommandText = "SELECT COUNT(*) FROM " + Vault.Instance.Configuration.Instance.DatabaseTable + " WHERE steam_id = '" + player.CSteamID.ToString() + "'";
                int vaultCount = Convert.ToInt32(MySQLCommand.ExecuteScalar());

                MySQLCommand.CommandText = "SELECT * FROM " + Vault.Instance.Configuration.Instance.DatabaseTable + " WHERE steam_id = '" + player.CSteamID.ToString() + "'";
                MySqlDataReader vaults = MySQLCommand.ExecuteReader();

                UnturnedChat.Say(player, "Vaults Available: " + vaultCount + " / " + Vault.Instance.Configuration.Instance.TotalAllowedVaults, Color.white);

                while (vaults.Read())
                {
                    if (vaultCount > 0)
                    {
                        UnturnedChat.Say(player, "Vault: " + vaults["inventory"], Color.white);                        
                    }
                    else
                    {
                        UnturnedChat.Say(player, Vault.Instance.Translations.Instance.Translate("vault_saved_noitems"), Color.white);
                    }
                }
                vaults.Close();
                MySQLConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        /**
         * SAVE PLAYER INVENTORY ITEM(S)
         * 
         * This function saves a player's inventory or individual item (depending on configuration settings)
         * to the server database that is configured
         * @param UnturnedPlayer player Player data
         * @param int itemId Item ID to save to the database; if equals 0 save entire inventory
         */
        public void SavePlayerInventory(UnturnedPlayer player, ushort itemId = 0)
        {
            try
            {
                MySqlConnection MySQLConnection = CreateConnection();
                MySqlCommand MySQLCommand = MySQLConnection.CreateCommand();
                MySQLConnection.Open();

                if (Vault.Instance.Configuration.Instance.VaultsSaveEntireInventory)
                {
                    // check if player vault already exists
                    MySQLCommand.CommandText = "SELECT * FROM " + Vault.Instance.Configuration.Instance.DatabaseTable + " WHERE steam_id = '" + player.CSteamID.ToString() + "'";
                    object vaultExists = MySQLCommand.ExecuteScalar();

                    // check if vault already exists
                    if (vaultExists == null)
                    {
                        List<string> InventoryItemsFound = new List<string>();
                        string InventoryDatabaseString = "";

                        // save and remove items
                        try
                        {
                            // get clothing
                            InventoryItemsFound.Add(player.Player.clothing.shirt.ToString());
                            InventoryItemsFound.Add(player.Player.clothing.pants.ToString());
                            InventoryItemsFound.Add(player.Player.clothing.mask.ToString());
                            InventoryItemsFound.Add(player.Player.clothing.hat.ToString());
                            InventoryItemsFound.Add(player.Player.clothing.glasses.ToString());
                            InventoryItemsFound.Add(player.Player.clothing.vest.ToString());
                            InventoryItemsFound.Add(player.Player.clothing.backpack.ToString());

                            // get items
                            foreach (var i in player.Inventory.items)
                            {
                                if (i == null) continue;
                                for (byte w = 0; w < i.width; w++)
                                {
                                    for (byte h = 0; h < i.height; h++)
                                    {
                                        try
                                        {
                                            byte index = i.getIndex(w, h);
                                            if (index == 255) continue;
                                            // add item found to list
                                            ItemJar invItem = player.Inventory.getItem(i.page, index);
                                            InventoryItemsFound.Add(invItem.item.id.ToString());
                                        }
                                        catch { }
                                    }
                                }
                            }

                            // create mysql string from array items separated by commas
                            InventoryDatabaseString = string.Join(",", InventoryItemsFound.ToArray());

                            if (InventoryItemsFound.Capacity > 0)
                            {
                                // update database
                                MySQLCommand.CommandText = "INSERT INTO " + Vault.Instance.Configuration.Instance.DatabaseTable + " (steam_id,inventory) VALUES ('" + player.CSteamID.ToString() + "','" + InventoryDatabaseString + "')";
                                MySQLCommand.ExecuteNonQuery();

                                // delete all player inventory items, in enabled in configuration
                                if (Vault.Instance.Configuration.Instance.DeleteInventoryItemsOnSave)
                                {
                                    ClearInventory(player);
                                }
                            }
                            else
                            {
                                // no items to save
                                UnturnedChat.Say(player, Vault.Instance.Translations.Instance.Translate("vault_saved_noitems"), Color.red);
                                return;
                            }
                            
                        }
                        catch (Exception ex)
                        {
                            Logger.LogException(ex);
                        }

                        // debug
                        if (Vault.Instance.Configuration.Instance.Debug) { Logger.Log(player.CharacterName + " saved Vault items: " + InventoryDatabaseString, ConsoleColor.Yellow); }

                        // vault saved successfully 
                        UnturnedChat.Say(player, Vault.Instance.Translations.Instance.Translate("vault_saved_inventory"), Color.green);
                    }
                    else
                    {
                        // vault already exists
                        UnturnedChat.Say(player, Vault.Instance.Translations.Instance.Translate("vault_full"), Color.red);
                    }
                }
                else
                /**
                 * SAVE INDIVIDUAL ITEM */
                {
                    // check if item to vault exists in player inventory
                    if (player.Inventory.has(itemId) != null)
                    {
                        // check available vaults
                        MySQLCommand.CommandText = "SELECT COUNT(*) FROM " + Vault.Instance.Configuration.Instance.DatabaseTable + " WHERE steam_id = '" + player.CSteamID.ToString() + "'";
                        int vaultCount = Convert.ToInt32(MySQLCommand.ExecuteScalar());

                        // check if player has used all available vaults
                        if (vaultCount >= Vault.Instance.Configuration.Instance.TotalAllowedVaults)
                        {
                            // all vaults are full
                            UnturnedChat.Say(player, Vault.Instance.Translations.Instance.Translate("vault_full"), Color.red);
                        }
                        else
                        {
                            // vault available; save the item 
                            // remove item from player inventory
                            MySQLCommand.CommandText = "INSERT INTO " + Vault.Instance.Configuration.Instance.DatabaseTable + " (steam_id,inventory) VALUES ('" + player.CSteamID.ToString() + "','" + itemId.ToString() + "')";
                            MySQLCommand.ExecuteNonQuery();

                            // remove item from player inventory, if enabled in configuration
                            if (Vault.Instance.Configuration.Instance.DeleteInventoryItemsOnSave)
                            {
                                RemoveInventoryItem(player, itemId);
                            }

                            // DEBUG
                            if (Vault.Instance.Configuration.Instance.Debug) { Logger.Log(player.CharacterName + " saved Vault item: " + itemId, ConsoleColor.Yellow); }

                            UnturnedChat.Say(player, Vault.Instance.Translations.Instance.Translate("vault_saved"), Color.green);
                        }
                    }
                    else
                    {
                        // player does not have that item
                        UnturnedChat.Say(player, Vault.Instance.Translations.Instance.Translate("vault_invalid_item"), Color.red);
                    }
                }
                MySQLConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        /**
         * OPEN PLAYER INVENTORY ITEM(S)
         * 
         * This function loads a player's inventory or individual item (depending on configuration settings)
         * from the server database that is configured
         * @param UnturnedPlayer player Player data
         * @param int itemId Item ID to load from database; if equals 0 load entire inventory
         */
        public void OpenPlayerInventory(UnturnedPlayer player, int vault = 0)
        {
            try
            {
                MySqlConnection MySQLConnection = CreateConnection();
                MySqlCommand MySQLCommand = MySQLConnection.CreateCommand();
                MySQLConnection.Open();

                MySQLCommand.CommandText = "SELECT steam_id FROM " + Vault.Instance.Configuration.Instance.DatabaseTable + " WHERE steam_id = '" + player.CSteamID.ToString() + "'";
                object result = MySQLCommand.ExecuteScalar();                

                if (result != null)
                {
                    // query player vault inventory items
                    MySQLCommand.CommandText = "SELECT inventory FROM " + Vault.Instance.Configuration.Instance.DatabaseTable + " WHERE steam_id = '" + player.CSteamID.ToString() + "'";
                    MySqlDataReader inventory = MySQLCommand.ExecuteReader();

                    if (inventory.Read())
                    {
                        string[] ItemIDs = inventory["inventory"].ToString().Split(',');

                        // add each item to player inventory
                        foreach (string id in ItemIDs)
                        {
                            Item item = new Item(ushort.Parse(id), true);
                            player.Inventory.forceAddItem(item,true);
                        }
                    }
                    inventory.Close();

                    // delete vault from database, if enabled in configuration
                    if (Vault.Instance.Configuration.Instance.DeleteDatabaseVaultOnOpen)
                    {
                        MySQLCommand.CommandText = "DELETE FROM " + Vault.Instance.Configuration.Instance.DatabaseTable + " WHERE steam_id = '" + player.CSteamID.ToString() + "'";
                        MySQLCommand.ExecuteNonQuery();
                    }

                    // DEBUG
                    if (Vault.Instance.Configuration.Instance.Debug) { Logger.Log(player.CharacterName + " opened Vault!", ConsoleColor.Yellow); }

                    // vault opened successfully 
                    UnturnedChat.Say(player, Vault.Instance.Translations.Instance.Translate("vault_opened"), Color.green);
                }
                else
                {
                    // no vault exists
                    UnturnedChat.Say(player, Vault.Instance.Translations.Instance.Translate("vault_empty"), Color.red);
                }
                MySQLConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        /**
         *  REMOVE ITEM FROM PLAYER INVENTORY
         *  
         *  This function removes an item from a player inventory, for use when saving 
         *  items to the database.
         *  @param UnturnedPlayer player Player data
         *  @param ushort itemId Item ID
         */
        public void RemoveInventoryItem(UnturnedPlayer player, ushort itemId)
        {
            try
            {
                for (byte page = 0; page < 8; page++)
                {
                    var count = player.Inventory.getItemCount(page);

                    for (byte index = 0; index < count; index++)
                    {
                        if (player.Inventory.getItem(page, index).item.id == itemId)
                        {
                            player.Inventory.removeItem(page, index);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        /**
         *  REMOVE ALL ITEMS FROM INVENTORY
         *  
         *  This function removes all items from a player inventory, for use when saving 
         *  items to the database.
         *  @param UnturnedPlayer player Player data
         */
        public void ClearInventory(UnturnedPlayer player)
        {            
            try
            {
                player.Player.equipment.dequip();

                foreach (var i in player.Inventory.items)
                {
                    if (i == null) continue;
                    for (byte w = 0; w < i.width; w++)
                    {
                        for (byte h = 0; h < i.height; h++)
                        {
                            try
                            {
                                byte index = i.getIndex(w, h);
                                if (index == 255) continue;
                                i.removeItem(index);
                            }
                            catch { }
                        }
                    }
                }
                                
                // glasses
                player.Player.clothing.askWearGlasses(0, 0, new byte[0], true);
                for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                {
                    player.Player.inventory.removeItem(2, 0);
                }
                // hat
                player.Player.clothing.askWearHat(0, 0, new byte[0], true);
                for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                {
                    player.Player.inventory.removeItem(2, 0);
                }
                // mask
                player.Player.clothing.askWearMask(0, 0, new byte[0], true);
                for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                {
                    player.Player.inventory.removeItem(2, 0);
                }
                // pants
                player.Player.clothing.askWearPants(0, 0, new byte[0], true);
                for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                {
                    player.Player.inventory.removeItem(2, 0);
                }
                // shirt
                player.Player.clothing.askWearShirt(0, 0, new byte[0], true);
                for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                {
                    player.Player.inventory.removeItem(2, 0);
                }
                // vest
                player.Player.clothing.askWearVest(0, 0, new byte[0], true);
                for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                {
                    player.Player.inventory.removeItem(2, 0);
                }
                // backpack
                player.Player.clothing.askWearBackpack(0, 0, new byte[0], true);
                for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                {
                    player.Player.inventory.removeItem(2, 0);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }
}