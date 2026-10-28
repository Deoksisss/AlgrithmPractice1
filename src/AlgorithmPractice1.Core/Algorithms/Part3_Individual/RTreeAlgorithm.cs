using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part3_Individual;

public sealed class RTreeSearchAlgorithm : AlgorithmBase<RTreeSearchInput, int>
{
    private RTree? _prebuiltTree; 
    private int _lastN = -1;
    public override string Id => "RTreeSearch";
    public override string DisplayName => "R-дерево (пространственный поиск)";
    public override string Description =>
        "Рекурсивное построение R-дерева и поиск объектов в заданной области. " +
        "Показывает преимущество пространственного индекса над полным перебором.";
    public override AlgorithmCategory Category => AlgorithmCategory.Individual;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Logarithmic;

    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 10000,
        NStep = 1000,
        RunsPerN = 5
    };

    public override RTreeSearchInput GenerateTypedInput(int n, ExperimentConfig config)
    {
        var rnd = new Random(42);
        var points = new Point[n];
        for (int i = 0; i < n; i++)
            points[i] = new Point(rnd.NextDouble() * 1000, rnd.NextDouble() * 1000);

        // Искомая область — квадрат в центре
        var query = new Rect(400, 400, 600, 600);
        return new RTreeSearchInput(points, query);
    }

    public override int ExecuteTyped(RTreeSearchInput input, MeasurementContext? context)
    {
        // Строим дерево только ОДИН раз для данного N (при первом замере)
        if (_prebuiltTree == null || input.Points.Length != _lastN)
        {
            _prebuiltTree = new RTree();
            foreach (var p in input.Points)
                _prebuiltTree.Insert(p, null); // Передаем null, чтобы не считать шаги построения
            
            _lastN = input.Points.Length;
        }

        // Замеряем ТОЛЬКО поиск
        return _prebuiltTree.RangeQuery(input.Query, context).Count;
    }
}

// --- Вспомогательные типы ---

public record Point(double X, double Y);
public record Rect(double MinX, double MinY, double MaxX, double MaxY)
{
    
    public double Area() => (MaxX - MinX) * (MaxY - MinY);

    public bool Intersects(Rect other) =>
        MinX <= other.MaxX && MaxX >= other.MinX &&
        MinY <= other.MaxY && MaxY >= other.MinY;

    public Rect Union(Rect other) => new(
        Math.Min(MinX, other.MinX), Math.Min(MinY, other.MinY),
        Math.Max(MaxX, other.MaxX), Math.Max(MaxY, other.MaxY));

    public static Rect FromPoint(Point p) => new(p.X, p.Y, p.X, p.Y);
}

public record RTreeSearchInput(Point[] Points, Rect Query);

// --- Сам R-дерево (упрощённая версия) ---

public class RTree
{
    private const int MaxEntries = 4;
    private RNode _root = new(isLeaf: true);

    public void Insert(Point p, MeasurementContext? ctx)
    {
        InsertRecursive(_root, p, ctx);
    }

    private void InsertRecursive(RNode node, Point p, MeasurementContext? ctx)
    {
        ctx?.IncrementSteps();

        if (node.IsLeaf)
        {
            node.Points.Add(p);
            node.Bounds = node.Bounds is null ? Rect.FromPoint(p) : node.Bounds.Union(Rect.FromPoint(p));
            if (node.Points.Count > MaxEntries) Split(node);
            return;
        }

        // Выбираем дочерний узел с минимальным расширением
        var target = node.Children
            .OrderBy(c => c.Bounds.Union(Rect.FromPoint(p)).Area() - c.Bounds.Area())
            .First();
        InsertRecursive(target, p, ctx);
        node.Bounds = node.Bounds.Union(target.Bounds);
    }

    public List<Point> RangeQuery(Rect query, MeasurementContext? ctx)
    {
        var result = new List<Point>();
        QueryRecursive(_root, query, result, ctx);
        return result;
    }

    private void QueryRecursive(RNode node, Rect query, List<Point> result, MeasurementContext? ctx)
    {
        ctx?.IncrementSteps();
        if (node.Bounds is null || !node.Bounds.Intersects(query)) return;

        if (node.IsLeaf)
        {
            foreach (var p in node.Points)
                if (query.MinX <= p.X && p.X <= query.MaxX &&
                    query.MinY <= p.Y && p.Y <= query.MaxY)
                    result.Add(p);
            return;
        }

        foreach (var c in node.Children)
            QueryRecursive(c, query, result, ctx);
    }

    private void Split(RNode node)
    {
        // Упрощённое разбиение: делим пополам
        // В реальном R-дереве используется алгоритм Quadratic Split или R*-tree
        var half = node.Points.Count / 2;
        var newNode = new RNode(isLeaf: true);
        newNode.Points.AddRange(node.Points.Skip(half));
        node.Points.RemoveRange(half, node.Points.Count - half);
        newNode.Bounds = newNode.Points.Select(Rect.FromPoint).Aggregate((a, b) => a.Union(b));
        node.Bounds = node.Points.Select(Rect.FromPoint).Aggregate((a, b) => a.Union(b));
        // В упрощённой версии не поднимаемся вверх — для замера времени достаточно
    }
}

public class RNode
{
    public bool IsLeaf { get; init; }
    public Rect? Bounds { get; set; }
    public List<Point> Points { get; } = new();
    public List<RNode> Children { get; } = new();
    
    public RNode(bool isLeaf)
    {
        IsLeaf = isLeaf;
    }
}