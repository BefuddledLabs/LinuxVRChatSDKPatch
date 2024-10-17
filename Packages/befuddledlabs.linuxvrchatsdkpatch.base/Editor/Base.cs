using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using VRC.Core;

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

        private static void OnProtonInstallPathGUI()
        {
            var protonPath = GetSavedProtonPath();
            EditorGUILayout.LabelField("Proton", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Installed Proton Path: ", protonPath);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("");
            
            if (GUILayout.Button("Edit"))
            {
                var initPath = "";
                if (!string.IsNullOrEmpty(protonPath))
                    initPath = protonPath;

                protonPath = EditorUtility.OpenFilePanel("Choose Proton Binary", initPath, "");
                SetProtonPath(protonPath);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(VRCSdkControlPanel), "ShowSettings")]
        public static IEnumerable<CodeInstruction> ShowSettingsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Call) continue;
                if (!codes[i].operand.ToString().Contains("OnVRCInstallPathGUI")) continue;
                codes.Insert(i, CodeInstruction.Call(typeof(Base), "OnProtonInstallPathGUI"));
                break;
            }

            return codes.AsEnumerable();
        }
    }
}