using Avalonia.Media;

namespace AvaloniaCharts.Controls;

public static class ColorHelper
{
    public static Color ParseColor(string hexOrName, byte alpha = 255)
    {
        if (string.IsNullOrWhiteSpace(hexOrName))
            return Color.FromArgb(alpha, 0, 0, 128);

        try
        {
            if (Color.TryParse(hexOrName, out Color parsed))
            {
                return Color.FromArgb(alpha, parsed.R, parsed.G, parsed.B);
            }
        }
        catch
        {
            // Игнорируем ошибку разбора и возвращаем fallback
        }

        return Color.FromArgb(alpha, 0, 0, 128);
    }
}
