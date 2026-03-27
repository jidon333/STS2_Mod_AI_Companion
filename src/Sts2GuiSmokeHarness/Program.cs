using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Sts2ModKit.Core.Configuration;

internal static partial class Program
{
    static async Task<int> Main(string[] args)
    {
        var command = args.Length == 0 ? "help" : args[0].ToLowerInvariant();
        var options = ParseOptions(args.Skip(1).ToArray());
        var workspaceRoot = Directory.GetCurrentDirectory();
        var configPath = ResolveConfigPath(options, workspaceRoot);
        var loadResult = ConfigurationLoader.LoadFromFile(configPath);
        var configuration = ApplyPathOverrides(loadResult.Configuration, options);

        try
        {
            switch (command)
            {
                case "run":
                    return await RunScenarioAsync(configuration, workspaceRoot, options).ConfigureAwait(false);

                case "inspect-run":
                    return InspectRun(options, workspaceRoot);

                case "inspect-session":
                    return InspectSession(options, workspaceRoot);

                case "replay-step":
                    return ReplayStep(options, workspaceRoot);

                case "replay-test":
                    return ReplayGoldenScenes(options, workspaceRoot);

                case "replay-parity-test":
                    return ReplayParityScenes(options, workspaceRoot);

                case "self-test":
                    RunSelfTest();
                    Console.WriteLine("GUI smoke harness self-test passed.");
                    return 0;

                default:
                    WriteUsage();
                    return 0;
            }
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }
}
