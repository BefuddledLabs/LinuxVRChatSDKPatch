using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
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
            var bundleFilePath = ((string)__args[0]).Replace('\\', '/');
            var pluginFilePath = ((string)__args[1]).Replace('\\', '/');

            var str1 = bundleFilePath;//UnityWebRequest.EscapeURL(bundleFilePath).Replace("+", "%20");
            var str2 = pluginFilePath;//UnityWebRequest.EscapeURL(pluginFilePath).Replace("+", "%20");
            var randomDigits = VRC.Tools.GetRandomDigits(10);
            var path = SDKClientUtilities.GetSavedVRCInstallPath();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Debug.LogError("couldn't get vrchat path..");
                return true;
            }

            var str3 = "'--url=create?roomId=" + randomDigits + "&hidden=true&name=BuildAndRun&url=file:///" + str1 + "'";
            if (!string.IsNullOrEmpty(str2))
                str3 = str3 + "&pluginUrl=file:///" + str2;
            var str4 = "--enable-debug-gui --enable-sdk-log-levels --enable-udon-debug-logging " +
                       (VRCSettings.ForceNoVR ? " --no-vr" : "") + (VRCSettings.WatchWorlds ? " --watch-worlds" : "");

            var args = "run " + path + " " + str3 + " " + str4;
            
            args = Regex.Replace(args, @"file:[/\\]*", "file:///Z:/");
            Debug.Log(Base.Editor.Base.GetSavedProtonPath() + args);
            var processStartInfo =
                new ProcessStartInfo(Base.Editor.Base.GetSavedProtonPath(), args)
                {
                    EnvironmentVariables =
                    {
                        { "STEAM_COMPAT_DATA_PATH", Environment.GetEnvironmentVariable("HOME") + "/.steam/steam/steamapps/compatdata/438100/" },
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
        [HarmonyPatch(typeof(VRCWorldAssetExporter), "ExportCurrentSceneResource", typeof(bool), typeof(Action<string>),
            typeof(Action<object>))]
        public static IEnumerable<CodeInstruction> ExportCurrentSceneResourceTranspiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldstr)
                {
                    if (codes[i].operand.ToString().Contains(".vrcw"))
                    {
                        i += 3;
                        var str2 = codes[i].operand;
                        i++;
                        codes.Insert(i++, new CodeInstruction(OpCodes.Ldloc_S, str2));
                        codes.Insert(i++, CodeInstruction.Call(typeof(String), "ToLower"));
                        codes.Insert(i++, new CodeInstruction(OpCodes.Stloc_S, str2));
                        break;
                    }
                }
            }

            return codes.AsEnumerable();
        }
    }
}
