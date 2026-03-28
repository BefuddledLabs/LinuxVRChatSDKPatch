using System;
using System.Diagnostics;
using System.IO;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEditor;
using VRC.Core;
using VRC.SDKBase.Editor;

namespace BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor
{
    [HarmonyPatch]
    public static class Base
    {
        [CanBeNull]
        public static string GetCompatDataPath()
        {
            var vrChatPath = SDKClientUtilities.GetSavedVRCInstallPath();
            if (string.IsNullOrWhiteSpace(vrChatPath))
                return null;

            var dir = new FileInfo(vrChatPath).Directory;
            if (dir == null)
                return null;

            while (!dir.Name.Contains("steamapps", StringComparison.OrdinalIgnoreCase))
            {
                dir = dir.Parent;
                if (dir == null)
                    return null;
            }

            return dir.FullName + "/compatdata/";
        }

        static Base()
        {
            // Check if the user has protontricks installed
            try
            {
                var p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "protontricks-launch",
                        Arguments = "-h", // so it doesn't fail
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false
                    }
                };

                p.Start();
                p.WaitForExit();

                HasProtonTricks = p.ExitCode == 0;
            }
            catch
            {
                HasProtonTricks = false;
            }
        }

        public static bool HasProtonTricks;

        [CanBeNull]
        public static string GetVrcCompatDataPath()
        {
            return GetCompatDataPath() + "438100/";
        }

        public static string ProtonPath
        {
            get
            {
                var savedVrcInstallPath = "";
                if (EditorPrefs.HasKey("LinuxVRC_protonPath"))
                    savedVrcInstallPath = EditorPrefs.GetString("LinuxVRC_protonPath");
                return savedVrcInstallPath;
            }
            set => EditorPrefs.SetString("LinuxVRC_protonPath", value);
        }
        
        public static bool ProtonTricksPrefs
        {
            get
            {
                var savedProtonTricksPrefs = true; // Default use proton tricks
                if (EditorPrefs.HasKey("LinuxVRC_protonTricksPrefs"))
                    savedProtonTricksPrefs = EditorPrefs.GetBool("LinuxVRC_protonTricksPrefs");
                return savedProtonTricksPrefs;
            }
            set => EditorPrefs.SetBool("LinuxVRC_protonTricksPrefs", value);
        }

        // Thanks Bartkk <3
        [HarmonyPrefix]
        [HarmonyPatch(typeof(VRC_SdkBuilder), "GetLocalLowPath")]
        public static bool GetLocalLowPathPrefix(ref string __result)
        {
            __result = GetVrcCompatDataPath() + "pfx/drive_c/users/steamuser/AppData/LocalLow/";
            return false;
        }
    }
}