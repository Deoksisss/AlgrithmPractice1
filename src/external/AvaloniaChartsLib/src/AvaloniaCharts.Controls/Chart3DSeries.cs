using AvaloniaCharts.Core;

namespace AvaloniaCharts.Controls;

/// <summary>
/// Набор данных и параметров стилизации для одной 3D поверхности.
/// </summary>
public sealed class Chart3DSeries
{
    /// <summary>
    /// Название поверхности для сноски/легенды.
    /// </summary>
    public string Name { get; set; } = "Поверхность";

    /// <summary>
    /// Данные регулярной сетки X x Y с высотами Z.
    /// </summary>
    public SurfaceData? Data { get; set; }

    /// <summary>
    /// Произвольное облако точек 3D (автоматически биннится в сетку, если Data не задано).
    /// </summary>
    public IReadOnlyList<Point3D>? PointCloud { get; set; }

    /// <summary>
    /// Цвет поверхности в формате HEX (например, "#FF5733", "#007ACC", "#FFFF00").
    /// </summary>
    public string ColorHex { get; set; } = "#007ACC";

    /// <summary>
    /// Стиль отображения поверхности (сплошная, сетка или сплошная с сеткой).
    /// </summary>
    public SurfaceStyle Style { get; set; } = SurfaceStyle.SolidWithWireframe;

    /// <summary>
    /// Прозрачность поверхности от 0.0 (прозрачная) до 1.0 (непрозрачная).
    /// </summary>
    public double Opacity { get; set; } = 0.75;
}
