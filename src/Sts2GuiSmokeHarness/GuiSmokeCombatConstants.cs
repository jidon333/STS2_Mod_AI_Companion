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

static class GuiSmokeCombatConstants
{
    public const double NonEnemyPrimeNormalizedX = 0.280;
    public const double NonEnemyPrimeNormalizedY = 0.620;
    public const double NonEnemyConfirmNormalizedX = 0.500;
    public const double NonEnemyConfirmNormalizedY = 0.560;
    public const int NonEnemyConfirmHoldMs = 75;
    public const double AttackConfirmNormalizedX = 0.500;
    public const double AttackConfirmNormalizedY = 0.560;
    public const int AttackConfirmPrimeMs = 90;
    public const int AttackConfirmHoldMs = 120;
    public const double HandSelectionConfirmNormalizedX = 0.952;
    public const double HandSelectionConfirmNormalizedY = 0.734;
}
