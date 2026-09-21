namespace AvaloniaCharts.Core;

/// <summary>
/// Утилита для биннинга произвольного облака 3D-точек в регулярную сетку N x N (SurfaceData).
/// </summary>
public static class GridBinner
{
    /// <summary>
    /// Преобразует облако точек Point3D в сетку SurfaceData размера gridSize x gridSize.
    /// </summary>
    public static SurfaceData BinPointCloud(IReadOnlyList<Point3D> points, int gridSize = 50)
    {
        if (gridSize < 2) gridSize = 2;

        var validPoints = points?
            .Where(p => !double.IsNaN(p.X) && !double.IsInfinity(p.X) &&
                        !double.IsNaN(p.Y) && !double.IsInfinity(p.Y) &&
                        !double.IsNaN(p.Z) && !double.IsInfinity(p.Z))
            .ToList() ?? new List<Point3D>();

        if (validPoints.Count == 0)
        {
            double[] emptyX = Enumerable.Range(0, gridSize).Select(i => (double)i).ToArray();
            double[] emptyY = Enumerable.Range(0, gridSize).Select(i => (double)i).ToArray();
            double[,] emptyZ = new double[gridSize, gridSize];
            return new SurfaceData { XValues = emptyX, YValues = emptyY, Z = emptyZ };
        }

        double minX = validPoints.Min(p => p.X);
        double maxX = validPoints.Max(p => p.X);
        double minY = validPoints.Min(p => p.Y);
        double maxY = validPoints.Max(p => p.Y);

        if (Math.Abs(maxX - minX) < 1e-9) { minX -= 1; maxX += 1; }
        if (Math.Abs(maxY - minY) < 1e-9) { minY -= 1; maxY += 1; }

        double[] xValues = new double[gridSize];
        double[] yValues = new double[gridSize];

        double stepX = (maxX - minX) / (gridSize - 1);
        double stepY = (maxY - minY) / (gridSize - 1);

        for (int i = 0; i < gridSize; i++) xValues[i] = minX + i * stepX;
        for (int j = 0; j < gridSize; j++) yValues[j] = minY + j * stepY;

        double[,] zSums = new double[gridSize, gridSize];
        int[,] zCounts = new int[gridSize, gridSize];

        foreach (var p in validPoints)
        {
            int ix = Math.Clamp((int)Math.Round((p.X - minX) / stepX), 0, gridSize - 1);
            int iy = Math.Clamp((int)Math.Round((p.Y - minY) / stepY), 0, gridSize - 1);

            zSums[ix, iy] += p.Z;
            zCounts[ix, iy]++;
        }

        double[,] zFinal = new double[gridSize, gridSize];

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                if (zCounts[i, j] > 0)
                {
                    zFinal[i, j] = zSums[i, j] / zCounts[i, j];
                }
                else
                {
                    double neighborSum = 0;
                    int neighborCount = 0;

                    for (int dx = -2; dx <= 2; dx++)
                    {
                        for (int dy = -2; dy <= 2; dy++)
                        {
                            int nx = i + dx;
                            int ny = j + dy;
                            if (nx >= 0 && nx < gridSize && ny >= 0 && ny < gridSize && zCounts[nx, ny] > 0)
                            {
                                neighborSum += zSums[nx, ny] / zCounts[nx, ny];
                                neighborCount++;
                            }
                        }
                    }

                    zFinal[i, j] = neighborCount > 0 ? neighborSum / neighborCount : 0.0;
                }
            }
        }

        return new SurfaceData
        {
            XValues = xValues,
            YValues = yValues,
            Z = zFinal
        };
    }
}
