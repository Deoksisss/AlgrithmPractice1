namespace AvaloniaCharts.Controls;

/// <summary>
/// Параметры отображения трехмерного графика (поверхности).
/// </summary>
public sealed class Chart3DParams
{
    public string Title { get; set; } = string.Empty;
    public string XLabel { get; set; } = "X";
    public string YLabel { get; set; } = "Y";
    public string ZLabel { get; set; } = "Z";
    public string WindowTitle { get; set; } = "Chart 3D";
}
