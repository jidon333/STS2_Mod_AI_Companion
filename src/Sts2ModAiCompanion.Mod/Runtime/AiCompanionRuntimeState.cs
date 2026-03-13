using System.Reflection;

namespace Sts2ModAiCompanion.Mod.Runtime;

internal static class AiCompanionRuntimeState
{
    private static readonly object Sync = new();
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMilliseconds(500);
    private static readonly string ModDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppContext.BaseDirectory;
    private static AiCompanionRuntimeConfig? _cachedConfig;
    private static DateTimeOffset _lastLoadedAt = DateTimeOffset.MinValue;
    private static DateTime _lastObservedConfigWriteUtc = DateTime.MinValue;

    public const string RuntimeConfigFileName = "sts2-mod-ai-companion.config.json";

    public const string RuntimeLogFileName = "sts2-mod-ai-companion.runtime.log";

    public static string GetModDirectory() => ModDirectory;

    public static AiCompanionRuntimeConfig GetConfig(bool forceRefresh = false)
    {
        var cached = _cachedConfig;
        if (!forceRefresh
            && cached is not null
            && DateTimeOffset.UtcNow - _lastLoadedAt < RefreshInterval)
        {
            return cached;
        }

        lock (Sync)
        {
            cached = _cachedConfig;
            if (!forceRefresh
                && cached is not null
                && DateTimeOffset.UtcNow - _lastLoadedAt < RefreshInterval)
            {
                return cached;
            }

            var configWriteUtc = GetConfigWriteUtc();
            if (!forceRefresh
                && cached is not null
                && configWriteUtc == _lastObservedConfigWriteUtc)
            {
                _lastLoadedAt = DateTimeOffset.UtcNow;
                return cached;
            }

            _cachedConfig = AiCompanionRuntimeConfigLoader.Load(ModDirectory);
            _lastLoadedAt = DateTimeOffset.UtcNow;
            _lastObservedConfigWriteUtc = configWriteUtc;
            AiCompanionRuntimeLog.WriteLine($"runtime config refreshed: enabled={_cachedConfig.Enabled}");
            return _cachedConfig;
        }
    }

    private static DateTime GetConfigWriteUtc()
    {
        var configPath = Path.Combine(ModDirectory, RuntimeConfigFileName);
        return File.Exists(configPath)
            ? File.GetLastWriteTimeUtc(configPath)
            : DateTime.MinValue;
    }
}
