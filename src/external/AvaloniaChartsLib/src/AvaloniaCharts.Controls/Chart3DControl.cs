using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaCharts.Core;

namespace AvaloniaCharts.Controls;

/// <summary>
/// Avalonia Control для отображения одной или нескольких 3D поверхностей на одном пространстве с легендой, прозрачностью и стилями сетки/сплошной заливки.
/// </summary>
public class Chart3DControl : Control
{
    private readonly List<Chart3DSeries> _seriesList = new();
    private Chart3DParams _params = new();
    private readonly Camera3D _camera = new();

    private bool _isOrbiting;
    private Point _lastPointerPosition;

    public Chart3DControl()
    {
        ClipToBounds = true;
        _camera.Target = new Point3D(0, 0, 0);
        _camera.Distance = 5.0;
        _camera.Yaw = Math.PI / 4.0;
        _camera.Pitch = Math.PI / 6.0;
    }

    public void SetParams(Chart3DParams parameters)
    {
        _params = parameters ?? new Chart3DParams();
        InvalidateVisual();
    }

    public void SetSeries(IEnumerable<Chart3DSeries> series)
    {
        _seriesList.Clear();
        if (series != null)
        {
            _seriesList.AddRange(series);
        }
        InvalidateVisual();
    }

    public void AddSeries(Chart3DSeries series)
    {
        if (series != null)
        {
            _seriesList.Add(series);
            InvalidateVisual();
        }
    }

    public void SetData(SurfaceData data)
    {
        _seriesList.Clear();
        if (data != null)
        {
            _seriesList.Add(new Chart3DSeries
            {
                Name = "Поверхность",
                Data = data,
                ColorHex = "#007ACC",
                Style = SurfaceStyle.SolidWithWireframe,
                Opacity = 0.8
            });
        }
        InvalidateVisual();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var currentPoint = e.GetCurrentPoint(this);
        if (currentPoint.Properties.IsLeftButtonPressed)
        {
            _isOrbiting = true;
            _lastPointerPosition = e.GetPosition(this);
            e.Pointer.Capture(this);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_isOrbiting)
        {
            Point currentPos = e.GetPosition(this);
            Vector delta = currentPos - _lastPointerPosition;

            _camera.Yaw += delta.X * 0.008;
            _camera.Pitch += delta.Y * 0.008;
            _camera.Pitch = Math.Clamp(_camera.Pitch, -Math.PI / 2.0 + 0.05, Math.PI / 2.0 - 0.05);

            _lastPointerPosition = currentPos;
            InvalidateVisual();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_isOrbiting)
        {
            _isOrbiting = false;
            e.Pointer.Capture(null);
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        double factor = e.Delta.Y > 0 ? 0.9 : 1.1;
        _camera.Distance = Math.Clamp(_camera.Distance * factor, 1.0, 50.0);
        InvalidateVisual();
    }

    private sealed class QuadPolygon
    {
        public required Chart3DSeries Series { get; init; }
        public required Point3D[] WorldVertices { get; init; }
        public required Point[] ScreenVertices { get; init; }
        public required double DistanceToCamera { get; init; }
        public required double NormalizedHeight { get; init; }
        public required Point3D Normal { get; init; }
    }

    private sealed class ResolvedSeries
    {
        public required Chart3DSeries Series { get; init; }
        public required SurfaceData Surface { get; init; }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        double width = Bounds.Width;
        double height = Bounds.Height;

        if (width <= 0 || height <= 0)
            return;

        context.FillRectangle(Brushes.White, new Rect(0, 0, width, height));

        if (!string.IsNullOrEmpty(_params.Title))
        {
            var titleText = CreateText(_params.Title, 16, FontWeight.Bold, Brushes.Black);
            context.DrawText(titleText, new Point(width / 2 - titleText.Width / 2, 10));
        }

        List<ResolvedSeries> resolvedList = new List<ResolvedSeries>();

        foreach (var s in _seriesList)
        {
            SurfaceData? data = s.Data;
            if (data == null && s.PointCloud != null && s.PointCloud.Count > 0)
            {
                data = GridBinner.BinPointCloud(s.PointCloud);
            }

            if (data != null && data.XValues.Length >= 2 && data.YValues.Length >= 2)
            {
                resolvedList.Add(new ResolvedSeries { Series = s, Surface = data });
            }
        }

        if (resolvedList.Count == 0)
        {
            var noDataText = CreateText("Нет данных", 18, FontWeight.Normal, Brushes.Gray);
            context.DrawText(noDataText, new Point(width / 2 - noDataText.Width / 2, height / 2 - noDataText.Height / 2));
            return;
        }

        // Общие границы по всем поверхностям
        double minX = resolvedList.Min(r => r.Surface.XValues.Min());
        double maxX = resolvedList.Max(r => r.Surface.XValues.Max());
        double minY = resolvedList.Min(r => r.Surface.YValues.Min());
        double maxY = resolvedList.Max(r => r.Surface.YValues.Max());

        double minZ = double.MaxValue;
        double maxZ = double.MinValue;

        foreach (var res in resolvedList)
        {
            int nx = res.Surface.XValues.Length;
            int ny = res.Surface.YValues.Length;

            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    double z = res.Surface.Z[i, j];
                    if (!double.IsNaN(z) && !double.IsInfinity(z))
                    {
                        if (z < minZ) minZ = z;
                        if (z > maxZ) maxZ = z;
                    }
                }
            }
        }

