#nullable enable

namespace BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor
{
    public static class LinuxVrcEditorPrefs
    {
        public const string PrefsKeyCustomProtonPath = "LinuxVRC_customProtonPath";
        public const string PrefsKeyCannyDialog = "LinuxVRC_cannyDialog";

        /// <summary>
        ///     An absolute path to the install dir of the user's preferred Proton version, or <see langword="null"/> if it is unset.
        /// </summary>
        public static string? CustomProtonPath
        {
            get
            {
                string? val = null;
                if (UnityEditor.EditorPrefs.HasKey(PrefsKeyCustomProtonPath))
                    val = UnityEditor.EditorPrefs.GetString(PrefsKeyCustomProtonPath);
                return val;
            }
            set => UnityEditor.EditorPrefs.SetString(PrefsKeyCustomProtonPath, value);
        }

        /// <summary>
        ///     Whether the user has been shown the popup asking them to vote for the VRCSDK Canny for Linux support.
        ///     If true, it won't be shown again.
        /// </summary>
        public static bool CannyDialog
        {
            get => UnityEditor.EditorPrefs.GetBool(PrefsKeyCannyDialog, false);
            set => UnityEditor.EditorPrefs.SetBool(PrefsKeyCannyDialog, value);
        }
    }
}