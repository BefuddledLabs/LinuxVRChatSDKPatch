using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor
{
    [HarmonyPatch]
    public static class UI
    {
        private static void OnProtonInstallPathGUI()
        {
            var protonPath = Base.GetSavedProtonPath();
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
                Base.SetProtonPath(protonPath);
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
                codes.Insert(i, CodeInstruction.Call(typeof(UI), nameof(OnProtonInstallPathGUI)));
                break;
            }

            return codes.AsEnumerable();
        }
    }
}