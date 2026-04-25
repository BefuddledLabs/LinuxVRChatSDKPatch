#nullable enable

using System.IO;
using UnityEditor;
using UnityEngine;
using VRC.Core;

namespace BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor.Locators
{
    /// <summary>
    ///     Locates <c>VRChat.exe</c> using Steam, taking VRCSDK's user-set preferences into account.
    /// </summary>
    /// <remarks>
    ///     We must know these VRChat paths:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>compatdata path, which contains the Proton prefix</description>
    ///         </item>
    ///         <item>
    ///             <description><c>AppData/LocalLow</c>, located within the prefix</description>
    ///         </item>
    ///         <item>
    ///             <description>VRChat install path, which contains <c>VRChat.exe</c></description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <c>VRChat.exe</c>
    ///             </description>
    ///         </item>
    ///     </list>
    ///     These aren't in fixed locations on every machine, but if the path to <c>VRChat.exe</c> is known, we can locate the
    ///     rest.
    ///     <br />
    ///     This class provides <see cref="GetVrcInstallPathFromSteam" /> that will locate Steam, then the VRChat install
    ///     directory, then discover <c>VRChat.exe</c>.
    ///     <br />
    ///     VRCSDK has a Unity EditorPref named <c>VRC_installedClientPath</c> which is a variable meant to hold an absolute
    ///     path to <c>VRChat.exe</c>. Initially, it is empty, but once VRCSDK first loads, it attempts to discover
    ///     <c>VRChat.exe</c> and set this preference. It will fail on Linux, so we patch it to call our
    ///     <see cref="GetVrcInstallPathFromSteam" />  instead. With this, <c>VRC_installedClientPath</c> is initialized
    ///     correctly, and we can treat it as the source of truth. Being a preference, the user may also choose to set their
    ///     own custom path to <c>VRChat.exe</c> in VRCSDK Settings.
    ///     <br />
    ///     Unless specified otherwise, all of these methods take the value of <c>VRC_installedClientPath</c> into account.
    /// </remarks>
    public static class VrcLocator
    {
        /// <summary>
        ///     The Steam AppId for VRChat.
        /// </summary>
        public const string VrcAppId = "438100";

        /// <summary>
        ///     Locates VRChat's <c>AppData/LocalLow</c> directory, and returns it as an absolute filepath.
        ///     <br />
        ///     This takes into account VRCSDK's <c>VRC_installedClientPath</c>.
        /// </summary>
        /// <returns>An absolute path to <c>LocalLow</c> if found, <see langword="null" /> otherwise.</returns>
        public static string? GetLocalLowPath()
        {
            var compatDataPath = GetCompatDataPath();
            if (compatDataPath == null)
            {
                return null;
            }

            var relativeLocalLow = Path.Combine(compatDataPath, "pfx",
                "drive_c", "users", "steamuser", "AppData", "LocalLow");
            relativeLocalLow = Path.GetFullPath(relativeLocalLow);

            return relativeLocalLow;
        }

        /// <summary>
        ///     Gets the value for <c>STEAM_COMPAT_DATA_PATH</c>, e.g. <c>~/.local/share/Steam/steamapps/compatdata/438100/</c>
        ///     <br />
        ///     This takes the value of VRCSDK's <c>VRC_installedClientPath</c> into account.
        /// </summary>
        /// <returns>An absolute path to the compatdata path if found, <see langword="null" /> otherwise.</returns>
        public static string? GetCompatDataPath()
        {
            var savedVrcInstallPath = GetVrcInstallPath();
            if (savedVrcInstallPath == null)
            {
                return null;
            }

            // = <a steam library>/steamapps/common/VRChat/VRChat.exe
            // to
            // = <a steam library>/
            var libraryPath = Path.GetFullPath(Path.Combine(savedVrcInstallPath, "..", "..", "..", ".."));
            var compatDataPath = Path.Combine(libraryPath, "steamapps", "compatdata", VrcAppId);
            compatDataPath = Path.GetFullPath(compatDataPath);
            return compatDataPath;
        }

