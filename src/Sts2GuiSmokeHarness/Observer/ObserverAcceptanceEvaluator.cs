using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;

sealed class ObserverAcceptanceEvaluator
{
    public bool IsPhaseSatisfied(GuiSmokePhase phase, ObserverState observer, IReadOnlyList<GuiSmokeHistoryEntry>? history = null)
    {
        var sceneReady = observer.SceneReady != false;
        return phase switch
        {
            GuiSmokePhase.WaitMainMenu => sceneReady && string.Equals(observer.CurrentScreen, "main-menu", StringComparison.OrdinalIgnoreCase),
            GuiSmokePhase.WaitRunLoad => false,
            GuiSmokePhase.WaitCharacterSelect => sceneReady && string.Equals(observer.CurrentScreen, "character-select", StringComparison.OrdinalIgnoreCase),
            GuiSmokePhase.WaitMap => sceneReady && MapForegroundReconciliation.HasMapForegroundOwnership(observer, history ?? Array.Empty<GuiSmokeHistoryEntry>()),
            GuiSmokePhase.WaitPostMapNodeRoom => false,
            GuiSmokePhase.WaitCombat => CombatBarrierPolicy.IsStableCombatEntryObserver(observer),
            GuiSmokePhase.HandleCombat => false,
            _ => false,
        };
    }
}
