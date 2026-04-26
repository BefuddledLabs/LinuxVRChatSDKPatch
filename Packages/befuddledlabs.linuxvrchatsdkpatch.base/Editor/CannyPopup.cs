#nullable enable

using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor
{
    [InitializeOnLoad]
    public static class CannyPopup
    {
        [MenuItem("VRChat SDK/Utilities/Linux/Reset Canny popup preference")]
        public static void ResetCannyDialog() => LinuxVrcEditorPrefs.CannyDialog = false;

        static CannyPopup()
        {
            if (LinuxVrcEditorPrefs.CannyDialog ||
                !RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return;

            var result = EditorUtility.DisplayDialog("Linux VRChat SDK Patch",
                "Please upvote this VRChat Canny, which would obviate the need for these SDK patches.",
                "Open Canny", "Don't show again");

            if (result)
                Application.OpenURL(
                    "https://feedback.vrchat.com/sdk-bug-reports/p/add-proton-support-to-the-sdk-for-local-tests");
            LinuxVrcEditorPrefs.CannyDialog = true;
        }
    }
}