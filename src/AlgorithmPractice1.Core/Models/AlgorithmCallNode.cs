namespace AlgorithmPractice1.Core.Models;

/// <summary>
/// Узел дерева вызовов рекурсивного алгоритма.
/// </summary>
public class AlgorithmCallNode
{
    public int Id { get; init; }
    public int? ParentId { get; init; }
    public int Depth { get; init; }
    public int[] ArrayState { get; init; } = Array.Empty<int>();
    public string Description { get; init; } = string.Empty;
    
    public int SwapIndexA { get; init; } = -1;
    public int SwapIndexB { get; init; } = -1;
    
    // Координаты для отрисовки (заполняются на этапе построения)
    public double X { get; set; }
    public double Y { get; set; }
}