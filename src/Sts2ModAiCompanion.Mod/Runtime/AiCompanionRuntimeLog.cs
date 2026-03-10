using System.Reflection;

namespace Sts2ModAiCompanion.Mod.Runtime;

internal static class AiCompanionRuntimeLog
{
    private static readonly string ModDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppContext.BaseDirectory;

    public static void WriteLine(string message)
    {
        var formatted = $"[STS2 Mod AI Companion] {message}";
        Console.WriteLine(formatted);

        try
        {
            var logPath = Path.Combine(ModDirectory, AiCompanionRuntimeState.RuntimeLogFileName);
            File.AppendAllText(logPath, formatted + Environment.NewLine);
        }
        catch
        {
            // Keep the runtime resilient even if file logging is unavailable.
        }
    }
}
