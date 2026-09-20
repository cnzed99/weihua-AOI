using System.IO;

namespace HuarayCam;

internal static class HuarayRuntime
{
    private static readonly object Sync = new();
    private static bool initialized;

    public static void EnsureAvailable()
    {
        lock (Sync)
        {
            if (initialized) return;

            string? configured = Environment.GetEnvironmentVariable("HUARAY_MV_RUNTIME");
            string installed = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "HuarayTech",
                "MV Viewer",
                "Runtime",
                "x64"
            );
            string? runtime = new[] { configured, installed }
                .FirstOrDefault(candidate => !string.IsNullOrWhiteSpace(candidate)
                    && File.Exists(Path.Combine(candidate, "MVSDKmd.dll")));
            if (runtime is string resolvedRuntime)
            {
                string current = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
                if (!current.Split(';').Any(part =>
                    string.Equals(part.TrimEnd('\\'), resolvedRuntime.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase)))
                {
                    Environment.SetEnvironmentVariable("PATH", resolvedRuntime + ";" + current);
                }
            }

            initialized = true;
        }
    }
}
