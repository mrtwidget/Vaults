using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using UnityEngine;

namespace NEXIS.Vaults
{
    public class CommandVaults : IRocketCommand
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
            get { return "vaults"; }
        }

        public string Help
        {
            get { return "View your current Vault items."; }
        }

        public List<string> Aliases
        {
            get { return new List<string>() { }; }
        }

        public string Syntax
        {
            get { return "/vaults <help>"; }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "nexis.vaults" };
            }
        }

        public void Execute(IRocketPlayer caller, params string[] param)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;

            if (Vault.Instance.Configuration.Instance.VaultsEnabled)
            {
                if (param.Length > 0)
                {
                    switch (param[0])
                    {
                        case "help":
                            UnturnedChat.Say(caller, Help, Color.white);
                            UnturnedChat.Say(caller, Syntax, Color.white);
                            break;
                        default:
                            UnturnedChat.Say(caller, Vault.Instance.Translations.Instance.Translate("vaults_invalid_action"), Color.red);
                            break;
                    }
                }
                else
                {
                    // list vaults
                    Vault.Instance.Database.ListVaults(player);
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