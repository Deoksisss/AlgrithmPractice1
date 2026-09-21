using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Abstractions;

/// <summary>
/// Базовый интерфейс исследуемого алгоритма.
/// </summary>
public interface IAlgorithm
{
    /// <summary>
    /// Устойчивый строковый идентификатор алгоритма (константа для БД и реестра).
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Локализованное название алгоритма для интерфейса.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Краткое описание алгоритма и его роли в лабораторной работе.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Категория алгоритма по частям работы.
    /// </summary>
    AlgorithmCategory Category { get; }

    /// <summary>
    /// Что именно измеряется: время (Stopwatch) или шаги (MeasurementContext).
    /// </summary>
    MeasurementType MeasurementType { get; }

    /// <summary>
    /// Теоретическая сложность алгоритма (для подбора функции МНК).
    /// </summary>
    ComplexityFunctionType TheoreticalComplexity { get; }

    /// <summary>
    /// Рекомендуемая конфигурация по умолчанию (калиброванная для быстрого прогона).
    /// </summary>
    ExperimentConfig DefaultConfig { get; }

    /// <summary>
    /// Генерирует входные данные для алгоритма размера n (и m, k, x при наличии).
    /// </summary>
    object GenerateInput(int n, ExperimentConfig config);

    /// <summary>
    /// Выполняет алгоритм над сгенерированным входом.
    /// Для замера времени context равен null.
    /// Для алгоритмов Части IV context используется для подсчёта точного числа шагов.
    /// </summary>
    object? Execute(object input, MeasurementContext? context);
}

/// <summary>
/// Типизированный интерфейс алгоритма.
/// </summary>
public interface IAlgorithm<TInput, TOutput> : IAlgorithm
{
    TInput GenerateTypedInput(int n, ExperimentConfig config);
    TOutput ExecuteTyped(TInput input, MeasurementContext? context);
}

/// <summary>
/// Базовый абстрактный класс для реализации IAlgorithm.
/// </summary>
public abstract class AlgorithmBase<TInput, TOutput> : IAlgorithm<TInput, TOutput>
{
    public abstract string Id { get; }
    public abstract string DisplayName { get; }
    public abstract string Description { get; }
    public abstract AlgorithmCategory Category { get; }
    public abstract MeasurementType MeasurementType { get; }
    public abstract ComplexityFunctionType TheoreticalComplexity { get; }
    public abstract ExperimentConfig DefaultConfig { get; }

    public abstract TInput GenerateTypedInput(int n, ExperimentConfig config);
    public abstract TOutput ExecuteTyped(TInput input, MeasurementContext? context);

    public object GenerateInput(int n, ExperimentConfig config) =>
        GenerateTypedInput(n, config)!;

    public object? Execute(object input, MeasurementContext? context) =>
        ExecuteTyped((TInput)input, context);
}
