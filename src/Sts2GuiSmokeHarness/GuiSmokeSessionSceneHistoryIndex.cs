using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModAiCompanion.Mod;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;
using static GuiSmokeChoicePrimitiveSupport;

sealed class GuiSmokeSessionSceneHistoryIndex
{
    private readonly HashSet<string> _seenSignatures = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<KnownRecipeHint>> _knownRecipesByKey = new(StringComparer.Ordinal);

    public static GuiSmokeSessionSceneHistoryIndex Load(string sessionRoot)
    {
        var index = new GuiSmokeSessionSceneHistoryIndex();
        index.LoadFromSessionArtifacts(sessionRoot);
        return index;
    }

    public bool HasSeen(string sceneSignature)
    {
        return _seenSignatures.Contains(sceneSignature);
    }

    public IReadOnlyList<KnownRecipeHint> GetKnownRecipes(string sceneSignature, string phase)
    {
        return _knownRecipesByKey.TryGetValue(BuildRecipeKey(sceneSignature, phase), out var hints)
            ? hints
            : Array.Empty<KnownRecipeHint>();
    }

    public void NoteRecipe(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        if (string.IsNullOrWhiteSpace(request.SceneSignature)
            || string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase)
            || string.Equals(decision.Status, "abort", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(decision.ActionKind))
        {
            return;
        }

        _seenSignatures.Add(request.SceneSignature);
        var key = BuildRecipeKey(request.SceneSignature, request.Phase);
        if (!_knownRecipesByKey.TryGetValue(key, out var hints))
        {
            hints = new List<KnownRecipeHint>(capacity: 3);
            _knownRecipesByKey[key] = hints;
        }

        hints.Add(new KnownRecipeHint(
            request.SceneSignature,
            request.Phase,
            decision.ActionKind!,
            decision.TargetLabel,
            decision.ExpectedScreen,
            decision.Reason));

        if (hints.Count > 3)
        {
            hints.RemoveRange(0, hints.Count - 3);
        }
    }

    public void NoteUnknownScene(GuiSmokeStepRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SceneSignature))
        {
            return;
        }

        _seenSignatures.Add(request.SceneSignature);
    }

    private void LoadFromSessionArtifacts(string sessionRoot)
    {
        LoadRecipes(Path.Combine(sessionRoot, "scene-recipes.ndjson"));
        LoadUnknownScenes(Path.Combine(sessionRoot, "unknown-scenes.ndjson"));
    }

    private void LoadRecipes(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            SceneRecipeEntry? entry;
            try
            {
                entry = JsonSerializer.Deserialize<SceneRecipeEntry>(line, GuiSmokeShared.JsonOptions);
            }
            catch (JsonException)
            {
                continue;
            }

            if (entry is null)
            {
                continue;
            }

            _seenSignatures.Add(entry.SceneSignature);
            var key = BuildRecipeKey(entry.SceneSignature, entry.Phase);
            if (!_knownRecipesByKey.TryGetValue(key, out var hints))
            {
                hints = new List<KnownRecipeHint>(capacity: 3);
                _knownRecipesByKey[key] = hints;
            }

            hints.Add(new KnownRecipeHint(
                entry.SceneSignature,
                entry.Phase,
                entry.ActionKind,
                entry.TargetLabel,
                entry.ExpectedScreen,
                entry.Reason));

            if (hints.Count > 3)
            {
                hints.RemoveRange(0, hints.Count - 3);
            }
        }
    }

    private void LoadUnknownScenes(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            UnknownSceneEntry? entry;
            try
            {
                entry = JsonSerializer.Deserialize<UnknownSceneEntry>(line, GuiSmokeShared.JsonOptions);
            }
            catch (JsonException)
            {
                continue;
            }

            if (entry is not null && !string.IsNullOrWhiteSpace(entry.SceneSignature))
            {
                _seenSignatures.Add(entry.SceneSignature);
            }
        }
    }

    private static string BuildRecipeKey(string sceneSignature, string phase)
    {
        return string.Concat(sceneSignature, "::", phase);
    }
}
