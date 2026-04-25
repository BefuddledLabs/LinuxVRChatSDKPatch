#nullable enable

using System;
using System.IO;

namespace BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor.Locators
{
    /// <summary>
    ///     Uses the XDG Base Directory Specification to find various user directories.
    /// </summary>
    /// <remarks>
    ///     Pared down from <see href="https://github.com/xdg-net/Xdg.Directories">Xdg.Directories</see> library, MIT.
    /// </remarks>
    public static class XdgBaseDirectory
    {
        /// <summary>
        ///     The user's home directory.
        /// </summary>
        public static string Home => Environment.GetEnvironmentVariable("HOME") ??
                                     Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        // TODO: should also read ${XDG_CONFIG_HOME:-$HOME/.config}/user-dirs.dirs
        /// <summary>
        ///     The user's data files directory (e.g. <c>~/.local/share</c>).
        /// </summary>
        public static string DataHome =>
            Environment.GetEnvironmentVariable("XDG_DATA_HOME")
            ?? Path.Combine(Home, ".local", "share");
    }
}