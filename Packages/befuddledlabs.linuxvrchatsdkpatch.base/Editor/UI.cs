using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor
{
    [HarmonyPatch]
    public static class UI
    {
        private static string[] _options = { "Manual", "ProtonTricks" };

        private static void OnProtonInstallPathGUI()
        {
            var protonPath = Base.ProtonPath;
            EditorGUILayout.LabelField("Proton", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Proton Python File: ", protonPath);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("");

            if (GUILayout.Button("Edit"))
            {
                var initPath = "";
                if (!string.IsNullOrEmpty(protonPath))
                    initPath = protonPath;

                protonPath = EditorUtility.OpenFilePanel("Choose Proton Python File (not wine in the proton folder)",
                    initPath, "");
                Base.ProtonPath = protonPath;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        private static void OnUseProtonTricksGUI()
        {
            var selectedIndex = Base.ProtonTricksPrefs ? 1 : 0;
            if (EditorGUILayout.Popup("VRC/Proton Path Selection", selectedIndex, _options) != selectedIndex)
                Base.ProtonTricksPrefs = !Base.ProtonTricksPrefs;
        }

        private static CodeInstruction _originalCall;

        private static void OnGUI()
        {
            if (Base.HasProtonTricks)
                OnUseProtonTricksGUI();
            else
            {
                EditorGUILayout.LabelField(
                    "If proton tricks is installed, the patch will optionally use that to find your proton install");
                EditorGUILayout.Space();
            }

            if (!Base.ProtonTricksPrefs)
                OnProtonInstallPathGUI();

            if (_originalCall.operand is MethodInfo method)
                method.Invoke(null, null);
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

                _originalCall = codes[i];
                codes[i] = CodeInstruction.Call(typeof(UI), nameof(OnGUI));

                break;
            }

            return codes.AsEnumerable();
        }
    }
}