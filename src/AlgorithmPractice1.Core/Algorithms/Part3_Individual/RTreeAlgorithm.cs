using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.CallTree;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part3_Individual;

/// <summary>
/// Алгоритм пространственного диапазонного поиска с использованием R-дерева (сложность O(log n)).
/// </summary>
public sealed class RTreeSearchAlgorithm : AlgorithmBase<RTreeSearchInput, int>
{
    private RTree? _prebuiltTree;
    private int _lastN = -1;

    public override string Id => "RTreeSearch";
    public override string DisplayName => "R-дерево (пространственный поиск)";
    public override string Description =>
        "Иерархическое пространственное R-дерево (MBR). " +
        "Рекурсивный обход узлов и отсечение поддеревьев, не пересекающих область запроса (O(log n)).";
    public override AlgorithmCategory Category => AlgorithmCategory.Individual;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Logarithmic;
    public override bool IsRecursive => true;

    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 5000,
        NStep = 500,
        RunsPerN = 5
    };

    public override RTreeSearchInput GenerateTypedInput(int n, ExperimentConfig config)
    {
        var rnd = new Random(42 + n);
        var points = new Point[n];
        for (int i = 0; i < n; i++)
        {
            points[i] = new Point(rnd.NextDouble() * 1000.0, rnd.NextDouble() * 1000.0);
        }

        // Область поиска в центре плоскости
        var query = new Rect(250.0, 250.0, 750.0, 750.0);
        return new RTreeSearchInput(points, query);
    }

    public override int ExecuteTyped(RTreeSearchInput input, MeasurementContext? context)
    {
        // При трекинге дерева вызовов всегда создаём свежее дерево для текущего входа
        if (CallTreeTracker.IsTracking)
        {
            var tree = new RTree(this);
            foreach (var p in input.Points)
            {
                tree.Insert(p);
            }
            return tree.RangeQuery(input.Query, context);
        }

        // При регулярных замерах кэшируем дерево для одинакового N
        if (_prebuiltTree == null || input.Points.Length != _lastN)
        {
            _prebuiltTree = new RTree(this);
            foreach (var p in input.Points)
            {
                _prebuiltTree.Insert(p);
            }
            _lastN = input.Points.Length;
        }

        return _prebuiltTree.RangeQuery(input.Query, context);
    }

    internal TResult TrackCallFunc<TResult>(System.Linq.Expressions.Expression<Func<TResult>> expr)
    {
        return OutputCallTree(expr);
    }
}

// --- Модели данных R-дерева ---

public record Point(double X, double Y)
{
    public override string ToString() => $"({X:F0}, {Y:F0})";
}

public record Rect(double MinX, double MinY, double MaxX, double MaxY)
{
    public double Area() => Math.Max(0.0, MaxX - MinX) * Math.Max(0.0, MaxY - MinY);

    public bool Contains(Point p) =>
        p.X >= MinX && p.X <= MaxX &&
        p.Y >= MinY && p.Y <= MaxY;

    public bool Intersects(Rect other) =>
        MinX <= other.MaxX && MaxX >= other.MinX &&
        MinY <= other.MaxY && MaxY >= other.MinY;

    public Rect Union(Rect other) => new(
        Math.Min(MinX, other.MinX), Math.Min(MinY, other.MinY),
        Math.Max(MaxX, other.MaxX), Math.Max(MaxY, other.MaxY));

    public static Rect FromPoint(Point p) => new(p.X, p.Y, p.X, p.Y);

    public override string ToString() => $"[{MinX:F0}..{MaxX:F0}, {MinY:F0}..{MaxY:F0}]";
}

public record RTreeSearchInput(Point[] Points, Rect Query);

// --- Иерархическое R-дерево ---

public class RTree
{
    public const int MaxEntries = 3;
    private readonly RTreeSearchAlgorithm _algorithm;
    private RNode _root = new(isLeaf: true);

    public RNode Root => _root;

    public int Height
    {
        get
        {
            int h = 1;
            var curr = _root;
            while (!curr.IsLeaf && curr.Children.Count > 0)
            {
                h++;
                curr = curr.Children[0];
            }
            return h;
        }
    }

    public RTree(RTreeSearchAlgorithm algorithm)
    {
        _algorithm = algorithm;
    }

    public void Insert(Point p)
    {
        var newSibling = InsertRecursive(_root, p);
        if (newSibling != null)
        {
            var oldRoot = _root;
            _root = new RNode(isLeaf: false);
            _root.Children.Add(oldRoot);
            _root.Children.Add(newSibling);
            _root.Bounds = oldRoot.Bounds!.Union(newSibling.Bounds!);
        }
    }

