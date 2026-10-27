using System.Globalization;
using AlgorithmPractice1.Core.CallTree;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace AlgorithmPractice1.GUI.Controls;

/// <summary>
/// Интерактивный элемент управления для визуализации дерева рекурсивных вызовов в виде графа с панорамированием и масштабированием.
/// </summary>
public class CallTreeGraphControl : Control
{
    public static readonly StyledProperty<CallTreeNode?> RootNodeProperty =
        AvaloniaProperty.Register<CallTreeGraphControl, CallTreeNode?>(nameof(RootNode));

    public CallTreeNode? RootNode
    {
        get => GetValue(RootNodeProperty);
        set => SetValue(RootNodeProperty, value);
    }

    private class NodeLayout
    {
        public CallTreeNode Node { get; }
        public double X { get; set; }
        public double Y { get; set; }
        public double SubtreeWidth { get; set; }
        public List<NodeLayout> Children { get; } = new();

        public NodeLayout(CallTreeNode node)
        {
            Node = node;
        }
    }

    private const double CardWidth = 320.0;
    private const double CardHeight = 94.0;
    private const double SiblingGap = 28.0;
    private const double LevelGap = 75.0;

    private double _zoomScale = 1.0;
    private Vector _panOffset = new(40, 40);
    private bool _isPanning;
    private Point _lastPointerPosition;
    private NodeLayout? _rootLayout;

    static CallTreeGraphControl()
    {
        AffectsRender<CallTreeGraphControl>(RootNodeProperty);
    }

