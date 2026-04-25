# VRCSDK Patching Notes

## How VRCSDK is meant to work

The Unity EditorPref key `VRC_installedClientPath` is meant to hold a string of a absolute path that points to VRChat.exe.

This is always set after VRCSDK initializes; if it was not already set, it pulls the value from some key in the registry, which was probably set beforehand by VRChat.exe or launch.exe.

In the settings page, the user has an option to change `VRC_installedClientPath`. I have no idea what the practical use-case is for doing this. Clicking "Revert to Default" resets it by reading from the registry again.

The VRCSDK.World "Build and test" button will execute VRChat.exe directly, giving it many arguments that launch the game in offline mode with a `file:///` URI to the built world AssetBundle ending in `.vrcw`. It only falls back to "executing" the equivalent `vrchat://` link directly if VRChat.exe hasn't been found -- that is, `VRC_installedClientPath` is unset because the registry key was missing, or VRChat was recently moved to a different Steam library.

The VRCSDK fetches avatars in the Content Manager window, by searching directly inside `Environment.SpecialFolder.LocalApplicationData`. This is `AppData/LocalLow` on Windows and `~/.local/share/VRChat` on Linux, but there's no practical reason for the latter to exist. This dir has even been known to interfere with VRCFaceTracking. The AssetBundle can technically work no matter where you save it, but it's best if it's saved to the LocalLow inside VRChat's Proton prefix.

## Alternatives

Prior to this doc, we've been manually launching Proton, and the user must select the Proton version by way of searching for the `proton` Python script file to use as an entrypoint. It also did not locate the VRChat game directory, so moving it to a non-default Steam library would break it.

Now, we use `SteamLocator`, `VrcLocator`, and `ProtonLocator` to find everything, reconciling it with Unity preferences.

There is still the option to ask Steam to launch it, with `steam -applaunch 438100` or something, but I don't know if it would work. We want it to use offline mode, allow multiple instances, and not interfere with any instance running in online mode. The URI handler below has the same effect as launching it from Steam. The `vrchat://` URIs can work on Linux, but they need a .desktop file.

## How we patch VRCSDK

This is an exhaustive list.

- Fixes the VRCSDK initialization so it can correctly find VRChat.exe.
  - We replace the `LoadRegistryVRCInstallPath` method to call our `VrcLocator` instead.
- Prevents the pointless creation (lol) and use of `~/.local/share/VRChat`, instead saving test worlds and avatars to the Proton prefix.
  - We replace the method that looks for `LocalLow` to do Not that.
- Fixes the Content Manager tab so it can show your test avatars.
- Fixes the Build and Test button.
  - We replace the entire `VRCWorldAssetExporter.RunWorldTestDesktop` method to:
    - Translate the path of the saved AssetBundle to a winepath relative to `Z:/`.
    - Call Proton instead, with all `STEAM_COMPAT_` env vars set.
    - We do not launch it in Steam Linux Runtime, and I hope it doesn't come to that. But it is possible, we'd just read `toolmanifest.vdf` and wrap the command further.
- Adds some UI in VRCSDK Settings tab to select a different Proton to use instead of what's set in Steam.

## Ideas/Roadmap

- [ ] Warn if `~/.local/share/VRChat` exists, because the latest VRCFaceTracking.Avalonia release (currently v1.1.1.0) will break if it sees this.
- [ ] Run `xdg-mime query` and offer to set up [the URI handler](#vrchat-uri-handler).
- [ ] Patch UdonSharp exception watcher.
- [ ] MIT license button in Tools menu.

## Snippets

### VRChat Offline Mode

Example command I use to run VRChat in offline mode, supporting multiple clients all loaded into an instance of a world AssetBundle stored locally:

```bash
STEAM_COMPAT_DATA_PATH=/mnt/steam/steamapps/compatdata/438100/ STEAM_COMPAT_CLIENT_INSTALL_PATH=$HOME/.local/share/Steam/ STEAM_COMPAT_INSTALL_PATH=/mnt/steam/steamapps/common/VRChat/ ~/.local/share/Steam/compatibilitytools.d/GE-Proton10-33-rtsp24-1/proton run /mnt/steam/steamapps/common/VRChat/VRChat.exe '--url=create?roomId=8094722763&hidden=true&name=BuildAndRun&url=file:///Z:/mnt/steam/steamapps/common/VRChat/VRChat_Data/StreamingAssets/Worlds/errorworld.vrcw' --enable-debug-gui --enable-sdk-log-levels --enable-udon-debug-logging --no-vr
```

### VRChat URI Handler

Save to `~/.local/share/applications/vrchat-uri-handler.desktop`, and Firefox will let you click "Launch World" buttons on the website. It'll let you `xdg-open vrchat://` too.

```desktop file=vrchat-uri-handler.desktop
[Desktop Entry]
Name=URI-vrchat
Comment=URI handler for vrchat://
Exec=/usr/bin/env steam -applaunch 438100 %U
Terminal=false
Type=Application
Categories=Game;
MimeType=x-scheme-handler/vrchat;
NoDisplay=true
```
