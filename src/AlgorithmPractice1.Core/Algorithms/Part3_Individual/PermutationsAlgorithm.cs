using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part3_Individual;

/// <summary>
/// Генерация всех перестановок (Permutations, сложность O(n!))
/// </summary>
public sealed class PermutationsAlgorithm : AlgorithmBase<int[], long> , ICallTreeProvider
{
    public override string Id => "Permutations";
    public override string DisplayName => "Алгоритм генерации всех перестановок";
    public override string Description => "Дано множество из n элементов (например, {1, 2, 3}). Генерирует все возможные перестановки этих элементов.";
    public override AlgorithmCategory Category => AlgorithmCategory.Individual;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Faсtorial;
    
    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 10,
        NStep = 1,
        RunsPerN = 5
    };

    public override int[] GenerateTypedInput(int n, ExperimentConfig config)
    {
        var array = new int[n];
        for (int i = 0; i < n; i++)
        {
            array[i] = i+1;
        }
        return array;
    }
    
    public override long ExecuteTyped(int[] input, MeasurementContext? context)
    {
        long count = 0;
        Permute(input, 0, count, context);
        return count;
    }

    private void Permute(int[] input, int start,long count, MeasurementContext? context)
    {
        if (start == input.Length - 1)
        {
            count++;
            context?.IncrementSteps();
            return;
        }
        
        for (int i = start; i < input.Length; i++)
        {
            Swap(input, start, i);
            Permute(input, start + 1, count, context);
            Swap(input, start, i);
        }
    }
    
    private static void Swap<T>(T[] a, int i, int j)
    {
        if (i == j) return;
        (a[i], a[j]) = (a[j], a[i]);
    }
    
    public List<AlgorithmCallNode> GenerateCallTree(int n)
    {
        var nodes = new List<AlgorithmCallNode>();
        int nextId = 0;
        var input = new int[n];
        for (int i = 0; i < n; i++) input[i] = i + 1;

        BuildTreeRecursive(input, 0, 0, null, nodes, ref nextId);
        CalculateLayout(nodes);
        return nodes;
    }

    private void BuildTreeRecursive(int[] input, int start, int depth, int? parentId, List<AlgorithmCallNode> nodes, ref int nextId)
    {
        // Если это корень, создаем его без подсветки
        if (parentId == null)
        {
            int rootId = nextId++;
            nodes.Add(new AlgorithmCallNode
            {
                Id = rootId,
                ParentId = null,
                Depth = depth,
                ArrayState = (int[])input.Clone(),
                Description = "Старт",
                SwapIndexA = -1, SwapIndexB = -1
            });
            parentId = rootId;
        }

        if (start == input.Length - 1)
        {
            return; // Листья создаются в цикле ниже
        }

        for (int i = start; i < input.Length; i++)
        {
            // 1. Создаем узел ПОДГОТОВКИ (подсветка)
            int prepId = nextId++;
            nodes.Add(new AlgorithmCallNode
            {
                Id = prepId,
                ParentId = parentId,
                Depth = depth,
                ArrayState = (int[])input.Clone(), // Еще не поменяли
                Description = $"Меняем {start} и {i}",
                SwapIndexA = start,
                SwapIndexB = i
            });

            // 2. Делаем обмен
            Swap(input, start, i);

            // 3. Если это последний элемент, создаем ЛИСТ (итоговая перестановка)
            if (start == input.Length - 2)
            {
                nodes.Add(new AlgorithmCallNode
                {
                    Id = nextId++,
                    ParentId = prepId,
                    Depth = depth + 1,
                    ArrayState = (int[])input.Clone(), // Уже поменяли
                    Description = "Перестановка",
                    SwapIndexA = -1, SwapIndexB = -1
                });
            }
            else
            {
                // 4. Иначе уходим в рекурсию
                BuildTreeRecursive(input, start + 1, depth + 1, prepId, nodes, ref nextId);
            }

            // 5. Возврат
            Swap(input, start, i);
        }
    }

    private void CalculateLayout(List<AlgorithmCallNode> nodes)
{
    if (nodes.Count == 0) return;

    // Создаем словарь для быстрого поиска
    var nodeDict = nodes.ToDictionary(n => n.Id);
    
    // Находим корень (у него нет родителя)
    var root = nodes.FirstOrDefault(n => n.ParentId == null);
    if (root == null) return;

    // Собираем детей для каждого узла
    var childrenDict = new Dictionary<int, List<AlgorithmCallNode>>();
    foreach (var node in nodes)
    {
        if (node.ParentId.HasValue)
        {
            if (!childrenDict.ContainsKey(node.ParentId.Value))
                childrenDict[node.ParentId.Value] = new List<AlgorithmCallNode>();
            childrenDict[node.ParentId.Value].Add(node);
        }
    }

    // Переменная для отслеживания текущей X-координаты для листьев
    double nextLeafX = 0;
    
    // Вертикальный шаг между уровнями
    double levelHeight = 120;
    // Горизонтальный шаг между листьями
    double leafSpacing = 150;

    // Рекурсивная функция для расстановки
    void PositionNode(AlgorithmCallNode node)
    {
        node.Y = node.Depth * levelHeight;

        if (childrenDict.TryGetValue(node.Id, out var children) && children.Count > 0)
        {
            // Сначала расставляем всех детей
            foreach (var child in children)
            {
                PositionNode(child);
            }

            // Родитель становится ровно посередине между первым и последним ребенком
            double firstChildX = children.First().X;
            double lastChildX = children.Last().X;
            node.X = (firstChildX + lastChildX) / 2.0;
        }
        else
        {
            // Если это лист (нет детей), ставим его на текущую свободную позицию по X
            node.X = nextLeafX;
            nextLeafX += leafSpacing;
        }
    }

    // Запускаем расстановку с корня
    PositionNode(root);

    // Теперь нужно центрировать всё дерево относительно (0,0)
    // Находим минимальные и максимальные координаты
    double minX = nodes.Min(n => n.X);
    double maxX = nodes.Max(n => n.X);
    double maxY = nodes.Max(n => n.Y);

    // Вычисляем центр
    double centerX = (minX + maxX) / 2.0;
    
    // Сдвигаем все узлы, чтобы центр дерева был в (0,0)
    foreach (var node in nodes)
    {
        node.X -= centerX;
        node.Y -= maxY / 2.0; // Центрируем по вертикали тоже
    }
}
}