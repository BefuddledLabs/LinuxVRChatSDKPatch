#nullable enable

using System.IO;
using BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor.Locators;
using UnityEngine;

namespace BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor
{
    /// <summary>
    ///     Represents all Linux-specific settings required to start VRChat using the World SDK Build and Test functionality,
    ///     determined by resolving all defaults with the user's preferences in Unity and Steam.
    /// </summary>
    public class LaunchConfiguration
    {
        private LaunchConfiguration(string steamRoot, string protonExecutable, string compatDataPath,
            string vrcInstallRoot)
        {
            SteamRoot = steamRoot;
            ProtonExecutable = protonExecutable;
            CompatDataPath = compatDataPath;
            VrcInstallRoot = vrcInstallRoot;
        }

        /// <summary>
        ///     An absolute path to the Steam root directory.
        /// </summary>

        public string SteamRoot { get; }

        /// <summary>
        ///     An absolute path to a Proton version's <c>proton</c> Python script.
        /// </summary>
        public string ProtonExecutable { get; }

        /// <summary>
        ///     An absolute path to the <c>compatdata/</c> directory in a Proton prefix.
        /// </summary>

        public string CompatDataPath { get; }

        /// <summary>
        ///     An absolute path to <c>common/VRChat/</c>
        /// </summary>
        public string VrcInstallRoot { get; }

        /// <summary>
        ///     Determine the environment taking preferences and locations from Steam and Unity into account by which we should
        ///     launch VRChat in offline Build and Test mode.
        /// </summary>
        /// <param name="protonPath">
        ///     The value of the Unity EditorPref for custom Proton version, or <see langword="null" /> if it is unset.
        /// </param>
        /// <returns></returns>
        public static LaunchConfiguration? Resolve(string? protonPath)
        {
            // SteamRoot
            var steamRoot = SteamLocator.FindSteamRoot();
            if (steamRoot == null || !SteamLocator.IsValidSteamRoot(steamRoot))
            {
                Debug.LogError($"Couldn't find Steam root: \"{steamRoot}\"");
                return null;
            }

            // ProtonExecutable
            if (!ProtonLocator.IsValidCompatToolPath(protonPath))
            {
                Debug.Log("Custom Proton install path is unset or invalid, will auto-detect.");

                protonPath = ProtonLocator.GetSteamVdfCompatTool(steamRoot, VrcLocator.VrcAppId);
                if (!ProtonLocator.IsValidCompatToolPath(protonPath))
                {
                    Debug.LogError($"Couldn't find compat tool used for VRChat: {protonPath}");
                    return null;
                }
            }

            if (protonPath == null)
                return null; // satisfies compiler

            var protonExecutable = Path.Combine(protonPath, "proton");

            // CompatDataPath
            var compatDataPath = VrcLocator.GetCompatDataPath();
            if (compatDataPath == null || !VrcLocator.IsValidCompatDataPath(compatDataPath))
            {
                Debug.LogError($"Could not find VRChat's compatdata: \"{compatDataPath}\"");
                return null;
            }

            // VrcGameRoot
            var vrcExePath = VrcLocator.GetVrcInstallPath();
            if (vrcExePath == null || !VrcLocator.IsValidVrcInstallPath(vrcExePath))
            {
                Debug.LogError($"Could not locate VRChat.exe: \"{vrcExePath}\"");
                return null;
            }

            var vrcInstallRoot = Path.GetDirectoryName(vrcExePath);
            // ReSharper disable once InvertIf
            if (vrcInstallRoot == null || !Directory.Exists(vrcInstallRoot))
            {
                Debug.LogError($"Could not locate VRChat's install directory: \"{vrcInstallRoot}\"");
                return null;
            }

            // TODON'T: read toolmanifest.vdf "commandline"? nah
            return new LaunchConfiguration(steamRoot, protonExecutable, compatDataPath, vrcInstallRoot);
        }

        public void DebugPrint()
        {
            Debug.Log($"Steam root: \"{SteamRoot}\"");
            Debug.Log($"Proton executable: \"{ProtonExecutable}\"");
            Debug.Log($"Compat data path: \"{CompatDataPath}\"");
            Debug.Log($"VRChat install directory: \"{VrcInstallRoot}\"");
        }
    }
}