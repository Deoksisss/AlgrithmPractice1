using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.GUI.Controls;

public class CallTreeCanvas : Control
{
    private List<AlgorithmCallNode> _nodes = new();
    private int _currentNodeId = -1;

    // Для зума и панорамирования
    private double _scale = 1.0;
    private Vector _pan = new Vector(0, 0);
    private bool _isPanning;
    private Point _lastPointerPosition;

    public CallTreeCanvas()
    {
        ClipToBounds = true;
        Focusable = true; // Чтобы ловить колесико мыши
    }

    public void SetTree(List<AlgorithmCallNode> nodes)
    {
        _nodes = nodes;
        _currentNodeId = nodes.Count > 0 ? nodes[0].Id : -1;
        InvalidateVisual();
    }

    public void SetCurrentNode(int nodeId)
    {
        _currentNodeId = nodeId;
        InvalidateVisual();
    }

    public void ResetView()
    {
        _scale = 1.0;
        _pan = new Vector(0, 0);
        InvalidateVisual();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        double zoomFactor = e.Delta.Y > 0 ? 1.15 : 1.0 / 1.15;
        
        var pointerPos = e.GetPosition(this);
        
        // Зум относительно курсора мыши
        double newScale = Math.Clamp(_scale * zoomFactor, 0.1, 3.0);
        double effectiveZoom = newScale / _scale;
        
        _pan = pointerPos - (pointerPos - _pan) * effectiveZoom;
        _scale = newScale;
        
        InvalidateVisual();
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
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_isPanning)
        {
            var currentPos = e.GetPosition(this);
            var delta = currentPos - _lastPointerPosition;
            _pan += delta;
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

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        
        // Белый фон
        context.FillRectangle(Brushes.White, new Rect(0, 0, Bounds.Width, Bounds.Height));

        if (_nodes.Count == 0) return;

        var nodeDict = new Dictionary<int, AlgorithmCallNode>();
        foreach (var n in _nodes) nodeDict[n.Id] = n;

        // Применяем трансформацию (зум + панорамирование)
        using (context.PushTransform(Matrix.CreateScale(_scale, _scale) * Matrix.CreateTranslation(_pan.X, _pan.Y)))
        {
            // 1. Рисуем связи (линии)
            var linePen = new Pen(new SolidColorBrush(Color.Parse("#CCCCCC")), 1.5);
            foreach (var node in _nodes)
            {
                if (node.ParentId.HasValue && nodeDict.TryGetValue(node.ParentId.Value, out var parent))
                {
                    // Центр низа родителя и центр верха ребенка
                    // Ширина узла фиксированная (120), высота 40
                    var startPoint = new Point(parent.X + 60, parent.Y + 40); 
                    var endPoint = new Point(node.X + 60, node.Y);
                    context.DrawLine(linePen, startPoint, endPoint);
                }
            }

            // 2. Рисуем узлы
            var typeface = new Typeface("Consolas");
            
            foreach (var node in _nodes)
            {
                bool isCurrent = node.Id == _currentNodeId;
                
                var rect = new Rect(node.X, node.Y, 120, 40);
                var bgColor = isCurrent ? Color.Parse("#0055D4") : Colors.White; // Синий как в графиках
                var borderColor = isCurrent ? Color.Parse("#0055D4") : Color.Parse("#999999");
                var defaultTextColor = isCurrent ? Colors.White : Colors.Black;
                
                // Рисуем фон узла
                context.FillRectangle(new SolidColorBrush(bgColor), rect, 4);
                context.DrawRectangle(new Pen(new SolidColorBrush(borderColor), isCurrent ? 2 : 1), rect, 4);

                // --- Отрисовка массива с подсветкой ---
                double currentX = node.X + 10;
                double currentY = node.Y + 10;
                
                // Открывающая скобка
                var bracketText = new FormattedText("[", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 14, new SolidColorBrush(defaultTextColor));
                context.DrawText(bracketText, new Point(currentX, currentY));
                currentX += bracketText.Width;

                for (int idx = 0; idx < node.ArrayState.Length; idx++)
                {
                    // Определяем, участвует ли этот индекс в обмене
                    bool isSwapping = (idx == node.SwapIndexA || idx == node.SwapIndexB);
                    
                    // Если элемент меняется — красим в оранжевый (или желтый, если узел активен)
                    IBrush itemBrush = defaultTextColor == Colors.White 
                        ? (isSwapping ? Brushes.Yellow : Brushes.White)
                        : (isSwapping ? Brushes.OrangeRed : Brushes.Black);

                    string itemText = node.ArrayState[idx].ToString();
                    var itemFormatted = new FormattedText(itemText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 14, itemBrush);
                    
                    context.DrawText(itemFormatted, new Point(currentX, currentY));
                    currentX += itemFormatted.Width;

                    // Запятая (если это не последний элемент)
                    if (idx < node.ArrayState.Length - 1)
                    {
                        var comma = new FormattedText(",", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 14, new SolidColorBrush(defaultTextColor));
                        context.DrawText(comma, new Point(currentX, currentY));
                        currentX += comma.Width;
                    }
                }

                // Закрывающая скобка
                var closeBracket = new FormattedText("]", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 14, new SolidColorBrush(defaultTextColor));
                context.DrawText(closeBracket, new Point(currentX, currentY));
            }
        }
    }
}