using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using HarmonyLib;
using UnityEngine.Networking;
using VRC.Core;
using VRC.SDK3.Editor.Builder;
using VRC.SDKBase.Editor;
using Debug = UnityEngine.Debug;

namespace BefuddledLabs.LinuxVRChatSdkPatch.Worlds.Editor
{
    [HarmonyPatch]
    public static class World
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(VRCWorldAssetExporter), "RunWorldTestDesktop", typeof(string), typeof(string))]
        public static bool RunWorldTestDesktopPrefix(object[] __args)
        {
            var vrcInstallPath = SDKClientUtilities.GetSavedVRCInstallPath();
            if (string.IsNullOrEmpty(vrcInstallPath) || !File.Exists(vrcInstallPath))
            {
                Debug.LogError("couldn't get VRChat path..");
                return true;
            }
            
            var compatDataPath = Base.Editor.Base.GetVrcCompatDataPath();
            if (compatDataPath == null) // Check if we could find the compatdata directory
            {
                Debug.LogError("Could not find compatdata Path");
                return false;
            }

            // Making sure that the paths are using forward slashes
            var bundleFilePath = ((string)__args[0]).Replace('\\', '/');
            var pluginFilePath = ((string)__args[1]).Replace('\\', '/');
            
            bundleFilePath = UnityWebRequest.EscapeURL(bundleFilePath).Replace("+", "%20");
            pluginFilePath = UnityWebRequest.EscapeURL(pluginFilePath).Replace("+", "%20");

            var args = new StringBuilder();
            args.Append("run ");
            args.Append(vrcInstallPath);
            args.Append(' ');
            
            args.Append("--url=create?roomId=");
                args.Append(VRC.Tools.GetRandomDigits(10)); // Random roomId
                    args.Append("&hidden=true");
                        args.Append("&name=BuildAndRun");
                            args.Append("&url=file:///");
                                args.Append(bundleFilePath);
                                if (!string.IsNullOrEmpty(pluginFilePath))
                                {
                                    args.Append("&pluginUrl=file:///");
                                    args.Append(pluginFilePath);
                                }
            
            args.Append(" --enable-debug-gui");
            args.Append(" --enable-sdk-log-levels");
            args.Append(" --enable-udon-debug-logging");
            if (VRCSettings.ForceNoVR)
                args.Append(" --no-vr");
            if (VRCSettings.WatchWorlds)
                args.Append(" --watch-worlds");
            

            var argsPathFixed = Regex.Replace(args.ToString(), @"file:[/\\]*", "file:///Z:/"); // The file we have is relative to / and not the "c drive" Z:/ is /
            
            Debug.Log(Base.Editor.Base.GetSavedProtonPath() + argsPathFixed);

            var processStartInfo =
                new ProcessStartInfo(Base.Editor.Base.GetSavedProtonPath(), argsPathFixed)
                {
                    EnvironmentVariables =
                    {
                        { "STEAM_COMPAT_DATA_PATH", compatDataPath },
                        { "STEAM_COMPAT_CLIENT_INSTALL_PATH", Environment.GetEnvironmentVariable("HOME") + "/.steam/" }
                    },
                    WorkingDirectory = Path.GetDirectoryName(vrcInstallPath) ?? "",
                    UseShellExecute = false
                };
            for (var index = 0; index < VRCSettings.NumClients; ++index)
            {
                Process.Start(processStartInfo);
                Thread.Sleep(3000);
            }

            return false;
        }
    }
}