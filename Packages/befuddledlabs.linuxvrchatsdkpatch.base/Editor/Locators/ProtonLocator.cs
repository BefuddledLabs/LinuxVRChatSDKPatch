#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor.Locators
{
    /// <summary>
    ///     Reads through Steam directories and VDF files to find custom and official compatibility tools ("compat tools"; e.g.
    ///     Proton), the AppIds they are used for, and the Steam user's preferred tool selections.
    /// </summary>
    /// <remarks>
    ///     Much of this was taken from VRCX, MIT.
    ///     <br />
    ///     See also official documentation:
    ///     <see
    ///         href="https://gitlab.steamos.cloud/steamrt/steam-runtime-tools/-/blob/main/docs/steam-compat-tool-interface.md">
    ///         Steam compatibility tool interface
    ///     </see>
    /// </remarks>
    public static class ProtonLocator
    {
        /// <summary>
        ///     Determines the compatibility tool (e.g., Proton) for a game having the given <paramref name="appId" />, and return
        ///     an absolute filepath to the tool's install dir.
        /// </summary>
        /// <param name="steamRoot">An absolute path to the Steam root directory.</param>
        /// <param name="appId">The Steam AppId of the game.</param>
        /// <returns>
        ///     An absolute filepath to the compat tool's install dir, or <see langword="null" /> if it couldn't be determined
        ///     or found.
        /// </returns>
        public static string? GetSteamVdfCompatTool(string steamRoot, string appId)
        {
            var configVdfPath = Path.Combine(steamRoot, "config", "config.vdf");
            if (!File.Exists(configVdfPath))
            {
                Debug.LogError($"Couldn't determine current Proton: config.vdf not found: {configVdfPath}");
                return null;
            }

            var vdfContent = File.ReadAllText(configVdfPath);
            var compatToolMapping = ExtractCompatToolMapping(vdfContent);

            if (!compatToolMapping.TryGetValue(appId, out var compatToolName) || compatToolName == null)
            {
                Debug.Log("config.vdf doesn't have an entry for VRChat, so we'll look for the default.");
                if (!compatToolMapping.TryGetValue("0", out compatToolName) || compatToolName == null)
                {
                    Debug.LogError(
                        $"Couldn't determine current Proton: config.vdf couldn't be parsed, or doesn't have an entry for the appid {appId} and couldn't find the default compat tool.");
                    return null;
                }
            }

            Debug.Log($"Using compat tool name: {compatToolName}");

            var compatTool = GetCompatToolDirForName(steamRoot, compatToolName);
            if (compatTool == null)
            {
                Debug.LogError(
                    $"Couldn't determine current Proton: couldn't find compat tool dir named \"{compatToolName}\"");
                return null;
            }

            Debug.Log($"Found compat tool dir: {compatTool}");
            return compatTool;
        }

        /// <summary>
        ///     Determines whether the given filepath refers to an existing compatibility tool install dir.
        /// </summary>
        /// <param name="compatToolPath">An absolute path to a compatibility tool.</param>
        /// <returns><see langword="true" /> if the tool dir is valid, <see langword="false" /> otherwise.</returns>
        public static bool IsValidCompatToolPath(string? compatToolPath)
        {
            if (string.IsNullOrEmpty(compatToolPath))
            {
                return false;
            }

            // compatibilitytool.vdf is only included with compatibilitytools.d ones, not official ones
            // thus, toolmanifest.vdf is a better indicator
            var toolVdf = Path.Combine(compatToolPath!, "toolmanifest.vdf");
            var fileExists = File.Exists(toolVdf);
            Debug.Log($"File.Exists(\"{toolVdf}\") = {fileExists}");
            return fileExists;
        }

        /// <summary>
        ///     Locates the Steam root and finds the <c>compatibilitytools.d</c> directory.
        /// </summary>
        /// <returns>An absolute path to compatibilitytools.d, or <see langword="null" /> if it or the Steam root can't be found.</returns>
        public static string? GetCompatibilityToolsDotD()
        {
            var steamRoot = SteamLocator.FindSteamRoot();
            if (steamRoot == null || !SteamLocator.IsValidSteamRoot(steamRoot))
            {
                return null;
            }

            var dotD = Path.Combine(steamRoot, "compatibilitytools.d");
            return Directory.Exists(dotD) ? dotD : null;
        }

        /// <summary>
        ///     Interprets <paramref name="vdfContent" /> as a VDF and extracts the mapping between each AppId and its selected
        ///     compat tool name (empty string if unset/default).
        /// </summary>
        /// <remarks>
        ///     Taken from VRCX, MIT.
        /// </remarks>
        /// <param name="vdfContent"></param>
        /// <returns>A mapping between Steam AppIds and the name of the selected compat tool.</returns>
        private static Dictionary<string, string> ExtractCompatToolMapping(string vdfContent)
        {
            var compatToolMapping = new Dictionary<string, string>();
            const string sectionHeader = "\"CompatToolMapping\"";
            var sectionStart = vdfContent.IndexOf(sectionHeader, StringComparison.Ordinal);

            if (sectionStart == -1)
            {
                Debug.LogError("CompatToolMapping not found");
                return compatToolMapping;
            }

            var blockStart = vdfContent.IndexOf('{', sectionStart) + 1;
            var blockEnd = FindMatchingBracket(vdfContent, blockStart - 1);

            if (blockStart == -1 || blockEnd == -1)
            {
                Debug.LogError("CompatToolMapping block not found");
                return compatToolMapping;
            }

            var blockContent = vdfContent.Substring(blockStart, blockEnd - blockStart);

            // "123" { "crap" "blah" "name" "proton 67"
            // captures `123` and `proton 67`
            var keyValuePattern = new Regex("\"(\\d+)\"\\s*\\{[^}]*\"name\"\\s*\"([^\"]+)\"",
                RegexOptions.Multiline);

            var matches = keyValuePattern.Matches(blockContent);
            foreach (Match match in matches)
            {
                var key = match.Groups[1].Value;
                var name = match.Groups[2].Value;

                if (key != "0")
                {
                    compatToolMapping[key] = name;
                }
            }

            return compatToolMapping;
        }

        /// <summary>
        ///     Scan common locations for all compat tool directories and libraries, and return an absolute path to the tool that
        ///     matches the name or alias <paramref name="compatToolName" />.
        /// </summary>
        /// <param name="steamRoot">An absolute path to the Steam root directory.</param>
        /// <param name="compatToolName">
        ///     The name or alias of the compat tool, as listed in <c>config.vdf</c> or its <c>compatibilitytool.vdf</c>.
        /// </param>
        /// <returns>
        ///     An absolute path to the compat tool install dir, or <see langword="null" /> if it doesn't match a known
        ///     official name or can't be found.
        /// </returns>
        private static string? GetCompatToolDirForName(string steamRoot, string compatToolName)
        {
            var steamRootDotD = Path.Combine(steamRoot, "compatibilitytools.d");
            var foundDotD = FindCompatToolInDotD(steamRootDotD, compatToolName);

            var foundOfficial = FindOfficialCompatToolForName(steamRoot, compatToolName);

            // also look for system-installed protons.
            // I guess the majority of /usr paths come from folks getting protons from the AUR.
            var systemDotD = Path.Combine("usr", "share", "steam", "compatibilitytools.d");
            var foundSystemDotD = FindCompatToolInDotD(systemDotD, compatToolName);

            Debug.Log(
                $"Possibly matching compat tools: custom=\"{foundDotD}\", systemCustom=\"{foundSystemDotD}\", official=\"{foundOfficial}\"");

            // if the same tool is installed in more than one, I have no idea which one takes precedence in Steam.
            return foundDotD ?? foundSystemDotD ?? foundOfficial;
        }

        /// <summary>
        ///     Given the name of an official Proton version <paramref name="compatToolName" />, try to locate its install dir.
        /// </summary>
        /// <remarks>
        ///     If the given <paramref name="compatToolName" /> is the identifier or an alias for an official compatibility tool
        ///     that we recognize, knowing that it would be installed into a Steam library, check all known Steam libraries for a
        ///     compat tool whose directory matches the official <c>installDir</c>.
        /// </remarks>
        /// <param name="steamRoot">An absolute path to the Steam root directory.</param>
        /// <param name="compatToolName">
        ///     The name or alias of the compat tool, as listed in <c>config.vdf</c>.
        /// </param>
        /// <returns>
        ///     An absolute path to the compat tool install dir, or <see langword="null" /> if it doesn't match a known name
        ///     or isn't installed.
        /// </returns>
        private static string? FindOfficialCompatToolForName(string steamRoot, string compatToolName)
        {
            // look for official protons like "Proton 9.0 (Beta)" and "Proton 10.0"
            var matchingOfficialCompatTool = OfficialCompatToolData.All
                .FirstOrDefault(tool => tool.Name == compatToolName || tool.Aliases.Contains(compatToolName));

            // does it match an official tool name or an alias of one?
            if (matchingOfficialCompatTool == null)
                return null;

            Debug.Log(
                $"Matching \"{compatToolName}\" to official compat tool: {matchingOfficialCompatTool.Name}, checking" +
                $"Steam libraries for appid {matchingOfficialCompatTool.AppId}");

            // is it installed to one of our libraries?
            var libraryWithOfficial = SteamLocator.GetLibraryWithAppId(steamRoot, matchingOfficialCompatTool.AppId);
            Debug.Log($"Located in library: \"{libraryWithOfficial ?? "sorry, nothing"}\"");
            if (libraryWithOfficial == null)
                return null;

            var foundOfficial = Path.Combine(libraryWithOfficial, "steamapps", "common",
                matchingOfficialCompatTool.InstallDir);
            if (Directory.Exists(foundOfficial) &&
                // sanity check - all compat tools, custom or official, have this file in the installdir
                File.Exists(Path.Combine(foundOfficial, "toolmanifest.vdf")))
            {
                return foundOfficial;
            }

            return null;
        }

        /// <summary>
        ///     Given <paramref name="toolsDir" /> is an absolute filepath to a <c>compatibilitytools.d</c> directory, search
        ///     through its contents for a tool that matches the given name <paramref name="compatToolName" />, and return an
        ///     absolute filepath to it.
        /// </summary>
        /// <param name="toolsDir">
        ///     An absolute filepath to a <c>compatibilitytools.d</c> directory, whether in a Steam root or system-wide.
        /// </param>
        /// <param name="compatToolName">
        ///     The name of the compat tool, as listed in its <c>compatibilitytool.vdf</c>.
        /// </param>
        /// <returns>An absolute filepath to the compat tool's install dir, or <see langword="null" /> if it can't be found.</returns>
        private static string? FindCompatToolInDotD(string toolsDir, string compatToolName)
        {
            if (!Directory.Exists(toolsDir))
                return null;
            if (string.IsNullOrWhiteSpace(compatToolName))
                return null;

            // Several files inside the directory could indicate the compat tool name. But not all are reliable!
            // Q: the name of the directory itself?
            // A: NOPE - the name is "Proton-stl" but the installdir is "SteamTinkerLaunch"
            // Q: the contents of the "version" file?
            // A: NOPE - the name "GE-Proton10-33-rtsp23-zerocopy-test" has a version of "GE-Proton10-33-rtsp23-4-2-g2e8f1d695"
            // Q: the "internal name" inside "compatibilitytool.vdf"?
            // A: I think so! This is as expected in all my Proton versions.
            // The file "toolmanifest.vdf" has nothing relevant.

            return (from toolDir in Directory.GetDirectories(toolsDir)
                let compatToolVdfPath = Path.Combine(toolDir, "compatibilitytool.vdf")
                where File.Exists(compatToolVdfPath)
                let compatToolVdfContents = File.ReadAllText(compatToolVdfPath)
                // I could parse this file, but why bother lol
                where compatToolVdfContents.Contains(compatToolName)
                select toolDir).FirstOrDefault();
        }

        // Taken from VRCX, MIT
        private static int FindMatchingBracket(string content, int openBracketIndex)
        {
            var depth = 0;
            for (var i = openBracketIndex; i < content.Length; i++)
            {
                switch (content[i])
                {
                    case '{':
                        depth++;
                        break;
                    case '}':
                    {
                        depth--;
                        if (depth == 0)
                            return i;
                        break;
                    }
                }
            }

            Debug.LogError($"No matching bracket found in VDF starting from position {openBracketIndex}");
            return -1;
        }
    }
}