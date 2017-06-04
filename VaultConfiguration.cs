using Rocket.API;

namespace NEXIS.Vaults
{
    public class VaultConfiguration : IRocketPluginConfiguration
    {
        public bool VaultsEnabled;
        public bool VaultsSaveEntireInventory;
        public bool DeleteInventoryItemsOnSave;
        public bool DeleteDatabaseVaultOnOpen;
        public int TotalAllowedVaults;
        public bool ShareVaultsAcrossServers;

        public bool Debug;

        public string DatabaseHost;
        public string DatabaseUser;
        public string DatabasePass;
        public string DatabaseName;
        public int DatabasePort;
        public string DatabaseTable;

        public void LoadDefaults()
        {
            // Configuration Settings
            VaultsEnabled = true;
            VaultsSaveEntireInventory = true;
            DeleteInventoryItemsOnSave = true;
            DeleteDatabaseVaultOnOpen = true;
            TotalAllowedVaults = 3;
            ShareVaultsAcrossServers = false;

            // Debug Mode
            Debug = false;
            
            // Database Settings
            DatabaseHost = "localhost";
            DatabaseUser = "unturned";
            DatabasePass = "password";
            DatabaseName = "unturned";
            DatabasePort = 3306;
            DatabaseTable = "vaults";
        }
    }
}