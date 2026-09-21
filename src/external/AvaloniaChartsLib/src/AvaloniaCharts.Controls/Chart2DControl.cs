using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaCharts.Core;

namespace AvaloniaCharts.Controls;

/// <summary>
/// Avalonia Control для отображения одного или нескольких 2D графиков на одной плоскости с легендой и стилизацией линий.
/// </summary>
public class Chart2DControl : Control
{
    private readonly List<Chart2DSeries> _seriesList = new();
    private Chart2DParams _params = new();

    private double _zoomScale = 1.0;
    private Vector _panOffset = new(0, 0);
    private bool _isPanning;
    private Point _lastPointerPosition;

    public Chart2DControl()
    {
        ClipToBounds = true;
    }

    public void SetParams(Chart2DParams parameters)
    {
        _params = parameters ?? new Chart2DParams();
        InvalidateVisual();
    }

    public void SetSeries(IEnumerable<Chart2DSeries> series)
    {
        _seriesList.Clear();
        if (series != null)
        {
            _seriesList.AddRange(series);
        }
        ResetView();
        InvalidateVisual();
    }

    public void AddSeries(Chart2DSeries series)
    {
        if (series != null)
        {
            _seriesList.Add(series);
            InvalidateVisual();
        }
    }

    public void SetData(IReadOnlyList<Point2D>? points)
    {
        _seriesList.Clear();
        if (points != null)
        {
            _seriesList.Add(new Chart2DSeries
            {
                Name = "График",
                Points = points,
                ColorHex = "#000080",
                LineStyle = LineStyle.Solid
            });
        }
        ResetView();
        InvalidateVisual();
    }

