using Avalonia.Controls;
using AvaloniaCharts.Core;

namespace AvaloniaCharts.Controls;

/// <summary>
/// Публичное API для создания и отображения 2D графиков.
/// </summary>
public sealed class Chart2D
{
    private readonly Chart2DControl _control = new();
    private Chart2DParams _params = new();

    public void SetParams(Chart2DParams parameters)
    {
        _params = parameters ?? new Chart2DParams();
        _control.SetParams(_params);
    }

    public void SetData(IReadOnlyList<Point2D> points)
    {
        _control.SetData(points);
    }

    public void SetSeries(IEnumerable<Chart2DSeries> series)
    {
        _control.SetSeries(series);
    }

    public void AddSeries(Chart2DSeries series)
    {
        _control.AddSeries(series);
    }

    public void Run()
    {
        ChartRunner.Launch(_control, string.IsNullOrEmpty(_params.WindowTitle) ? "Chart" : _params.WindowTitle);
    }

    public Control AsControl() => _control;
}
