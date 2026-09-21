using AvaloniaCharts.Core;
using Xunit;

namespace AvaloniaCharts.Tests;

public class AxisScaleTests
{
    [Fact]
    public void Calculate_NormalValues_ReturnsExpectedTicksAndRange()
    {
        double[] values = new[] { 0.0, 2.5, 5.0, 7.5, 10.0 };

        var result = AxisScale.Calculate(values);

        Assert.True(result.Min <= 0.0, $"Expected Min <= 0, got {result.Min}");
        Assert.True(result.Max >= 10.0, $"Expected Max >= 10, got {result.Max}");
        Assert.True(result.Step > 0, "Step must be positive");
        Assert.NotEmpty(result.Ticks);
    }

    [Fact]
    public void Calculate_SingleValue_ReturnsFixedPadding()
    {
        double[] values = new[] { 42.0 };

        var result = AxisScale.Calculate(values);

        Assert.True(result.Min < 42.0);
        Assert.True(result.Max > 42.0);
        Assert.Contains(42.0, result.Ticks);
    }

    [Fact]
    public void Calculate_EmptyCollection_ReturnsDefaultRange()
    {
        double[] values = Array.Empty<double>();

        var result = AxisScale.Calculate(values);

        Assert.Equal(-1.0, result.Min);
        Assert.Equal(1.0, result.Max);
        Assert.Equal(0.5, result.Step);
        Assert.NotEmpty(result.Ticks);
    }

    [Fact]
    public void Calculate_NaNAndInfinityValues_FiltersInvalidEntries()
    {
        double[] values = new[] { double.NaN, 5.0, double.PositiveInfinity, 15.0, double.NegativeInfinity };

        var result = AxisScale.Calculate(values);

        Assert.False(double.IsNaN(result.Min));
        Assert.False(double.IsNaN(result.Max));
        Assert.True(result.Min <= 5.0);
        Assert.True(result.Max >= 15.0);
    }

    [Theory]
    [InlineData(0.3, 0.5)]
    [InlineData(1.2, 2.0)]
    [InlineData(4.5, 5.0)]
    [InlineData(8.0, 10.0)]
    public void CalculateNiceStep_ReturnsNiceStepInSet(double rawStep, double expectedNiceStep)
    {
        double step = AxisScale.CalculateNiceStep(rawStep);
        Assert.Equal(expectedNiceStep, step, precision: 5);
    }
}
