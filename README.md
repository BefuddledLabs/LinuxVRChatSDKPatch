# Linux VRChat SDK Patch

> [!WARNING]
> This modifies the VRChat SDK using [Harmony](https://github.com/pardeike/Harmony) to properly work on Linux. \
> This is directly against the VRChat Terms of Service.

## How to install
1. [ALCOM](https://github.com/vrc-get/vrc-get) [Recommended]
    1. Add the `Linux VRChat SDK Patch` package to [ALCOM](https://github.com/vrc-get/vrc-get) via the listing at [`befuddledlabs.github.io/LinuxVRChatSDKPatch`](https://befuddledlabs.github.io/LinuxVRChatSDKPatch/).
    2. Install the appropriate package `Linux VRChat SDK Patch` Worlds or Avatars*
2. Manual
    1. Download the Base and either Worlds or Avatars* UnityPackage(s) from the [Releases](https://github.com/BefuddledLabs/LinuxVRChatSDKPatch/releases).

* The Avatars package hasn't been made yet.

## How to use
![VRCSdkSettings](/Docs/VRCSdkSettings)

Select both your VRChat exe and proton binary in the settings of the VRChat SDK's settings pannel.

## Development

To make modifications to this package:

1. Clone this repository to a non-unity project folder.
2. Create a symbolic link from the package(s) into a Unity project's package folder.
3. The package should be editable via Unity and any external editor.

## Acknowledgements
- [*Bartkk*](https://github.com/Bartkk0)
  - For making the original [VRCSDKonLinux](https://github.com/Bartkk0/VRCSDKonLinux).
  - And sharing their latest patches they hadn't gotten around to releasing.
