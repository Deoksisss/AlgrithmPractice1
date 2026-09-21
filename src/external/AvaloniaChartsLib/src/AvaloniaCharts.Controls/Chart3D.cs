using Avalonia.Controls;
using AvaloniaCharts.Core;

namespace AvaloniaCharts.Controls;

/// <summary>
/// Публичное API для создания и отображения 3D графиков поверхностей.
/// </summary>
public sealed class Chart3D
{
    private readonly Chart3DControl _control = new();
    private Chart3DParams _params = new();

    public void SetParams(Chart3DParams parameters)
    {
        _params = parameters ?? new Chart3DParams();
        _control.SetParams(_params);
    }

    public void SetData(SurfaceData data)
    {
        _control.SetData(data);
    }

    public void SetData(IReadOnlyList<Point3D> cloud)
    {
        SurfaceData binned = GridBinner.BinPointCloud(cloud);
        _control.SetData(binned);
    }

    public void SetSeries(IEnumerable<Chart3DSeries> series)
    {
        _control.SetSeries(series);
    }

    public void AddSeries(Chart3DSeries series)
    {
        _control.AddSeries(series);
    }

    public void Run()
    {
        ChartRunner.Launch(_control, string.IsNullOrEmpty(_params.WindowTitle) ? "Chart 3D" : _params.WindowTitle);
    }

    public Control AsControl() => _control;
}
