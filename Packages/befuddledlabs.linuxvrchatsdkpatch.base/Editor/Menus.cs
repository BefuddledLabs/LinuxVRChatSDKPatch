#nullable enable

using UnityEditor;
using UnityEngine;
using VRC.Core;

namespace BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor
{
    public static class Menus
    {
        /// <summary>
        /// Print out the LinuxVRC Unity EditorPrefs.
        /// </summary>
        [MenuItem("Tools/Linux VRChat SDK Patch/Debug Print Preferences")]
        public static void DebugPrintPreferences()
        {
            Debug.Log($"{LinuxVrcEditorPrefs.PrefsKeyCannyDialog}: \"{LinuxVrcEditorPrefs.CannyDialog}\"");
            Debug.Log($"{LinuxVrcEditorPrefs.PrefsKeyCustomProtonPath}: \"{LinuxVrcEditorPrefs.CustomProtonPath}\"");
        }

        /// <summary>
        /// Print out the VRCSDK Unity EditorPrefs that we care about.
        /// </summary>
        [MenuItem("Tools/Linux VRChat SDK Patch/Debug Print VRCSDK Preferences")]
        public static void DebugPrintVrcSdkPreferences()
        {
            Debug.Log($"{nameof(SDKClientUtilities.GetSavedVRCInstallPath)}: \"{SDKClientUtilities.GetSavedVRCInstallPath()}\"");
            const string key = "VRC_installedClientPath";
            Debug.Log($"EditorPrefs {key}: \"{EditorPrefs.GetString(key)}\"");
        }

        /// <summary>
        /// Print out the current launch configuration, as if we were launching Build and Test right now.
        /// </summary>
        /// <seealso cref="LaunchConfiguration"/>
        [MenuItem("Tools/Linux VRChat SDK Patch/Debug Print Launch Configuration")]
        public static void DebugPrintLaunchConfiguration()
        {
            var launchConfiguration = LaunchConfiguration.Resolve(protonPath: null);
            if (launchConfiguration == null)
            {
                Debug.Log("Couldn't determine launch configuration.");
            }
            else
            {
                launchConfiguration.DebugPrint();
            }
        }
    }
}