#nullable enable

using System.Diagnostics.CodeAnalysis;
using BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor.Locators;
using HarmonyLib;
using UnityEngine;
using VRC.SDK3.Editor.Builder;
using VRC.SDKBase.Editor;

namespace BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor.Patches
{
    /// <summary>
    ///     Patch for <see cref="VRCWorldAssetExporter.ExportCurrentSceneResource()" />,
    ///     <see cref="VRCSdkControlPanel.FetchTestAvatars()" />.
    /// </summary>
    [HarmonyPatch]
    // ReSharper disable once UnusedType.Global
    public class PatchGetLocalLowPath
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(VRC_SdkBuilder), nameof(VRC_SdkBuilder.GetLocalLowPath))]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        // ReSharper disable once UnusedMember.Global
        public static bool GetLocalLowPathPrefix(ref string __result)
        {
            var ourPath = VrcLocator.GetLocalLowPath();
            if (!string.IsNullOrEmpty(ourPath))
            {
                __result = ourPath!;
            }

            Debug.Log($"LocalLow Path: \"{ourPath}\"");
            return false; // skip original method
        }
    }
}