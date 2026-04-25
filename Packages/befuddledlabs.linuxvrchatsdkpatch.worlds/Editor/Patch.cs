#nullable enable

using System.Linq;
using System.Runtime.InteropServices;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace BefuddledLabs.LinuxVRChatSdkPatch.Worlds.Editor
{
    [InitializeOnLoad]
    public static class Patch
    {
        private const string HarmonyID = "BefuddledLabs.LinuxVRChatSdkPatch.Worlds";
        private static readonly Harmony WorldsHarmony = new(HarmonyID);

        static Patch()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return;
            
            WorldsHarmony.PatchAll();
            var count = WorldsHarmony.GetPatchedMethods().Count();
            Debug.Log($"{HarmonyID}: Patched {count} methods");
        }
    }
}
