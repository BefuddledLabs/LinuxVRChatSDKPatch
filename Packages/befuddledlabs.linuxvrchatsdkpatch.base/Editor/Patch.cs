#nullable enable

using System.Linq;
using System.Runtime.InteropServices;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor
{
    [InitializeOnLoad]
    public static class Patch
    {
        private const string HarmonyID = "BefuddledLabs.LinuxVRChatSdkPatch.Base";
        private static readonly Harmony BaseHarmony = new(HarmonyID);

        static Patch()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return;

            BaseHarmony.PatchAll();
            var count = BaseHarmony.GetPatchedMethods().Count();
            Debug.Log($"{HarmonyID}: Patched {count} methods");
        }
    }
}