    public void ResetView()
    {
        _zoomScale = 1.0;
        _panOffset = new Vector(0, 0);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var currentPoint = e.GetCurrentPoint(this);
        if (currentPoint.Properties.IsLeftButtonPressed)
        {
            _isPanning = true;
            _lastPointerPosition = e.GetPosition(this);
            e.Pointer.Capture(this);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_isPanning)
        {
            Point currentPos = e.GetPosition(this);
            Vector delta = currentPos - _lastPointerPosition;
            _panOffset += delta;
            _lastPointerPosition = currentPos;
            InvalidateVisual();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_isPanning)
        {
            _isPanning = false;
            e.Pointer.Capture(null);
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        double factor = e.Delta.Y > 0 ? 1.15 : 0.85;
        _zoomScale = Math.Clamp(_zoomScale * factor, 0.1, 50.0);
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        double width = Bounds.Width;
        double height = Bounds.Height;

        if (width <= 0 || height <= 0)
            return;

        context.FillRectangle(Brushes.White, new Rect(0, 0, width, height));

        double marginPaddingLeft = 70;
        double marginPaddingBottom = 50;
        double marginPaddingRight = 30;
        double marginPaddingTop = 40;

        Rect plotRect = new Rect(
            marginPaddingLeft,
            marginPaddingTop,
            Math.Max(10, width - marginPaddingLeft - marginPaddingRight),
            Math.Max(10, height - marginPaddingTop - marginPaddingBottom));

        if (!string.IsNullOrEmpty(_params.Title))
        {
            var titleText = CreateText(_params.Title, 16, FontWeight.Bold, Brushes.Black);
            context.DrawText(titleText, new Point(width / 2 - titleText.Width / 2, 10));
        }

        var activeSeries = _seriesList
            .Select(s => new
            {
                Series = s,
                ValidPoints = s.Points?
                    .Where(p => !double.IsNaN(p.X) && !double.IsInfinity(p.X) &&
                                !double.IsNaN(p.Y) && !double.IsInfinity(p.Y))
                    .OrderBy(p => p.X)
                    .ToList() ?? new List<Point2D>()
            })
            .Where(s => s.ValidPoints.Count > 0)
            .ToList();

        if (activeSeries.Count == 0)
        {
            var noDataText = CreateText("Нет данных", 18, FontWeight.Normal, Brushes.Gray);
            context.DrawText(noDataText, new Point(width / 2 - noDataText.Width / 2, height / 2 - noDataText.Height / 2));
            return;
        }

        var allX = activeSeries.SelectMany(s => s.ValidPoints.Select(p => p.X));
        var allY = activeSeries.SelectMany(s => s.ValidPoints.Select(p => p.Y));

        var scaleX = AxisScale.Calculate(allX);
        var scaleY = AxisScale.Calculate(allY);

        double xSpan = (scaleX.Max - scaleX.Min) / _zoomScale;
        double ySpan = (scaleY.Max - scaleY.Min) / _zoomScale;

        double xCenter = (scaleX.Min + scaleX.Max) / 2.0 - (_panOffset.X / plotRect.Width) * xSpan;
        double yCenter = (scaleY.Min + scaleY.Max) / 2.0 + (_panOffset.Y / plotRect.Height) * ySpan;

        double visXMin = xCenter - xSpan / 2.0;
        double visXMax = xCenter + xSpan / 2.0;
        double visYMin = yCenter - ySpan / 2.0;
        double visYMax = yCenter + ySpan / 2.0;

        var visScaleX = AxisScale.Calculate(new[] { visXMin, visXMax });
        var visScaleY = AxisScale.Calculate(new[] { visYMin, visYMax });

        Pen gridPen = new Pen(new SolidColorBrush(Color.FromRgb(230, 230, 230)), 1);
        Pen axisPen = new Pen(Brushes.Black, 1.5);

        foreach (var tick in visScaleX.Ticks)
        {
            if (tick < visXMin || tick > visXMax) continue;
            double px = plotRect.Left + (tick - visXMin) / (visXMax - visXMin) * plotRect.Width;

            context.DrawLine(gridPen, new Point(px, plotRect.Top), new Point(px, plotRect.Bottom));
            context.DrawLine(axisPen, new Point(px, plotRect.Bottom), new Point(px, plotRect.Bottom + 5));

            var labelText = CreateText(FormatNumber(tick), 11, FontWeight.Normal, Brushes.DarkSlateGray);
            context.DrawText(labelText, new Point(px - labelText.Width / 2, plotRect.Bottom + 7));
        }

        foreach (var tick in visScaleY.Ticks)
        {
            if (tick < visYMin || tick > visYMax) continue;
            double py = plotRect.Bottom - (tick - visYMin) / (visYMax - visYMin) * plotRect.Height;

            context.DrawLine(gridPen, new Point(plotRect.Left, py), new Point(plotRect.Right, py));
            context.DrawLine(axisPen, new Point(plotRect.Left - 5, py), new Point(plotRect.Left, py));

            var labelText = CreateText(FormatNumber(tick), 11, FontWeight.Normal, Brushes.DarkSlateGray);
            context.DrawText(labelText, new Point(plotRect.Left - labelText.Width - 8, py - labelText.Height / 2));
        }

        context.DrawRectangle(null, axisPen, plotRect);

        if (!string.IsNullOrEmpty(_params.XLabel))
        {
            var xLabelText = CreateText(_params.XLabel, 12, FontWeight.Bold, Brushes.Black);
            context.DrawText(xLabelText, new Point(plotRect.Left + plotRect.Width / 2 - xLabelText.Width / 2, height - 25));
        }

        if (!string.IsNullOrEmpty(_params.YLabel))
        {
            var yLabelText = CreateText(_params.YLabel, 12, FontWeight.Bold, Brushes.Black);
            context.DrawText(yLabelText, new Point(10, plotRect.Top + plotRect.Height / 2 - yLabelText.Height / 2));
        }

        // Отрисовка линий графиков
        using (context.PushClip(plotRect))
        {
            foreach (var item in activeSeries)
            {
                Color color = ColorHelper.ParseColor(item.Series.ColorHex);
                Pen linePen = CreatePen(color, item.Series.LineThickness, item.Series.LineStyle);

                StreamGeometry geometry = new StreamGeometry();
                using (StreamGeometryContext geoContext = geometry.Open())
                {
                    bool isFirst = true;
                    foreach (var pt in item.ValidPoints)
                    {
                        double screenX = plotRect.Left + (pt.X - visXMin) / (visXMax - visXMin) * plotRect.Width;
                        double screenY = plotRect.Bottom - (pt.Y - visYMin) / (visYMax - visYMin) * plotRect.Height;

                        Point p = new Point(screenX, screenY);
                        if (isFirst)
                        {
                            geoContext.BeginFigure(p, false);
                            isFirst = false;
                        }
                        else
                        {
                            geoContext.LineTo(p);
                        }
                    }
                }

                context.DrawGeometry(null, linePen, geometry);
            }
        }

        // Отрисовка сноски/легенды в левом верхнем углу
        DrawLegend(context, plotRect, _seriesList);
    }

    private static void DrawLegend(DrawingContext context, Rect plotRect, List<Chart2DSeries> seriesList)
    {
        if (seriesList.Count == 0) return;

        double itemHeight = 20;
        double iconWidth = 30;
        double padding = 8;
        double maxWidth = 0;

        List<(Chart2DSeries series, FormattedText text)> legendItems = new();

        foreach (var s in seriesList)
        {
            var text = CreateText(s.Name, 11, FontWeight.Normal, Brushes.Black);
            if (text.Width > maxWidth) maxWidth = text.Width;
            legendItems.Add((s, text));
        }

        double boxWidth = iconWidth + maxWidth + padding * 3;
        double boxHeight = legendItems.Count * itemHeight + padding * 2;

        Point boxPos = new Point(plotRect.Left + 10, plotRect.Top + 10);
        Rect legendBox = new Rect(boxPos.X, boxPos.Y, boxWidth, boxHeight);

        // Полупрозрачный фон с тонкой рамкой
        Brush legendBg = new SolidColorBrush(Color.FromArgb(230, 255, 255, 255));
        Pen legendBorder = new Pen(new SolidColorBrush(Color.FromRgb(200, 200, 200)), 1);

        context.DrawRectangle(legendBg, legendBorder, legendBox, 4, 4);

        double curY = boxPos.Y + padding;

        foreach (var item in legendItems)
        {
            Color color = ColorHelper.ParseColor(item.series.ColorHex);
            Pen linePen = CreatePen(color, 2.0, item.series.LineStyle);

            Point iconStart = new Point(boxPos.X + padding, curY + itemHeight / 2);
            Point iconEnd = new Point(boxPos.X + padding + iconWidth, curY + itemHeight / 2);

            context.DrawLine(linePen, iconStart, iconEnd);
            context.DrawText(item.text, new Point(boxPos.X + padding * 2 + iconWidth, curY + (itemHeight - item.text.Height) / 2));

            curY += itemHeight;
        }
    }

    private static Pen CreatePen(Color color, double thickness, LineStyle style)
    {
        IDashStyle? dashStyle = style switch
        {
            LineStyle.Dashed => new DashStyle(new double[] { 4, 3 }, 0),
            LineStyle.Dotted => new DashStyle(new double[] { 1.5, 3 }, 0),
            _ => null
        };

        return new Pen(new SolidColorBrush(color), thickness, dashStyle);
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

    private static string FormatNumber(double val)
    {
        if (Math.Abs(val) < 1e-9) return "0";
        if (Math.Abs(val) >= 10000 || Math.Abs(val) < 0.01) return val.ToString("E2", CultureInfo.InvariantCulture);
        return val.ToString("G5", CultureInfo.InvariantCulture);
    }
}
