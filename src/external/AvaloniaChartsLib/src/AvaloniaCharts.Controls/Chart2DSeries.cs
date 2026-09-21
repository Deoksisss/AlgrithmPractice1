using AvaloniaCharts.Core;

namespace AvaloniaCharts.Controls;

/// <summary>
/// Набор данных и параметров стилизации для одного 2D графика.
/// </summary>
public sealed class Chart2DSeries
{
    /// <summary>
    /// Название графика для сноски/легенды.
    /// </summary>
    public string Name { get; set; } = "График";

    /// <summary>
    /// Точки 2D графика.
    /// </summary>
    public IReadOnlyList<Point2D> Points { get; set; } = Array.Empty<Point2D>();

    /// <summary>
    /// Цвет линии в формате HEX (например, "#FF5733", "#007ACC", "#FFFF00") или имя цвета.
    /// </summary>
    public string ColorHex { get; set; } = "#000080";

    /// <summary>
    /// Тип линии (сплошная, прерывистая, пунктирная).
    /// </summary>
    public LineStyle LineStyle { get; set; } = LineStyle.Solid;

    /// <summary>
    /// Толщина линии в пикселях.
    /// </summary>
    public double LineThickness { get; set; } = 2.0;
}
