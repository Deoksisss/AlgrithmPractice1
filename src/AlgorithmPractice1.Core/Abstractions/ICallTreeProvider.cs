using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Abstractions;

/// <summary>
/// Интерфейс для алгоритмов, которые могут предоставить дерево вызовов для визуализации.
/// </summary>
public interface ICallTreeProvider
{
    string Id { get; }
    string DisplayName { get; }
    
    /// <summary>
    /// Генерирует дерево вызовов для заданного N.
    /// </summary>
    List<AlgorithmCallNode> GenerateCallTree(int n);
}