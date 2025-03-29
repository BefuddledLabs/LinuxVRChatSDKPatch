using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor
{
    [InitializeOnLoad]
    public static class CannyPopup
    {
        [MenuItem("VRChat SDK/Utilities/Linux/Clear LinuxVRC_cannyDialog")]
        public static void ResetLinuxVRC_cannyDialog() => EditorPrefs.SetBool("LinuxVRC_cannyDialog", false);
        
        static CannyPopup()
        {
            if (EditorPrefs.GetBool("LinuxVRC_cannyDialog", false) || !RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return;
            
            var result = EditorUtility.DisplayDialog("Linux VRChat Patch",
                "Please upvote this canny instead of needing this patch local tests.",
                "Open Canny", "Don't show again");
            
            if (result)
               Application.OpenURL("https://feedback.vrchat.com/sdk-bug-reports/p/add-proton-support-to-the-sdk-for-local-tests");
            EditorPrefs.SetBool("LinuxVRC_cannyDialog", true);//
        }
    }
}
