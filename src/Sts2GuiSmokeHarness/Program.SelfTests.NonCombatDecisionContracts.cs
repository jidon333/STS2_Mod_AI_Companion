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

internal static partial class Program
{
    private static void RunNonCombatDecisionContractsSelfTests()
    {
        var rewardRankingScreenshotPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-reward-ranking-self-test-{Guid.NewGuid():N}.png");
        try
        {
            using (var bitmap = new Bitmap(1280, 720))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var panelBrush = new SolidBrush(Color.FromArgb(222, 205, 168)))
            using (var arrowBrush = new SolidBrush(Color.FromArgb(220, 60, 55)))
            {
                graphics.Clear(Color.Black);
                graphics.FillRectangle(panelBrush, new Rectangle(420, 180, 480, 320));
                graphics.FillEllipse(arrowBrush, new Rectangle(595, 450, 90, 70));
                bitmap.Save(rewardRankingScreenshotPath, ImageFormat.Png);
            }

            RunNonCombatRewardDecisionContractSelfTests(rewardRankingScreenshotPath);
            RunNonCombatSubtypeDecisionContractSelfTests(rewardRankingScreenshotPath);
        }
        finally
        {
            if (File.Exists(rewardRankingScreenshotPath))
            {
                File.Delete(rewardRankingScreenshotPath);
            }
        }
    }
}
