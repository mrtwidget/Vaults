using System;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using UnityEngine;

namespace NEXIS.Vaults
{
    public class CommandVault : IRocketCommand
    {
        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Player; }
        }
        public bool AllowFromConsole
        {
            get { return false; }
        }

        public string Name
        {
            get { return "vault"; }
        }

        public string Help
        {
            get { return "Save items from your inventory to your own personal Vault."; }
        }

        public List<string> Aliases
        {
            get { return new List<string>() { }; }
        }

        public string Syntax
        {
            get { return "/vault {save|load|help} [item...]"; }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "nexis.vault" };
            }
        }

        public void Execute(IRocketPlayer caller, params string[] param)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;

            if (Vault.Instance.Configuration.Instance.VaultsEnabled)
            {
                if (param.Length > 0)
                {
                    if (param.Length == 1 && Vault.Instance.Configuration.Instance.VaultsSaveEntireInventory)
                    {
                        switch (param[0])
                        {
                            case "save":
                                // save player vault to database
                                Vault.Instance.Database.SavePlayerInventory(player);
                                break;
                            case "open":
                            case "load":
                                // open player vault from database
                                Vault.Instance.Database.OpenPlayerInventory(player);
                                break;
                            case "help":
                                UnturnedChat.Say(caller, Help, Color.white);
                                UnturnedChat.Say(caller, Syntax, Color.white);
                                break;
                            default:
                                // invalid action
                                UnturnedChat.Say(caller, Vault.Instance.Translations.Instance.Translate("vault_action_invalid"), Color.red);
                                break;
                        }
                    }
                    else if (param.Length == 2 && !Vault.Instance.Configuration.Instance.VaultsSaveEntireInventory)
                    {
                        ushort itemId;
                        if (ushort.TryParse(param[1], out itemId))
                        {
                            switch (param[0])
                            {
                                case "save":
                                    // save player vault to database
                                    Vault.Instance.Database.SavePlayerInventory(player, itemId);
                                    break;
                                case "open":
                                case "load":
                                    // open player vault from database
                                    Vault.Instance.Database.OpenPlayerInventory(player, itemId);
                                    break;
                                case "help":
                                    UnturnedChat.Say(caller, Help, Color.white);
                                    UnturnedChat.Say(caller, Syntax, Color.white);
                                    break;
                                default:
                                    // invalid action
                                    UnturnedChat.Say(caller, Vault.Instance.Translations.Instance.Translate("vault_action_invalid"), Color.red);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "Incorrect Syntax! Use:" + Syntax, Color.white);
                    }
                }
                else
                {
                    // user typed /vault only
                    UnturnedChat.Say(caller, Help, Color.white);
                }
            }
            else
            {
                // vault disabled in configuration
                UnturnedChat.Say(caller, Vault.Instance.Translations.Instance.Translate("vault_disabled"), Color.yellow);
            }
        }
    }
}