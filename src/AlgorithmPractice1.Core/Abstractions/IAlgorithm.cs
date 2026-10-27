using System.Linq.Expressions;
using System.Reflection;
using AlgorithmPractice1.Core.CallTree;
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
    /// Флаг, указывающий, является ли данный алгоритм рекурсивным.
    /// </summary>
    bool IsRecursive { get; }

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

    /// <summary>
    /// Генерирует и возвращает дерево рекурсивных вызовов для заданного размера n.
    /// </summary>
    CallTreeNode? GenerateCallTree(int n, ExperimentConfig? config = null);
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

    public virtual bool IsRecursive => false;

    public abstract TInput GenerateTypedInput(int n, ExperimentConfig config);
    public abstract TOutput ExecuteTyped(TInput input, MeasurementContext? context);

    public object GenerateInput(int n, ExperimentConfig config) =>
        GenerateTypedInput(n, config)!;

    public object? Execute(object input, MeasurementContext? context) =>
        ExecuteTyped((TInput)input, context);

    /// <summary>
    /// Генерирует и строит дерево рекурсивных вызовов для размера n.
    /// </summary>
    public virtual CallTreeNode? GenerateCallTree(int n, ExperimentConfig? config = null)
    {
        if (!IsRecursive) return null;
        config ??= DefaultConfig;
        var input = GenerateTypedInput(n, config);

        using var tracker = new CallTreeTracker();

        var rootArgs = new List<(string Name, object? Value)>
        {
            ("n", n),
            ("input", input)
        };
        var rootNode = tracker.EnterCall(DisplayName, rootArgs);

        try
        {
            var result = ExecuteTyped(input, null);
            tracker.ExitCall(rootNode, result);
        }
        catch (Exception ex)
        {
            tracker.ExitCall(rootNode, $"<ошибка: {ex.Message}>");
        }

        return tracker.Root;
    }

    /// <summary>
    /// Метод отслеживания и выполнения вызова в дереве рекурсивных вызовов (для void-методов).
    /// </summary>
    public List<string> OutputCallTree(Expression<Action> expression)
    {
        var (methodName, args) = ParseExpression(expression.Body);
        var tracker = CallTreeTracker.Current;
        var node = tracker?.EnterCall(methodName, args);

        try
        {
            var action = expression.Compile();
            action();
        }
        finally
        {
            if (tracker != null && node != null)
            {
                tracker.ExitCall(node);
            }
        }

        return args.Select(a => $"{a.Name} = {CallTreeTracker.FormatValue(a.Value)}").ToList();
    }

    /// <summary>
    /// Метод отслеживания и выполнения вызова в дереве рекурсивных вызовов (для методов, возвращающих значение).
    /// </summary>
    public TResult OutputCallTree<TResult>(Expression<Func<TResult>> expression)
    {
        var (methodName, args) = ParseExpression(expression.Body);
        var tracker = CallTreeTracker.Current;
        var node = tracker?.EnterCall(methodName, args);

        TResult result;
        try
        {
            var func = expression.Compile();
            result = func();
        }
        finally
        {
            if (tracker != null && node != null)
            {
                tracker.ExitCall(node);
            }
        }

        if (node != null)
        {
            node.ReturnValueText = CallTreeTracker.FormatValue(result);
        }

        return result;
    }

    private static (string MethodName, List<(string Name, object? Value)> Args) ParseExpression(Expression expr)
    {
        if (expr is MethodCallExpression methodCall)
        {
            var methodName = methodCall.Method.Name;
            var parameters = methodCall.Method.GetParameters();
            var args = new List<(string Name, object? Value)>();

            for (int i = 0; i < parameters.Length; i++)
            {
                // Исключаем внутренний контекст измерений и кэш мемоизации из визуального дерева
                if (typeof(MeasurementContext).IsAssignableFrom(parameters[i].ParameterType) ||
                    parameters[i].Name == "cache" ||
                    typeof(System.Collections.IDictionary).IsAssignableFrom(parameters[i].ParameterType))
                    continue;

                object? val = GetValueFromExpression(methodCall.Arguments[i]);
                args.Add((parameters[i].Name ?? $"arg{i}", val));
            }

            return (methodName, args);
        }

        return ("UnknownCall", new List<(string Name, object? Value)>());
    }

    private static object? GetValueFromExpression(Expression expr)
    {
        if (expr is ConstantExpression constant)
            return constant.Value;

        if (expr is MemberExpression member && member.Expression is ConstantExpression closureConst)
        {
            var closureObj = closureConst.Value;
            if (member.Member is FieldInfo fieldInfo)
                return fieldInfo.GetValue(closureObj);
            if (member.Member is PropertyInfo propInfo)
                return propInfo.GetValue(closureObj);
        }

        try
        {
            var lambda = Expression.Lambda(expr);
            return lambda.Compile().DynamicInvoke();
        }
        catch
        {
            return "<не удалось вычислить>";
        }
    }
}
