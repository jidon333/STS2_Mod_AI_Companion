using System.Drawing;
using System.Globalization;

internal static class GuiSmokeChoicePrimitiveSupport
{
    internal static bool ContainsAny(string? value, params string[] candidates)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return candidates.Any(candidate => value.Contains(candidate, StringComparison.OrdinalIgnoreCase));
    }

    internal static bool IsOverlayChoiceLabel(string? label)
    {
        return ContainsAny(label, "Backstop", "LeftArrow", "RightArrow");
    }

    internal static bool IsSkipOrProceedLabel(string? label)
    {
        return IsSkipLikeLabel(label)
               || IsProceedLikeLabel(label)
               || IsConfirmLikeLabel(label);
    }

    internal static bool IsSkipLikeLabel(string? label)
    {
        return ContainsAny(label, "Skip", "건너", "넘기");
    }

    internal static bool IsProceedLikeLabel(string? label)
    {
        return ContainsAny(label, "Proceed", "Continue", "진행", "계속");
    }

    internal static bool IsConfirmLikeLabel(string? label)
    {
        return ContainsAny(label, "Confirm", "확인", "선택");
    }

    internal static bool IsDismissLikeLabel(string? label)
    {
        return ContainsAny(label, "Cancel", "Close", "닫기", "취소", "Back");
    }

    internal static bool IsInspectPreviewBounds(string? screenBounds)
    {
        return TryParseBounds(screenBounds, out var bounds)
               && bounds.Width <= 120f
               && bounds.Height <= 120f
               && bounds.Y <= 170f
               && bounds.X <= 200f;
    }

    internal static bool HasLargeChoiceBounds(string? screenBounds)
    {
        return TryParseBounds(screenBounds, out var bounds)
               && bounds.Width >= 260f
               && bounds.Height >= 60f;
    }

    private static bool TryParseBounds(string? raw, out RectangleF bounds)
    {
        bounds = default;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var parts = raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4)
        {
            return false;
        }

        if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
            || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width)
            || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
        {
            return false;
        }

        if (width <= 0f || height <= 0f)
        {
            return false;
        }

        bounds = new RectangleF(x, y, width, height);
        return true;
    }
}
