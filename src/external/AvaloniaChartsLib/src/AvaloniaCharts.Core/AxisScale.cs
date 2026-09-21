namespace AvaloniaCharts.Core;

/// <summary>
/// Результат расчёта шкалы и делений.
/// </summary>
public readonly record struct ScaleResult(double Min, double Max, double Step, double[] Ticks);

/// <summary>
/// Расчёт минимального, максимального значения и "красивых" делений шкалы (алгоритм {1, 2, 5} × 10^n).
/// </summary>
public static class AxisScale
{
    /// <summary>
    /// Вычисляет границы и деления для указанного набора значений.
    /// </summary>
    public static ScaleResult Calculate(IEnumerable<double> values, int targetTickCount = 5, double paddingRatio = 0.05)
    {
        double min = double.MaxValue;
        double max = double.MinValue;
        bool hasValidData = false;

        if (values != null)
        {
            foreach (var val in values)
            {
                if (double.IsNaN(val) || double.IsInfinity(val))
                    continue;

                if (val < min) min = val;
                if (val > max) max = val;
                hasValidData = true;
            }
        }

        if (!hasValidData)
        {
            return new ScaleResult(-1.0, 1.0, 0.5, new[] { -1.0, -0.5, 0.0, 0.5, 1.0 });
        }

        if (Math.Abs(max - min) < 1e-9)
        {
            double center = min;
            double padding = Math.Abs(center) > 1e-9 ? Math.Abs(center) * 0.1 : 1.0;
            if (padding < 1e-9) padding = 1.0;

            min = center - padding;
            max = center + padding;
        }
        else
        {
            double span = max - min;
            double padding = span * Math.Clamp(paddingRatio, 0.01, 0.2);
            min -= padding;
            max += padding;
        }

        double rawStep = (max - min) / Math.Max(1, targetTickCount);
        double step = CalculateNiceStep(rawStep);

        double niceMin = Math.Floor(min / step) * step;
        double niceMax = Math.Ceiling(max / step) * step;

        var ticks = new List<double>();
        for (double t = niceMin; t <= niceMax + step * 0.001; t += step)
        {
            double roundedTick = Math.Round(t, 10);
            ticks.Add(roundedTick);
        }

        return new ScaleResult(niceMin, niceMax, step, ticks.ToArray());
    }

    /// <summary>
    /// Вычисляет ближайший "красивый" шаг делений из набора {1, 2, 5} × 10^n.
    /// </summary>
    public static double CalculateNiceStep(double rawStep)
    {
        if (rawStep <= 0 || double.IsNaN(rawStep) || double.IsInfinity(rawStep))
            return 1.0;

        double exponent = Math.Floor(Math.Log10(rawStep));
        double fraction = rawStep / Math.Pow(10, exponent);

        double niceFraction;
        if (fraction <= 1.0 + 1e-9)
            niceFraction = 1.0;
        else if (fraction <= 2.0 + 1e-9)
            niceFraction = 2.0;
        else if (fraction <= 5.0 + 1e-9)
            niceFraction = 5.0;
        else
            niceFraction = 10.0;

        return niceFraction * Math.Pow(10, exponent);
    }
}
