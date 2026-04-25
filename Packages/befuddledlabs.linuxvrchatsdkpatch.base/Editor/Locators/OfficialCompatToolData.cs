#nullable enable

namespace BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor.Locators
{
    /// <summary>
    ///     Data extracted from appcache.vdf on 2026-04-17 using a one-off script using new-vdf-parser[1]
    ///     [1]: https://crates.io/crates/new-vdf-parser
    /// </summary>
    public static class OfficialCompatToolData
    {
        private static readonly CompatTool Proton9 = new(
            "proton_9",
            "2805730",
            "proton-9.0-4pin".Split(","),
            "Proton 9.0 (Beta)"
        );

        private static readonly CompatTool Proton10 = new(
            "proton_10",
            "3658110",
            "proton-9,proton-9.0-1RC,proton-stable,proton-next,proton_next,proton-7.0-1,proton-7.0-2,proton-7.0-3,proton-7.0-4,proton-7.0-5,proton-7.0-6,proton-8.0-1,proton-8.0-2,proton-8.0-3,proton-8.0-4,proton-8.0-5,proton-8.0RC,proton-9.0-2RC,proton-9.0-3RC,proton-9.0-4RC,proton-10,proton-10.0-beta,proton-10.0-3RC"
                .Split(","),
            "Proton 10.0"
        );

        private static readonly CompatTool Proton11 = new(
            "proton_11",
            "4628710",
            "proton-11.0-beta".Split(","),
            "Proton 11.0"
        );

        private static readonly CompatTool ProtonHotfix = new(
            "proton_hotfix",
            "2180100",
            "proton-hotfix".Split(","),
            "Proton Hotfix"
        );

        private static readonly CompatTool ProtonExperimental = new(
            "proton_experimental",
            "1493710",
            "proton-experimental".Split(","),
            "Proton - Experimental"
        );

        public static readonly CompatTool[] All = { Proton9, Proton10, Proton11, ProtonHotfix, ProtonExperimental };
    }

    /// <summary>
    ///     Stores information about official compatibility tools that get installed to Steam libraries.
    /// </summary>
    public class CompatTool
    {
        public CompatTool(string name, string appId, string[] aliases, string installDir)
        {
            Name = name;
            AppId = appId;
            Aliases = aliases;
            InstallDir = installDir;
        }

        /// <summary>
        ///     The name of the tool as it appears in <c>CompatToolMapping</c>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The Steam AppId of this tool.
        /// </summary>
        public string AppId { get; }

        /// <summary>
        ///     A list of other (possibly older) names that this tool may also appear in <c>CompatToolMapping</c>.
        /// </summary>
        public string[] Aliases { get; }

        /// <summary>
        ///     The name of the directory within a Steam library folder that this tool will be installed in.
        /// </summary>
        public string InstallDir { get; }
    }
}