        /// <summary>
        ///     Get the known path to <c>VRChat.exe</c>.
        ///     <br />
        ///     If VRCSDK's editor preference key <c>VRC_installedClientPath</c> is set and the path exists, return that.
        ///     Otherwise, this tries to locate <c>VRChat.exe</c> via Steam <c>libraryfolders.vdf</c> shenanigans, then updates
        ///     <c>VRC_installedClientPath</c> if needed.
        /// </summary>
        /// <returns>An absolute filepath to <c>VRChat.exe</c> if found, <see langword="null" /> otherwise.</returns>
        public static string? GetVrcInstallPath()
        {
            var savedVrcExePath = SDKClientUtilities.GetSavedVRCInstallPath();
            if (IsValidVrcInstallPath(savedVrcExePath))
                return savedVrcExePath;

            Debug.Log(
                $"VRCSDK's own saved VRC install path doesn't point to VRChat.exe: \"{savedVrcExePath ?? "<missing>"}\". We'll try to locate and correct it.");

            var ourVrcExePath = GetVrcInstallPathFromSteam();
            if (!IsValidVrcInstallPath(ourVrcExePath))
                return null;

            Debug.Log($"Updating VRCSDK's saved VRC install path from \"{savedVrcExePath}\" to \"{ourVrcExePath}\"");
            // We would ideally call SDKClientUtilities.SetVRCInstallPath() here, but that method refuses to set
            // VRC_installedClientPath if is unset. That doesn't make any sense, but whatever. Sidestep it.
            // SDKClientUtilities.SetVRCInstallPath(ourVrcExePath);
            EditorPrefs.SetString("VRC_installedClientPath", ourVrcExePath);
            var readback = SDKClientUtilities.GetSavedVRCInstallPath();
            Debug.Log($"Updated; it's now \"{readback}\"");
            return ourVrcExePath;
        }

        /// <summary>
        ///     Locates <c>VRChat.exe</c> as installed by Steam.
        ///     <br />
        ///     Finds Steam's installation path, parses <c>libraryfolders.vdf</c>, finds the library that VRChat is installed in,
        ///     finds <c>VRChat.exe</c>, and returns its absolute filepath.
        ///     <br />
        ///     This does not take into account VRCSDK's editor preference key <c>VRC_installedClientPath</c>.
        /// </summary>
        /// <returns>An absolute filepath to <c>VRChat.exe</c> if found, <see langword="null" /> otherwise.</returns>
        public static string? GetVrcInstallPathFromSteam()
        {
            var steamRoot = SteamLocator.FindSteamRoot();
            if (steamRoot == null)
            {
                return null;
            }

            // e.g. ~/.local/share/Steam or /mnt/steam
            var libraryPath = SteamLocator.GetLibraryWithAppId(steamRoot, VrcAppId);
            if (libraryPath == null)
            {
                Debug.LogError("Couldn't find any Steam library with VRChat installed.");
                return null;
            }

            var vrcExePath = Path.Combine(libraryPath, "steamapps", "common", "VRChat", "VRChat.exe");

            if (IsValidVrcInstallPath(vrcExePath))
                return vrcExePath;
            Debug.LogError(
                $"Couldn't locate VRChat.exe at path: {vrcExePath}\n" +
                "Please try to set it manually in VRChat SDK > Show Control Panel > Settings > VRChat Client.");
            return null;
        }

        /// <summary>
        ///     Determines whether the given path refers to an existing compatdata directory, that contains a Proton prefix.
        /// </summary>
        /// <param name="compatDataPath">The path to test.</param>
        /// <returns><see langword="true" /> if the prefix is detected, <see langword="false" /> otherwise.</returns>
        public static bool IsValidCompatDataPath(string? compatDataPath)
        {
            return !string.IsNullOrEmpty(compatDataPath) &&
                   // Directory.Exists(compatDataPath) &&
                   Directory.Exists(Path.Combine(compatDataPath!, "pfx"));
        }

        /// <summary>
        ///     Determines whether the given filepath refers to an existing <c>VRChat.exe</c> file within an installed game
        ///     directory.
        /// </summary>
        /// <param name="vrcInstallPath">The path to test.</param>
        /// <returns><see langword="true" /> if <c>VRChat.exe</c> was found, <see langword="false" /> otherwise.</returns>
        public static bool IsValidVrcInstallPath(string? vrcInstallPath)
        {
            return !string.IsNullOrEmpty(vrcInstallPath) &&
                   File.Exists(vrcInstallPath) &&
                   vrcInstallPath!.EndsWith("VRChat.exe");
        }
    }
}