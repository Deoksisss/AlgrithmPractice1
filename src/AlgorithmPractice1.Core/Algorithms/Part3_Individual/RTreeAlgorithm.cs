using System;
using System.Collections.Generic;
using System.Linq;
using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part3_Individual;

/// <summary>
/// Алгоритм пространственного диапазонного поиска с использованием R-дерева (сложность O(log n)).
/// </summary>
public sealed class RTreeSearchAlgorithm : AlgorithmBase<RTreeSearchInput, int>
{
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
        NMax = 100000, // Дерево строится долго, но поиск очень быстрый
        NStep = 10000,
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

        var query = new Rect(250.0, 250.0, 750.0, 750.0);

        // ВАЖНО: Строим дерево ЗДЕСЬ. Время построения не попадет в замеры.
        var tree = new RTree();
        foreach (var p in points)
        {
            tree.Insert(p);
        }

        return new RTreeSearchInput(points, query, tree);
    }

    public override int ExecuteTyped(RTreeSearchInput input, MeasurementContext? context)
    {
        // Замеряем ТОЛЬКО поиск в готовом дереве
        return input.Tree.RangeQuery(input.Query, context);
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

// Класс, который хранит готовое дерево
public class RTreeSearchInput
{
    public Point[] Points { get; }
    public Rect Query { get; }
    public RTree Tree { get; }

    public RTreeSearchInput(Point[] points, Rect query, RTree tree)
    {
        Points = points;
        Query = query;
        Tree = tree;
    }
}

// --- Иерархическое R-дерево ---

public class RTree
{
    public const int MaxEntries = 4; // Максимум детей в узле
    private RNode _root = new(isLeaf: true);

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

    public void Insert(Point p)
    {
        var newSibling = InsertRecursive(_root, p);
        if (newSibling != null)
        {
            // Если корень переполнился, создаем новый корень
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
        return SearchNode(_root, query, ctx);
    }

    private int SearchNode(RNode node, Rect query, MeasurementContext? ctx)
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
                totalFound += SearchNode(child, query, ctx);
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