        if (minZ > maxZ) { minZ = -1; maxZ = 1; }
        if (Math.Abs(maxX - minX) < 1e-9) { minX -= 1; maxX += 1; }
        if (Math.Abs(maxY - minY) < 1e-9) { minY -= 1; maxY += 1; }
        if (Math.Abs(maxZ - minZ) < 1e-9) { minZ -= 1; maxZ += 1; }

        Point3D ToWorld(SurfaceData sData, int ix, int iy)
        {
            double rx = sData.XValues[ix];
            double ry = sData.YValues[iy];
            double rz = sData.Z[ix, iy];

            if (double.IsNaN(rz) || double.IsInfinity(rz)) rz = minZ;

            double wx = (rx - minX) / (maxX - minX) * 2.4 - 1.2;
            double wy = (rz - minZ) / (maxZ - minZ) * 2.4 - 1.2;
            double wz = (ry - minY) / (maxY - minY) * 2.4 - 1.2;
            return new Point3D(wx, wy, wz);
        }

        Matrix4x4Data transform = _camera.GetTransformMatrix(width, height);
        Point3D eyePos = _camera.GetEyePosition();

        List<QuadPolygon> polygons = new List<QuadPolygon>();
        Point3D lightDir = Normalize(new Point3D(0.4, 1.0, 0.6));

        foreach (var res in resolvedList)
        {
            int nx = res.Surface.XValues.Length;
            int ny = res.Surface.YValues.Length;

            for (int i = 0; i < nx - 1; i++)
            {
                for (int j = 0; j < ny - 1; j++)
                {
                    Point3D w00 = ToWorld(res.Surface, i, j);
                    Point3D w10 = ToWorld(res.Surface, i + 1, j);
                    Point3D w11 = ToWorld(res.Surface, i + 1, j + 1);
                    Point3D w01 = ToWorld(res.Surface, i, j + 1);

                    Point3D s00 = Matrix4x4Math.TransformPoint(transform, w00);
                    Point3D s10 = Matrix4x4Math.TransformPoint(transform, w10);
                    Point3D s11 = Matrix4x4Math.TransformPoint(transform, w11);
                    Point3D s01 = Matrix4x4Math.TransformPoint(transform, w01);

                    double avgWx = (w00.X + w10.X + w11.X + w01.X) * 0.25;
                    double avgWy = (w00.Y + w10.Y + w11.Y + w01.Y) * 0.25;
                    double avgWz = (w00.Z + w10.Z + w11.Z + w01.Z) * 0.25;

                    double dx = avgWx - eyePos.X;
                    double dy = avgWy - eyePos.Y;
                    double dz = avgWz - eyePos.Z;
                    double dist = Math.Sqrt(dx * dx + dy * dy + dz * dz);

                    Point3D v1 = new Point3D(w10.X - w00.X, w10.Y - w00.Y, w10.Z - w00.Z);
                    Point3D v2 = new Point3D(w01.X - w00.X, w01.Y - w00.Y, w01.Z - w00.Z);
                    Point3D normal = Normalize(Cross(v1, v2));

                    double avgZRaw = (res.Surface.Z[i, j] + res.Surface.Z[i + 1, j] + res.Surface.Z[i + 1, j + 1] + res.Surface.Z[i, j + 1]) * 0.25;
                    double normH = Math.Clamp((avgZRaw - minZ) / (maxZ - minZ), 0.0, 1.0);

                    polygons.Add(new QuadPolygon
                    {
                        Series = res.Series,
                        WorldVertices = new[] { w00, w10, w11, w01 },
                        ScreenVertices = new[] { new Point(s00.X, s00.Y), new Point(s10.X, s10.Y), new Point(s11.X, s11.Y), new Point(s01.X, s01.Y) },
                        DistanceToCamera = dist,
                        NormalizedHeight = normH,
                        Normal = normal
                    });
                }
            }
        }

