#nullable enable
using System.Diagnostics;

namespace BefuddledLabs.LinuxVRChatSdkPatch.Base.Editor.Locators
{
    public static class SteamRunDetector
    {
        private static bool? _isInstalled;
        public static bool IsInstalled => _isInstalled ??= CheckIsInstalled();

        private static bool CheckIsInstalled()
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo("sh", "-c \"command -v steam-run --help\"")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                if (process == null)
                    return false;

                if (process.WaitForExit(1000)) return process.ExitCode == 0;
                process.Kill();
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}