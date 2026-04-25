#nullable enable

using System.IO;
using UnityEngine;

namespace BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor.Locators
{
    public static class SteamLocator
    {
        /// <summary>
        ///     Search common paths for the Steam root directory, and if one is found, return an absolute path to it.
        /// </summary>
        /// <returns>An absolute path to the Steam root we found (if any), or <see langword="null" /> otherwise.</returns>
        public static string? FindSteamRoot()
        {
            var steamRoots = new[]
            {
                Path.Combine(XdgBaseDirectory.DataHome, "Steam"), // typically ~/.local/share/Steam
                Path.Combine(XdgBaseDirectory.Home, ".steam", "steam"),
                Path.Combine(XdgBaseDirectory.Home, ".steam", "root"),
                Path.Combine(XdgBaseDirectory.Home, ".steam", "debian-installation")
            };

            foreach (var steamRoot in steamRoots)
            {
                if (IsValidSteamRoot(steamRoot))
                    return steamRoot;
            }

            Debug.LogError("Couldn't find any Steam directory containing libraryfolders.vdf");
            return null;
        }

        /// <summary>
        ///     Given the path to Steam root, search <c>steamapps/libraryfolders.vdf</c> for the Steam library that a particular
        ///     <paramref name="appId" /> is installed to.
        /// </summary>
        /// <remarks>
        ///     Taken from VRCX, MIT.
        /// </remarks>
        /// <param name="steamRoot">An absolute path to the Steam root directory.</param>
        /// <param name="appId">The Steam AppId of the game.</param>
        /// <returns>
        ///     An absolute path to the library folder that should contain the app, or <see langword="null" /> if it wasn't
        ///     found.
        /// </returns>
        public static string? GetLibraryWithAppId(string steamRoot, string appId)
        {
            var libraryFoldersVdfPath = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");

            if (!File.Exists(libraryFoldersVdfPath))
            {
                Debug.LogWarning(
                    $"Attempted to search for appid, but was handed something that is probably not libraryfolders.vdf: {libraryFoldersVdfPath}");
                return null;
            }

            string? libraryPath = null;
            foreach (var line in File.ReadLines(libraryFoldersVdfPath))
            {
                // Assumes line will be \t\t"path"\t\t"pathToLibrary"
                if (line.Contains("\"path\""))
                {
                    var parts = line.Split("\t");
                    if (parts.Length < 4)
                        continue;

                    libraryPath = parts[4].Replace("\"", "");
                }

                if (!line.Contains($"\"{appId}\""))
                    continue;
                if (Directory.Exists(libraryPath))
                    return libraryPath;
                Debug.LogWarning(
                    $"libraryfolders.vdf references library \"{libraryPath}\", but that path doesn't exist");
                return null;
            }

            return null;
        }

        /// <summary>
        ///     Returns true if the given path appears to be a Steam root. We'll say a Steam root is a directory that contains a
        ///     <c>libraryfolders.vdf</c> file.
        /// </summary>
        /// <param name="steamRoot">An absolute path to the Steam root directory.</param>
        /// <returns><see langword="true" /> if the directory looks like a Steam root, <see langword="false" /> otherwise.</returns>
        public static bool IsValidSteamRoot(string? steamRoot)
        {
            if (string.IsNullOrEmpty(steamRoot) || !Directory.Exists(steamRoot))
            {
                return false;
            }

            var vdfPath = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
            return File.Exists(vdfPath);
        }
    }
}