using AvaloniaCharts.Controls;

namespace AlgorithmPractice1.GUI.Models;

public record ChartDisplayModel
{
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public string AlgorithmId { get; init; } = string.Empty;
    public bool Is3D { get; init; }

    public IReadOnlyList<Chart2DSeries>? Series2D { get; init; }
    public Chart2DParams? Params2D { get; init; }

    public IReadOnlyList<Chart3DSeries>? Series3D { get; init; }
    public Chart3DParams? Params3D { get; init; }
}