    public CallTreeGraphControl()
    {
        ClipToBounds = true;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == RootNodeProperty)
        {
            CalculateLayout();
            CenterView();
            InvalidateVisual();
        }
    }

    public void ZoomIn()
    {
        _zoomScale = Math.Min(_zoomScale * 1.25, 4.0);
        InvalidateVisual();
    }

    public void ZoomOut()
    {
        _zoomScale = Math.Max(_zoomScale / 1.25, 0.2);
        InvalidateVisual();
    }

    public void ResetView()
    {
        _zoomScale = 1.0;
        CenterView();
        InvalidateVisual();
    }

    public void CenterView()
    {
        if (_rootLayout != null && Bounds.Width > 0)
        {
            double rootCenterX = _rootLayout.X + CardWidth / 2.0;
            _panOffset = new Vector(Bounds.Width / 2.0 - rootCenterX * _zoomScale, 40.0);
        }
        else
        {
            _panOffset = new Vector(40, 40);
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        var pointerPos = e.GetPosition(this);
        double delta = e.Delta.Y > 0 ? 1.15 : 0.85;
        double newZoom = Math.Clamp(_zoomScale * delta, 0.2, 4.0);

        if (Math.Abs(newZoom - _zoomScale) > 0.001)
        {
            // Зумирование относительно курсора
            _panOffset = pointerPos - (pointerPos - _panOffset) * (newZoom / _zoomScale);
            _zoomScale = newZoom;
            InvalidateVisual();
        }
        e.Handled = true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isPanning = true;
            _lastPointerPosition = e.GetPosition(this);
            e.Pointer.Capture(this);
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_isPanning)
        {
            var currentPos = e.GetPosition(this);
            _panOffset += currentPos - _lastPointerPosition;
            _lastPointerPosition = currentPos;
            InvalidateVisual();
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_isPanning)
        {
            _isPanning = false;
            e.Pointer.Capture(null);
            e.Handled = true;
        }
    }

    private void CalculateLayout()
    {
        if (RootNode == null)
        {
            _rootLayout = null;
            return;
        }

        _rootLayout = BuildLayoutTree(RootNode);
        ComputeSubtreeWidths(_rootLayout);
        AssignPositions(_rootLayout, 0, 0);
    }

    private static NodeLayout BuildLayoutTree(CallTreeNode node)
    {
        var layout = new NodeLayout(node);
        foreach (var child in node.Children)
        {
            layout.Children.Add(BuildLayoutTree(child));
        }
        return layout;
    }

    private static void ComputeSubtreeWidths(NodeLayout layout)
    {
        if (layout.Children.Count == 0)
        {
            layout.SubtreeWidth = CardWidth;
            return;
        }

        double totalChildrenWidth = 0;
        foreach (var child in layout.Children)
        {
            ComputeSubtreeWidths(child);
            totalChildrenWidth += child.SubtreeWidth;
        }

        totalChildrenWidth += (layout.Children.Count - 1) * SiblingGap;
        layout.SubtreeWidth = Math.Max(CardWidth, totalChildrenWidth);
    }

    private static void AssignPositions(NodeLayout layout, double leftX, int level)
    {
        layout.Y = level * (CardHeight + LevelGap);

        if (layout.Children.Count == 0)
        {
            layout.X = leftX + (layout.SubtreeWidth - CardWidth) / 2.0;
            return;
        }

        double currentLeft = leftX;
        foreach (var child in layout.Children)
        {
            AssignPositions(child, currentLeft, level + 1);
            currentLeft += child.SubtreeWidth + SiblingGap;
        }

        double firstChildCenter = layout.Children[0].X + CardWidth / 2.0;
        double lastChildCenter = layout.Children[^1].X + CardWidth / 2.0;
        layout.X = (firstChildCenter + lastChildCenter) / 2.0 - CardWidth / 2.0;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        // Фоновая сетка
        context.FillRectangle(new SolidColorBrush(Color.Parse("#F8FAFC")), bounds);

        if (_rootLayout == null)
        {
            var typeface = new Typeface(FontFamily.Default);
            var emptyText = new FormattedText(
                "Дерево вызовов пусто. Запустите пакет с рекурсивным алгоритмом или выберите алгоритм слева.",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                14,
                new SolidColorBrush(Color.Parse("#64748B"))
            );
            context.DrawText(emptyText, new Point(bounds.Width / 2 - emptyText.Width / 2, bounds.Height / 2 - 10));
            return;
        }

        using (context.PushTransform(Matrix.CreateTranslation(_panOffset.X, _panOffset.Y) * Matrix.CreateScale(_zoomScale, _zoomScale)))
        {
            // 1. Отрисовка связей (линий вызовов)
            DrawConnections(context, _rootLayout);

            // 2. Отрисовка узлов
            DrawNodes(context, _rootLayout);
        }
    }

    private void DrawConnections(DrawingContext context, NodeLayout layout)
    {
        var linePen = new Pen(new SolidColorBrush(Color.Parse("#8B5CF6")), 1.8);

        double startX = layout.X + CardWidth / 2.0;
        double startY = layout.Y + CardHeight;

        foreach (var child in layout.Children)
        {
            double endX = child.X + CardWidth / 2.0;
            double endY = child.Y;

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point(startX, startY), false);
                double controlY1 = startY + (endY - startY) * 0.5;
                double controlY2 = endY - (endY - startY) * 0.5;
                ctx.CubicBezierTo(new Point(startX, controlY1), new Point(endX, controlY2), new Point(endX, endY));
            }

            context.DrawGeometry(null, linePen, geometry);
            DrawConnections(context, child);
        }
    }

    private void DrawNodes(DrawingContext context, NodeLayout layout)
    {
        var node = layout.Node;
        var rect = new Rect(layout.X, layout.Y, CardWidth, CardHeight);

        bool isRoot = node.Parent == null;
        var bgBrush = new SolidColorBrush(isRoot ? Color.Parse("#FAF5FF") : Color.Parse("#FFFFFF"));
        var borderPen = new Pen(new SolidColorBrush(isRoot ? Color.Parse("#9333EA") : Color.Parse("#CBD5E1")), isRoot ? 2.0 : 1.2);

        // Карточка узла со скруглёнными углами
        context.DrawRectangle(bgBrush, borderPen, new RoundedRect(rect, 8.0));

        // Верхняя декоративная полоска
        var headerRect = new Rect(layout.X, layout.Y, CardWidth, 4.0);
        var stripBrush = new SolidColorBrush(isRoot ? Color.Parse("#7E22CE") : Color.Parse("#A855F7"));
        context.DrawRectangle(stripBrush, null, new RoundedRect(headerRect, 8.0, 8.0, 0, 0));

        var typeface = new Typeface(FontFamily.Default);
        var boldTypeface = new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.SemiBold);
        var monoTypeface = new Typeface("Cascadia Code, Consolas, Courier New, monospace");

        // 1. Имя метода
        string title = $"{node.MethodName}()";
        var titleText = new FormattedText(
            title,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            boldTypeface,
            12,
            new SolidColorBrush(Color.Parse("#0F172A"))
        );
        context.DrawText(titleText, new Point(layout.X + 10, layout.Y + 8));

        // Номер вызова в верхнем правом углу
        string callBadge = $"#{node.CallId}";
        var callBadgeText = new FormattedText(
            callBadge,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            10,
            new SolidColorBrush(Color.Parse("#94A3B8"))
        );
        context.DrawText(callBadgeText, new Point(layout.X + CardWidth - callBadgeText.Width - 10, layout.Y + 8));

        // 2. Аргументы вызова (каждый аргумент на отдельной строке)
        double currentY = layout.Y + 28;
        if (node.Arguments.Count == 0)
        {
            var noArgsText = new FormattedText(
                "<без аргументов>",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                11,
                new SolidColorBrush(Color.Parse("#64748B"))
            );
            context.DrawText(noArgsText, new Point(layout.X + 10, currentY));
            currentY += 18;
        }
        else
        {
            // Отрисовываем аргументы (до 2 строк: n и input)
            int linesToDraw = Math.Min(node.Arguments.Count, 2);
            for (int i = 0; i < linesToDraw; i++)
            {
                var arg = node.Arguments[i];
                string argStr = $"{arg.Name} = {arg.Value}";
                if (argStr.Length > 44) argStr = argStr.Substring(0, 41) + "...";

                var argText = new FormattedText(
                    argStr,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    monoTypeface,
                    11,
                    new SolidColorBrush(Color.Parse("#334155"))
                );
                context.DrawText(argText, new Point(layout.X + 10, currentY));
                currentY += 18;
            }
        }

        // 3. Результат вызова (если есть)
        if (!string.IsNullOrEmpty(node.ReturnValueText))
        {
            string result = $"➔ {node.ReturnValueText}";
            if (result.Length > 44) result = result.Substring(0, 41) + "...";

            var resText = new FormattedText(
                result,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                boldTypeface,
                11,
                new SolidColorBrush(Color.Parse("#16A34A"))
            );
            context.DrawText(resText, new Point(layout.X + 10, layout.Y + CardHeight - 22));
        }

        foreach (var child in layout.Children)
        {
            DrawNodes(context, child);
        }
    }
}
