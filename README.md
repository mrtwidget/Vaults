# Vaults

*Allows players to save and retrieve their entire inventory - or individual items - to a server-side MySQL database.*

This plugin was developed using [Rocket Mod](https://rocketmod.net/) libraries for the [Steam](http://store.steampowered.com/) game [Unturned](http://store.steampowered.com/app/304930/).

![picture alt](http://nexisrealms.com/images/hosted/vaults.jpg "Vaults v1.0 Logo")

**Current Release :**
- Vaults v1.0

**How to Install:**

***Vaults Rocket Plugin***
1. Compile this project
2. Copy the compiled `Vaults.dll` to your Rocket Mod plugin directory
3. Start/stop your server to generate `./plugins/Vaults/Vaults.configuration.xml`
4. Edit `Vaults.configuration.xml` and configure MySQL database settings
5. Add a Rocket Mod permission for the /vaults & /vault command by adding it to your `Permissions.config.xml`
    - *Example*: 
        - `<Permission Cooldown="0">vaults</Permission>`
        - `<Permission Cooldown="0">vault</Permission>`
6. Start Unturned Server

---

**Resources:**
- Nexis Realms: [nexisrealms.com](http://nexisrealms.com/)

---

*author: Nexis (steam:iamtwidget) <[nexis@nexisrealms.com](mailto:nexis@nexisrealms.com)>*