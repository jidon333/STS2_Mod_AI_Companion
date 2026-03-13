using System.Reflection;
using Sts2ModKit.Core.Harness;

namespace Sts2ModAiCompanion.HarnessBridge;

public static class HarnessBridgeEntryPoint
{
    private static readonly object Sync = new();
    private static HarnessBridgeHost? _host;

    public static string BridgeVersion => "0.2.0-alpha";

    public static bool Initialize(HarnessQueueLayout layout, int pollIntervalMs)
    {
        lock (Sync)
        {
            if (_host is not null)
            {
                return true;
            }

            _host = new HarnessBridgeHost(layout, pollIntervalMs);
            _host.Start();
            return true;
        }
    }

    internal static void ActivateHarnessSession()
    {
        TryEnableTestMode();
    }

    private static void TryEnableTestMode()
    {
        try
        {
            var testModeType = FindLoadedType("MegaCrit.Sts2.Core.TestSupport.TestMode");
            if (testModeType is not null)
            {
                var turnOnInternal = testModeType.GetMethod("TurnOnInternal", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (turnOnInternal is not null)
                {
                    turnOnInternal.Invoke(null, null);
                }
                else
                {
                    var isOnProperty = testModeType.GetProperty("IsOn", BindingFlags.Public | BindingFlags.Static);
                    isOnProperty?.SetValue(null, true);
                }
            }

            var saveManagerType = FindLoadedType("MegaCrit.Sts2.Core.Saves.SaveManager");
            var saveManager = saveManagerType?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            if (saveManager is not null)
            {
                saveManagerType?.GetMethod("SetFtuesEnabled", BindingFlags.Public | BindingFlags.Instance)?.Invoke(saveManager, new object[] { false });
                saveManagerType?.GetMethod("MarkFtueAsComplete", BindingFlags.Public | BindingFlags.Instance)?.Invoke(saveManager, new object[] { "accept_tutorials_ftue" });
            }
        }
        catch
        {
            // Test-only best effort. Harness can proceed even if runtime test mode
            // toggles are unavailable in the current game build.
        }
    }

    private static Type? FindLoadedType(string fullName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var type = assembly.GetType(fullName, throwOnError: false, ignoreCase: false);
                if (type is not null)
                {
                    return type;
                }
            }
            catch
            {
                // Ignore assembly reflection failures during best-effort lookup.
            }
        }

        return null;
    }
}
