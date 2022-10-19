## Description:
Creates and manages a separate environment for playing PC games on a TV, to have a "console like" experience.

## Requirements:
- Visual Studio 2022
- .NET Framework 4.7.2
- DS4Windows
- Playnite

## Installation:

- Open the solution in Visual Studio
- Set build configuration to Release and build the solution
- Copy build output folder's content (`./bin/Release`) to a desired installation folder (`[InstallationFolder]`)
- Customize the service configuration for your needs (`[InstallationFolder]/config.json`)
- Setup `BackgroundService` (`[InstallationFolder]/BackgroundService.exe`) to start automatically during Windows startup (You have to do it manully, there isn't any built in feature for this.)
- Setup playnite plugin:
  - Open Playnite
  - Add `PlaynitePlugin` as an extension: Go to Playnite settings, `For developers` section and add the `PlaynitePlugin`'s folder from the installation path (`[InstallationFolder]/PlaynitePlugin`) to the `External extensions` list.
  - **Important:** Close Playnite completely (should not remain open in the background either). Playnite is managed by `BackgroundService`. If it remains open before switching to TV environment, Playnite will not work properly. 
- Start `BackgroundService` (`[InstallationFolder]/BackgroundService.exe`)

## Hotkeys:
Hotkeys can be changed from source code (See: `./BackgroundService/Source/InternalSettings.cs`)

- **Switch environment** (`Alt + NumPad 0`): Switches between TV and PC environments.
- **Reset current environment** (`Alt + NumPad 1`): Restarts third-party applications (e.g.: Playnite, DS4Windows). Closes game stores and resets environment settings (e.g. desktop, display, cursor visibility). Useful when a third-party app is bugged or enters into an invalid state.
- **Toggle cursor visibility** (`Alt + NumPad 8`): Toggles cursor's global visibility. This hotkey is usable to show the cursor and be able to use mouse input in TV environment (in this environment cursor is invisible by default).
- **Toggle console visibility** (`Alt + NumPad 8`): Toggles `BackgroundProcess`'s console window's visibility. By default the window is hidden. Hotkey can be used to show the console with debug logs and events.

## Components:

### BackgroundService:
Headless background service for controlling TV and PC environments. Users can change, interact with the environments through this service using different hotkeys (See `Hotkeys`).

### Core:
A library, containing common source code shared amongst project components.

### PlaynitePlugin:
A Playnite extension, notifies `BackgroundService` when a Playnite event is triggered (e.g. Game is starting)
