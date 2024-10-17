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
            var compatDataPath = Base.Editor.Base.GetVrcCompatDataPath();
            if (compatDataPath == null)
            {
                Debug.LogError("Could not find compatdata Path");
                return false;
            }

            var bundleFilePath = ((string)__args[0]).Replace('\\', '/');
            var pluginFilePath = ((string)__args[1]).Replace('\\', '/');

            var path = SDKClientUtilities.GetSavedVRCInstallPath();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Debug.LogError("couldn't get vrchat path..");
                return true;
            }

            var args = new StringBuilder();
            args.Append("run ");
            args.Append(path);
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

            var argsPathFixed = Regex.Replace(args.ToString(), @"file:[/\\]*", "file:///Z:/");
            var processStartInfo =
                new ProcessStartInfo(Base.Editor.Base.GetSavedProtonPath(), argsPathFixed)
                {
                    EnvironmentVariables =
                    {
                        { "STEAM_COMPAT_DATA_PATH", Environment.GetEnvironmentVariable("HOME") + compatDataPath },
                        { "STEAM_COMPAT_CLIENT_INSTALL_PATH", Environment.GetEnvironmentVariable("HOME") + "/.steam/" }
                    },
                    WorkingDirectory = Path.GetDirectoryName(path) ?? "",
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

        // Thanks Bartkk0
        // Source: https://github.com/Bartkk0/VRCSDKonLinux/blob/master/WorldSdkPatcher.cs
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(VRCWorldAssetExporter), "ExportCurrentSceneResource", typeof(bool), typeof(Action<string>), typeof(Action<object>))]
        public static IEnumerable<CodeInstruction> ExportCurrentSceneResourceTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Ldstr) continue;
                if (!codes[i].operand.ToString().Contains(".vrcw")) continue;
                
                i += 3;
                var str2 = codes[i].operand;
                codes.Insert(++i, new CodeInstruction(OpCodes.Ldloc_S, str2));
                codes.Insert(++i, CodeInstruction.Call(typeof(String), "ToLower"));
                codes.Insert(++i, new CodeInstruction(OpCodes.Stloc_S, str2));
                break;
            }

            return codes.AsEnumerable();
        }
    }
}