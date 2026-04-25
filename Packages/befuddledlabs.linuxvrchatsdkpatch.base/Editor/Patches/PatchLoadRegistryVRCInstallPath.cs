#nullable enable

using System.Diagnostics.CodeAnalysis;
using BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor.Locators;
using HarmonyLib;
using UnityEngine;
using VRC.Core;

namespace BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor.Patches
{
    /// <summary>
    ///     Patch for VRCSDK's "Revert to Default" button in <see langword="VRCSdkControlPanel.OnVRCInstallPathGUI()" />, and
    ///     the initialization in <see cref="VRCSdkControlPanel.InitAccount()" />.
    /// </summary>
    [HarmonyPatch]
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedType.Global
    public static class PatchLoadRegistryVRCInstallPath
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SDKClientUtilities), nameof(SDKClientUtilities.LoadRegistryVRCInstallPath))]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        // ReSharper disable once UnusedMember.Global
        public static bool LoadRegistryVRCInstallPathPrefix(ref string __result)
        {
            var ourPath = VrcLocator.GetVrcInstallPathFromSteam();
            if (!string.IsNullOrEmpty(ourPath))
            {
                __result = ourPath!;
            }

            Debug.Log($"Found VRChat.exe from Steam: {ourPath!}");
            return false; // skip original method
        }
    }
}