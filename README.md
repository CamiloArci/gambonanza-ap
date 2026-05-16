# Gambonanza Archipelago Mod

This mod integrates the roguelike **Gambonanza** into the Archipelago MultiWorld system.

## For Developers & Testing

### 1. Setup the World
- Zip the `gambonanza/` directory and rename the result to `gambonanza.apworld`.
- Place the `.apworld` file in your Archipelago `custom_worlds/` directory (or directly into the `lib/worlds/` directory).

### 2. Generate a Seed
- Create a `Player.yaml` (or use the one in the Archipelago `players` folder) with:
  ```yaml
  name: YourName
  game: Gambonanza
  Gambonanza: {}
  ```
- Generate a seed using the Archipelago Launcher or the website.
- Host the server and copy the address (e.g., `archipelago.gg:38281`).

### 3. Build the Client
- Run `dotnet build` in the `client/` directory.
- Go to `client/bin/Debug/net472/`.
- Copy the following files to your game's `BepInEx/plugins/` folder:
  - `GambonanzaAP.dll`
  - `Archipelago.MultiClient.Net.dll`
  - `Newtonsoft.Json.dll`

### 4. Configuration
- Run the game once to generate the config file.
- Go to `BepInEx/config/` and open `com.[my_name].gambonanza.ap.cfg`.
- Set your `Server` and `SlotName`.
- **Note:** The game will automatically close on startup if the connection fails. Check `BepInEx/LogOutput.log` for errors.

### 5. In-Game Controls
- **F8**: Triggers a manual reconnection attempt to the Archipelago server.
- **Shop**: The first slot in the shop is supposed to be replaced with an "Archipelago Check". Buying it sends a location check to the MultiWorld. But it's not working 100% of the times.