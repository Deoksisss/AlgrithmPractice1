using Avalonia.Media;
using AvaloniaCharts.Controls;
using AvaloniaCharts.Core;
using Xunit;

namespace AvaloniaCharts.Tests;

public class MultiSeriesTests
{
    [Fact]
    public void ColorHelper_ParsesHexAndNames_ReturnsValidColor()
    {
        Color red = ColorHelper.ParseColor("#FF0000");
        Assert.Equal(255, red.R);
        Assert.Equal(0, red.G);
        Assert.Equal(0, red.B);

        Color yellow = ColorHelper.ParseColor("#FFC107");
        Assert.Equal(255, yellow.R);
        Assert.Equal(193, yellow.G);
        Assert.Equal(7, yellow.B);

        Color withAlpha = ColorHelper.ParseColor("#00FF00", 128);
        Assert.Equal(128, withAlpha.A);
        Assert.Equal(0, withAlpha.R);
        Assert.Equal(255, withAlpha.G);
    }

    [Fact]
    public void AxisScale_MultiSeriesCombinedPoints_CalculatesCorrectRange()
    {
        var series1Points = new List<Point2D> { new(0, -10), new(5, 20) };
        var series2Points = new List<Point2D> { new(-5, 0), new(15, 50) };

        var allX = series1Points.Select(p => p.X).Concat(series2Points.Select(p => p.X));
        var allY = series1Points.Select(p => p.Y).Concat(series2Points.Select(p => p.Y));

        var scaleX = AxisScale.Calculate(allX);
        var scaleY = AxisScale.Calculate(allY);

        Assert.True(scaleX.Min <= -5.0);
        Assert.True(scaleX.Max >= 15.0);
        Assert.True(scaleY.Min <= -10.0);
        Assert.True(scaleY.Max >= 50.0);
    }
}