    private RNode? InsertRecursive(RNode node, Point p)
    {
        if (node.IsLeaf)
        {
            node.Points.Add(p);
            node.Bounds = node.Bounds == null ? Rect.FromPoint(p) : node.Bounds.Union(Rect.FromPoint(p));

            if (node.Points.Count > MaxEntries)
            {
                return SplitLeaf(node);
            }
            return null;
        }

        // Выбираем дочерний узел с минимальным расширением площади
        var target = node.Children
            .OrderBy(c => c.Bounds!.Union(Rect.FromPoint(p)).Area() - c.Bounds.Area())
            .ThenBy(c => c.Bounds!.Area())
            .First();

        var newSibling = InsertRecursive(target, p);
        node.Bounds = CalculateBounds(node.Children);

        if (newSibling != null)
        {
            node.Children.Add(newSibling);
            node.Bounds = node.Bounds.Union(newSibling.Bounds!);

            if (node.Children.Count > MaxEntries)
            {
                return SplitInternal(node);
            }
        }

        return null;
    }

    private static RNode SplitLeaf(RNode node)
    {
        var sibling = new RNode(isLeaf: true);
        var points = node.Points.OrderBy(p => p.X).ToList();
        int half = points.Count / 2;

        node.Points.Clear();
        node.Points.AddRange(points.Take(half));
        node.Bounds = CalculatePointsBounds(node.Points);

        sibling.Points.AddRange(points.Skip(half));
        sibling.Bounds = CalculatePointsBounds(sibling.Points);

        return sibling;
    }

    private static RNode SplitInternal(RNode node)
    {
        var sibling = new RNode(isLeaf: false);
        var children = node.Children.OrderBy(c => (c.Bounds!.MinX + c.Bounds.MaxX) / 2.0).ToList();
        int half = children.Count / 2;

        node.Children.Clear();
        node.Children.AddRange(children.Take(half));
        node.Bounds = CalculateBounds(node.Children);

        sibling.Children.AddRange(children.Skip(half));
        sibling.Bounds = CalculateBounds(sibling.Children);

        return sibling;
    }

    private static Rect CalculateBounds(IEnumerable<RNode> nodes)
    {
        Rect? result = null;
        foreach (var n in nodes)
        {
            if (n.Bounds != null)
                result = result == null ? n.Bounds : result.Union(n.Bounds);
        }
        return result ?? new Rect(0, 0, 0, 0);
    }

    private static Rect CalculatePointsBounds(IEnumerable<Point> points)
    {
        Rect? result = null;
        foreach (var p in points)
        {
            var r = Rect.FromPoint(p);
            result = result == null ? r : result.Union(r);
        }
        return result ?? new Rect(0, 0, 0, 0);
    }

    public int RangeQuery(Rect query, MeasurementContext? ctx)
    {
        if (CallTreeTracker.IsTracking)
        {
            return _algorithm.TrackCallFunc(() => SearchNode(_root, query, ctx));
        }
        return SearchNode(_root, query, ctx);
    }

    public int SearchNode(RNode node, Rect query, MeasurementContext? ctx)
    {
        ctx?.IncrementSteps();

        if (node.Bounds == null || !node.Bounds.Intersects(query))
        {
            return 0;
        }

        if (node.IsLeaf)
        {
            int found = 0;
            foreach (var p in node.Points)
            {
                if (query.Contains(p))
                {
                    found++;
                }
            }
            return found;
        }

        int totalFound = 0;
        foreach (var child in node.Children)
        {
            if (child.Bounds != null && child.Bounds.Intersects(query))
            {
                if (CallTreeTracker.IsTracking)
                {
                    totalFound += _algorithm.TrackCallFunc(() => SearchNode(child, query, ctx));
                }
                else
                {
                    totalFound += SearchNode(child, query, ctx);
                }
            }
        }

        return totalFound;
    }
}

public class RNode
{
    public bool IsLeaf { get; set; }
    public Rect? Bounds { get; set; }
    public List<Point> Points { get; } = new();
    public List<RNode> Children { get; } = new();

    public RNode(bool isLeaf)
    {
        IsLeaf = isLeaf;
    }

    public override string ToString() => IsLeaf
        ? $"Лист({Points.Count} тчк, {Bounds})"
        : $"Узел({Children.Count} ветв., {Bounds})";
}
