# Linux VRChat SDK Patch

> [!WARNING]
> This modifies the VRChat SDK using [Harmony](https://github.com/pardeike/Harmony) to properly work on Linux. \
> This is directly against the VRChat Terms of Service.

<!-- break -->

> [!IMPORTANT]
> Please support this [Canny issue - Add Proton support to the SDK for local tests](https://feedback.vrchat.com/sdk-bug-reports/p/add-proton-support-to-the-sdk-for-local-tests) so that these patches wouldn't be required in the future.

## How to Install

1. [ALCOM][alcom] \[Recommended\]
    1. Add the **Linux VRChat SDK Patch** package to [ALCOM][alcom] via the listing at [`befuddledlabs.github.io/LinuxVRChatSDKPatch`](https://befuddledlabs.github.io/LinuxVRChatSDKPatch/).
    2. Install the appropriate **Linux VRChat SDK Patch** package for your project: **Worlds** or **Avatars**.
2. Manually
    1. Download the **Base** and either **Worlds** or **Avatars** UnityPackage(s) from [Releases](https://github.com/BefuddledLabs/LinuxVRChatSDKPatch/releases).

[alcom]: https://github.com/vrc-get/vrc-get

## How to Use

![VRCSDKSettings](/Docs/VRCSDKSettings.webp)

Everything should work out of the box.

This package detects VRChat's game directory and Proton prefix, your preferred Proton version for VRChat as set in Steam, whether custom or system-wide or official, in any Steam library, and patches VRCSDK functionality to take this into account.

If you want, you can choose a different VRChat.exe or different Proton version to launch by clicking **Edit** in the VRCSDK Settings tab.

If it isn't working, use the buttons in the **Tools &rarr; Linux VRChat SDK Patch** menu to print some logs, then please open a [GitHub issue](https://github.com/BefuddledLabs/LinuxVRChatSDKPatch/issues)!

## Features

- Fixes the VRCSDK initialization so it can correctly find VRChat.exe.
- Prevents the pointless creation (lol) and use of `~/.local/share/VRChat`, instead saving test worlds and avatars to the Proton prefix.
- Fixes the Content Manager tab so it can show your test avatars.
- Fixes the Build and Test button, allowing for multiple clients to test a world in offline mode.
- Adds some UI in VRCSDK Settings tab to select a different Proton to use instead of what's set in Steam.
- Show a one-time dialog to ask for votes on the Canny for Linux support.
- Add debugging helpers to the **Tools &rarr; Linux VRChat SDK Patch** menu.

## Development

For a technical overview on what these packages do, see [NOTES.md](NOTES.md).

To make modifications to this package:

1. Uninstall the **Linux VRChat SDK Patch** packages from your project, if necessary.
2. Clone this repository to a directory not inside your Unity project.
3. Create a symbolic link from the package(s) into your Unity project's package folder.
4. The package should be editable via Unity and any external editor.

## Acknowledgements

[**Bartkk**](https://github.com/Bartkk0) for making the original [VRCSDKonLinux](https://github.com/Bartkk0/VRCSDKonLinux) and sharing their then-unreleased patches.