        // Сортировка по дальности от камеры
        polygons.Sort((a, b) => b.DistanceToCamera.CompareTo(a.DistanceToCamera));

        foreach (var poly in polygons)
        {
            double lightDot = Math.Max(0.25, Math.Abs(Dot(poly.Normal, lightDir)));
            Color baseColor = ColorHelper.ParseColor(poly.Series.ColorHex);

            byte alpha = (byte)Math.Clamp(poly.Series.Opacity * 255, 10, 255);

            Color shadedColor = Color.FromArgb(
                alpha,
                (byte)Math.Clamp(baseColor.R * lightDot, 0, 255),
                (byte)Math.Clamp(baseColor.G * lightDot, 0, 255),
                (byte)Math.Clamp(baseColor.B * lightDot, 0, 255));

            Brush? fillBrush = poly.Series.Style == SurfaceStyle.Wireframe ? null : new SolidColorBrush(shadedColor);

            Pen? wirePen = poly.Series.Style switch
            {
                SurfaceStyle.Solid => null,
                SurfaceStyle.Wireframe => new Pen(new SolidColorBrush(Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B)), 0.8),
                _ => new Pen(new SolidColorBrush(Color.FromArgb((byte)(alpha * 0.5), 0, 0, 0)), 0.5)
            };

            StreamGeometry geo = new StreamGeometry();
            using (StreamGeometryContext geoCtx = geo.Open())
            {
                geoCtx.BeginFigure(poly.ScreenVertices[0], true);
                geoCtx.LineTo(poly.ScreenVertices[1]);
                geoCtx.LineTo(poly.ScreenVertices[2]);
                geoCtx.LineTo(poly.ScreenVertices[3]);
                geoCtx.EndFigure(true);
            }

