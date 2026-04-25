#nullable enable

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor;
using HarmonyLib;
using UnityEngine.Networking;
using VRC.Core;
using VRC.SDK3.Editor.Builder;
using VRC.SDKBase.Editor;
using Debug = UnityEngine.Debug;

namespace BefuddledLabs.LinuxVRChatSdkPatch.Worlds.Editor.Patches
{
    /// <summary>
    ///     Patch for the VRChat Worlds SDK Build and Test functionality.
    /// </summary>
    [HarmonyPatch]
    // ReSharper disable once UnusedType.Global
    public static class PatchBuildAndTestWorld
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(VRCWorldAssetExporter), "RunWorldTestDesktop", typeof(string))]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        // ReSharper disable once UnusedMember.Global
        public static bool RunWorldTestDesktopPrefix(object[] __args)
        {
            var bundleFilePath = (string)__args[0];

            // ORIGINAL METHOD, with patches. Patches are marked clearly.

            // <patch> - gather proton, prefix, steam root, and compatdata
            var launchConfig = LaunchConfiguration.Resolve(LinuxVrcEditorPrefs.CustomProtonPath);
            if (launchConfig == null)
            {
                Debug.LogError("Couldn't find everything needed; aborting build-and-test.");
                return false;
            }
            // </patch>

            // <patch> - logging
            launchConfig.DebugPrint();
            Debug.Log($"Bundle path: \"{bundleFilePath}\"");
            // </patch>

            // <patch> translate path with winepath, relative to Z:/
            bundleFilePath = ToWinePath(bundleFilePath);
            // </patch>
            var bundleUri = UnityWebRequest.EscapeURL(bundleFilePath).Replace("+", "%20");
            var randomDigits = VRC.Tools.GetRandomDigits(10);
            var executable = SDKClientUtilities.GetSavedVRCInstallPath();
            // <patch> - remove URL path, this is almost never taken in Windows either
            // if (string.IsNullOrEmpty(executable) || !File.Exists(executable))
            // executable = $"vrchat://create?roomId={randomDigits}&hidden=true&name=BuildAndRun&url=file:///{bundleUri}";
            // </patch>
            var argUrl = $"--url=create?roomId={randomDigits}&hidden=true&name=BuildAndRun&url=file:///{bundleUri}";
            var argsRest =
                $"--enable-debug-gui --enable-sdk-log-levels --enable-udon-debug-logging {(VRCSettings.ForceNoVR ? " --no-vr" : "")}{(VRCSettings.WatchWorlds ? " --watch-worlds" : "")}";
            // <patch> - set proton as the executable, prefix args with "run" (as in the Steam compatibility tool interface Verb) and path/to/VRChat.exe
            var startInfo = new ProcessStartInfo(launchConfig.ProtonExecutable, $"run {executable} {argUrl} {argsRest}")
                // </patch>
                {
                    WorkingDirectory = Path.GetDirectoryName(executable) ?? "",
                    // <patch> - add these environment variables and skip parsing with system shell
                    Environment =
                    {
                        { "STEAM_COMPAT_DATA_PATH", launchConfig.CompatDataPath },
                        { "STEAM_COMPAT_CLIENT_INSTALL_PATH", launchConfig.SteamRoot },
                        { "STEAM_COMPAT_INSTALL_PATH", launchConfig.VrcInstallRoot },
                    },
                    UseShellExecute = false
                    // </patch>
                };
            for (var index = 0; index < VRCSettings.NumClients; ++index)
            {
                Process.Start(startInfo);
                Thread.Sleep(3000);
            }

            AnalyticsSDK.BuildAndTestLaunched(RuntimeInformation.OSDescription, "Desktop", "world");
            // END ORIGINAL METHOD

            return false; // skips the original
        }

        /// <summary>
        ///     Turn <c>/home/you/some/filepath/file.txt</c> into <c>Z:\home\you\some\filepath\file.txt</c>
        /// </summary>
        /// <param name="path">The path to translate.</param>
        /// <returns>An absolute Windows-style path.</returns>
        private static string ToWinePath(string path)
        {
            var fullPath = Path.GetFullPath(path);
            var winePath = "Z:" + fullPath.Replace("/", @"\");
            Debug.Log($"Translated path: \"{winePath}\"");
            return winePath;
        }
    }
}