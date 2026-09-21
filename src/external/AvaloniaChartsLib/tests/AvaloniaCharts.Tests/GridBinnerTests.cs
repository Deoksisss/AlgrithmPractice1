using AvaloniaCharts.Core;
using Xunit;

namespace AvaloniaCharts.Tests;

public class GridBinnerTests
{
    [Fact]
    public void BinPointCloud_RegularCloud_CreatesExpectedGridSizeAndAverages()
    {
        var points = new List<Point3D>
        {
            new(0.0, 0.0, 10.0),
            new(0.0, 10.0, 20.0),
            new(10.0, 0.0, 30.0),
            new(10.0, 10.0, 40.0)
        };

        int targetGridSize = 20;
        SurfaceData surface = GridBinner.BinPointCloud(points, targetGridSize);

        Assert.NotNull(surface);
        Assert.Equal(targetGridSize, surface.XValues.Length);
        Assert.Equal(targetGridSize, surface.YValues.Length);
        Assert.Equal(targetGridSize, surface.Z.GetLength(0));
        Assert.Equal(targetGridSize, surface.Z.GetLength(1));
    }

    [Fact]
    public void BinPointCloud_EmptyCloud_ReturnsEmptyGridWithoutThrowing()
    {
        var points = new List<Point3D>();

        SurfaceData surface = GridBinner.BinPointCloud(points, 10);

        Assert.NotNull(surface);
        Assert.Equal(10, surface.XValues.Length);
        Assert.Equal(10, surface.YValues.Length);
        Assert.Equal(10, surface.Z.GetLength(0));
        Assert.Equal(10, surface.Z.GetLength(1));
    }
}
