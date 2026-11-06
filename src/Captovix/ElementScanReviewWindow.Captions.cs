using System.Windows.Media.Imaging;

namespace Captovix;

internal sealed partial class ElementScanReviewWindow
{
    private async Task RetryButtonCaptionsAsync(BitmapSource snapshot, ScanResult scan, string language, CancellationToken token)
    {
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(token);
        budget.CancelAfter(TimeSpan.FromSeconds(7));
        // Short labels can be missed or misread in sparse, full-image OCR.
        // Bound worker launches and keep the original result if the retry fails.
        var buttons = scan.Elements.Where(e => e.Kind == MockupKind.Button && e.Bounds.Width <= 520 &&
            e.Bounds.Height is >= 16 and <= 96 && (e.Text.Length == 0 || e.TextRuns.Count == 1 && e.Text.Length <= 12))
            .OrderBy(e => e.Text.Length != 0).Take(8);
        foreach (var button in buttons)
        {
            var local = button.Bounds with { X = button.Bounds.X - _region.X, Y = button.Bounds.Y - _region.Y };
            var interior = new PixelRect(local.X + 3, local.Y + 3, local.Width - 6, local.Height - 6)
                .Intersect(new(0, 0, snapshot.PixelWidth, snapshot.PixelHeight));
            if (interior.Width < 8 || interior.Height < 8) continue;
            try
            {
                var result = await OcrService.RecognizeAsync(snapshot, interior, language, 6, budget.Token);
                if (result.Error != null || result.Lines is not { Length: 1 }) continue;
                var line = result.Lines[0];
                var text = line.Text.Trim();
                if (line.Confidence < .70 || text.Length is 0 or > 80 || !text.Any(char.IsLetterOrDigit)) continue;
                var bounds = line.Bounds with { X = line.Bounds.X + interior.X + _region.X, Y = line.Bounds.Y + interior.Y + _region.Y };
                bounds = bounds.Intersect(button.Bounds);
                if (bounds.IsEmpty) continue;
                button.Text = text;
                button.TextRuns = [new(bounds, text, button.Colors.Foreground)];
                button.OcrBounds = bounds;
                button.FontSize = Math.Clamp(bounds.Height * .95, 8, 72);
                button.Reason += " Đã đọc lại chữ bên trong nút.";
            }
            catch (OperationCanceledException) when (!token.IsCancellationRequested) { break; }
        }
    }
}