            context.DrawGeometry(fillBrush, wirePen, geo);
        }

        DrawBoundingBoxAndLabels(context, transform, width, height, _params);
        DrawLegend(context, _seriesList);
    }

    private static void DrawLegend(DrawingContext context, List<Chart3DSeries> seriesList)
    {
        if (seriesList.Count == 0) return;

        double itemHeight = 20;
        double iconSize = 14;
        double padding = 8;
        double maxWidth = 0;

        List<(Chart3DSeries series, FormattedText text)> legendItems = new();

        foreach (var s in seriesList)
        {
            var text = CreateText(s.Name, 11, FontWeight.Normal, Brushes.Black);
            if (text.Width > maxWidth) maxWidth = text.Width;
            legendItems.Add((s, text));
        }

        double boxWidth = iconSize + maxWidth + padding * 3;
        double boxHeight = legendItems.Count * itemHeight + padding * 2;

        Point boxPos = new Point(20, 20);
        Rect legendBox = new Rect(boxPos.X, boxPos.Y, boxWidth, boxHeight);

        Brush legendBg = new SolidColorBrush(Color.FromArgb(230, 255, 255, 255));
        Pen legendBorder = new Pen(new SolidColorBrush(Color.FromRgb(200, 200, 200)), 1);

        context.DrawRectangle(legendBg, legendBorder, legendBox, 4, 4);

        double curY = boxPos.Y + padding;

        foreach (var item in legendItems)
        {
            Color baseColor = ColorHelper.ParseColor(item.series.ColorHex);
            byte alpha = (byte)Math.Clamp(item.series.Opacity * 255, 50, 255);

            Brush iconBrush = item.series.Style == SurfaceStyle.Wireframe
                ? new SolidColorBrush(Colors.Transparent)
                : new SolidColorBrush(Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B));

            Pen iconPen = new Pen(new SolidColorBrush(Color.FromRgb(baseColor.R, baseColor.G, baseColor.B)), 1);

            Rect iconRect = new Rect(boxPos.X + padding, curY + (itemHeight - iconSize) / 2, iconSize, iconSize);
            context.DrawRectangle(iconBrush, iconPen, iconRect, 2, 2);

            context.DrawText(item.text, new Point(boxPos.X + padding * 2 + iconSize, curY + (itemHeight - item.text.Height) / 2));

            curY += itemHeight;
        }
    }

    private static void DrawBoundingBoxAndLabels(DrawingContext context, Matrix4x4Data transform, double width, double height, Chart3DParams p)
    {
        Point3D[] boxCorners = new Point3D[]
        {
            new(-1.2, -1.2, -1.2), new(1.2, -1.2, -1.2),
            new(1.2, -1.2, 1.2), new(-1.2, -1.2, 1.2),
            new(-1.2, 1.2, -1.2), new(1.2, 1.2, -1.2),
            new(1.2, 1.2, 1.2), new(-1.2, 1.2, 1.2)
        };

        Point[] screenCorners = boxCorners.Select(c =>
        {
            Point3D s = Matrix4x4Math.TransformPoint(transform, c);
            return new Point(s.X, s.Y);
        }).ToArray();

        Pen boxPen = new Pen(new SolidColorBrush(Color.FromRgb(180, 180, 180)), 1.0);

        context.DrawLine(boxPen, screenCorners[0], screenCorners[1]);
        context.DrawLine(boxPen, screenCorners[1], screenCorners[2]);
        context.DrawLine(boxPen, screenCorners[2], screenCorners[3]);
        context.DrawLine(boxPen, screenCorners[3], screenCorners[0]);

        context.DrawLine(boxPen, screenCorners[0], screenCorners[4]);
        context.DrawLine(boxPen, screenCorners[1], screenCorners[5]);
        context.DrawLine(boxPen, screenCorners[2], screenCorners[6]);
        context.DrawLine(boxPen, screenCorners[3], screenCorners[7]);

        if (!string.IsNullOrEmpty(p.XLabel))
        {
            var xText = CreateText(p.XLabel + " (X)", 12, FontWeight.Bold, Brushes.DarkBlue);
            Point midX = new Point((screenCorners[0].X + screenCorners[1].X) / 2, (screenCorners[0].Y + screenCorners[1].Y) / 2 + 10);
            context.DrawText(xText, midX);
        }

        if (!string.IsNullOrEmpty(p.YLabel))
        {
            var yText = CreateText(p.YLabel + " (Y)", 12, FontWeight.Bold, Brushes.DarkGreen);
            Point midY = new Point((screenCorners[0].X + screenCorners[3].X) / 2 - 25, (screenCorners[0].Y + screenCorners[3].Y) / 2 + 10);
            context.DrawText(yText, midY);
        }

        if (!string.IsNullOrEmpty(p.ZLabel))
        {
            var zText = CreateText(p.ZLabel + " (Z)", 12, FontWeight.Bold, Brushes.DarkRed);
            Point midZ = new Point(screenCorners[0].X - 45, (screenCorners[0].Y + screenCorners[4].Y) / 2);
            context.DrawText(zText, midZ);
        }
    }

    private static Point3D Cross(Point3D a, Point3D b)
    {
        return new Point3D(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X);
    }

    private static double Dot(Point3D a, Point3D b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }

    private static Point3D Normalize(Point3D v)
    {
        double len = Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        if (len > 1e-9)
            return new Point3D(v.X / len, v.Y / len, v.Z / len);
        return new Point3D(0, 1, 0);
    }

    private static FormattedText CreateText(string text, double fontSize, FontWeight weight, IBrush foreground)
    {
        return new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            fontSize,
            foreground);
    }
}
