#nullable enable

using BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor.Locators;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor.Patches
{
    /// <summary>
    /// Patch to add our GUI to the VRCSDK control panel.
    /// </summary>
    [HarmonyPatch]
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedType.Global
    public static class PatchVRCSdkControlPanel
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(VRCSdkControlPanel), "OnVRCInstallPathGUI")]
        // ReSharper disable once UnusedMember.Global
        public static bool OnVRCInstallPathGUIPrefix()
        {
            // show our section
            OnLinuxVRChatSdkPatchGUI();
            
            // show "VRChat Client - Installed Client Path"
            return true; // run the original
        }
        
        private static void OnLinuxVRChatSdkPatchGUI()
        {
            EditorGUILayout.LabelField("Linux VRChat SDK Patch", EditorStyles.boldLabel);
            OnGUIRowCustomProton();
            EditorGUILayout.Separator();
        }

        private static void OnGUIRowCustomProton()
        {
            var customProtonPath = LinuxVrcEditorPrefs.CustomProtonPath;
            EditorGUILayout.LabelField("Custom Proton: ", customProtonPath ?? "");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("");

            if (GUILayout.Button("Edit"))
            {
                var initPath = GetInitPath();
                if (!string.IsNullOrEmpty(customProtonPath))
                    initPath = customProtonPath;

                customProtonPath = EditorUtility.OpenFolderPanel("Choose Proton directory", initPath, "");
                if (!ProtonLocator.IsValidCompatToolPath(customProtonPath))
                {
                    const string message =
                        "This does not look like a Proton path.\n" +
                        "Please choose a directory that contains a Proton version that you have installed." +
                        "It should contain files named \"proton\" and \"compatibilitytool.vdf\".";
                    EditorUtility.DisplayDialog("Couldn't set custom Proton path", message, "OK");
                }
                else
                {
                    LinuxVrcEditorPrefs.CustomProtonPath = customProtonPath;
                }
            }

            if (GUILayout.Button("Revert to Default"))
            {
                LinuxVrcEditorPrefs.CustomProtonPath = null;
            }

            EditorGUILayout.EndHorizontal();
        }

        private static string GetInitPath()
        {
            var initPath = ProtonLocator.GetCompatibilityToolsDotD();
            if (initPath != null)
                return initPath;
            Debug.LogWarning("Could not locate compatibilitytools.d directory");
            return string.Empty;
        }
    }
}