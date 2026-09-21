namespace AvaloniaCharts.Controls;

/// <summary>
/// Стиль отображения 3D поверхности.
/// </summary>
public enum SurfaceStyle
{
    /// <summary>
    /// Сплошная залитая поверхность.
    /// </summary>
    Solid,

    /// <summary>
    /// Каркасная сетка (без заливки граней).
    /// </summary>
    Wireframe,

    /// <summary>
    /// Сплошная поверхность с наложенной сеткой.
    /// </summary>
    SolidWithWireframe
}
