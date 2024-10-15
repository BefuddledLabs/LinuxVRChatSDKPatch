using HarmonyLib;
using UnityEditor;

namespace BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor
{
    [InitializeOnLoad]
    public static class Patch
    {
        internal static Harmony _harmony;


        static Patch()
        {
            _harmony = new Harmony("BefuddledLabs.LinuxVRChatSdkPatch-World");
            _harmony.PatchAll();
        }
    }
}