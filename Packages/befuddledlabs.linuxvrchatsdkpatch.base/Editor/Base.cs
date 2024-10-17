using System;
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

        [CanBeNull]
        public static string GetVrcCompatDataPath()
        {
            return GetCompatDataPath() + "438100/";
        }
        
        public static string GetSavedProtonPath()
        {
            var savedVrcInstallPath = "";
            if (EditorPrefs.HasKey("LinuxVRC_protonPath"))
                savedVrcInstallPath = EditorPrefs.GetString("LinuxVRC_protonPath");
            return savedVrcInstallPath;
        }

        public static void SetProtonPath(string protonPath)
        {
            EditorPrefs.SetString("LinuxVRC_protonPath", protonPath);
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