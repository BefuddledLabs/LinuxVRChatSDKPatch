using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using HarmonyLib;
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
        [HarmonyPatch(typeof(VRCWorldAssetExporter), "RunScene", typeof(string), typeof(string))]
        public static bool RunScenePrefix(object[] __args)
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

            var args = new StringBuilder();
            args.Append("run ");
            args.Append(vrcInstallPath);
            args.Append(' ');
            
            args.Append('\'');
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
            args.Append('\'');
            
            args.Append(" --enable-debug-gui");
            args.Append(" --enable-sdk-log-levels");
            args.Append(" --enable-udon-debug-logging");
            if (VRCSettings.ForceNoVR)
                args.Append(" --no-vr");
            if (VRCSettings.WatchWorlds)
                args.Append(" --watch-worlds");

            var argsPathFixed = Regex.Replace(args.ToString(), @"file:[/\\]*", "file:///Z:/"); // The file we have is relative to / and not the "c drive" Z:/ is /
            var processStartInfo =
                new ProcessStartInfo(Base.Editor.Base.GetSavedProtonPath(), argsPathFixed)
                {
                    EnvironmentVariables =
                    {
                        { "STEAM_COMPAT_DATA_PATH", compatDataPath },
                        { "STEAM_COMPAT_CLIENT_INSTALL_PATH", Environment.GetEnvironmentVariable("HOME") + "/.steam/" }
                    },
                    WorkingDirectory = Path.GetDirectoryName(vrcInstallPath) ?? "",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
            for (var index = 0; index < VRCSettings.NumClients; ++index)
            {
                Process.Start(processStartInfo);
                Thread.Sleep(3000);
            }

            return false;
        }

        // Thanks Bartkk <3
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(VRCWorldAssetExporter), "ExportCurrentSceneResource", typeof(bool), typeof(Action<string>), typeof(Action<object>))]
        public static IEnumerable<CodeInstruction> ExportCurrentSceneResourceTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();

            var insertionIndex = -1;
            for (var i = 0; i < code.Count - 1; i++)
            {
                // strArray[4] = ".vrcw";
                if (code[i].opcode != OpCodes.Ldstr) continue;
                if (!code[i].operand.ToString().Equals(".vrcw")) continue;
                
                insertionIndex = i - 1;
                break;
            }

            if (insertionIndex == -1)
            {
                Debug.LogError("Couldn't find place to modify ExportCurrentSceneResource");
                return code;
            }

            var toAdd = new List<CodeInstruction>();

            // ModifyArray(strArray);
            toAdd.Add(new CodeInstruction(OpCodes.Dup));
            toAdd.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(World), nameof(ModifyArray), new[] { typeof(string[]) })));

            code.InsertRange(insertionIndex, toAdd);
            return code;
        }

        static void ModifyArray(string[] arr)
        {
            // arr[1] = EditorUserBuildSettings.activeBuildTarget.ToString().ToLower();
            for (var i = 0; i < arr.Length; i++)
            {
                if(arr[i] == null) continue;
            
                arr[i] = arr[i].ToLower();
            }
        }
    }
}