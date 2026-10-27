using System.Collections;

namespace AlgorithmPractice1.Core.CallTree;

/// <summary>
/// Сервис отслеживания и построения дерева рекурсивных вызовов в текущем потоке/асинхронном контексте.
/// </summary>
public sealed class CallTreeTracker : IDisposable
{
    private static readonly AsyncLocal<CallTreeTracker?> _current = new();

    /// <summary>
    /// Активный трекер для текущего асинхронного контекста.
    /// </summary>
    public static CallTreeTracker? Current => _current.Value;

    /// <summary>
    /// Флаг активности трекинга дерева вызовов.
    /// </summary>
    public static bool IsTracking => _current.Value != null;

    private readonly Stack<CallTreeNode> _callStack = new();
    private int _counter;

    public CallTreeNode? Root { get; private set; }
    public int TotalCalls => _counter;
    public int MaxDepth { get; private set; }
    public int MaxNodesLimit { get; set; }
    public bool WasTruncated { get; private set; }

    public CallTreeTracker(int maxNodesLimit = 2000)
    {
        MaxNodesLimit = maxNodesLimit;
        _current.Value = this;
    }

    /// <summary>
    /// Регистрирует вход в вызов функции.
    /// </summary>
    public CallTreeNode? EnterCall(string methodName, IReadOnlyList<(string Name, object? Value)> args)
    {
        _counter++;
        int depth = _callStack.Count + 1;
        if (depth > MaxDepth) MaxDepth = depth;

        if (_counter > MaxNodesLimit)
        {
            WasTruncated = true;
            return null;
        }

        var node = new CallTreeNode
        {
            CallId = _counter,
            MethodName = methodName,
            Depth = depth,
            Parent = _callStack.Count > 0 ? _callStack.Peek() : null
        };

        foreach (var (name, val) in args)
        {
            node.Arguments.Add(new CallArgument(name, FormatValue(val)));
        }

        if (node.Parent != null)
        {
            node.Parent.Children.Add(node);
        }
        else if (Root == null)
        {
            Root = node;
        }

        _callStack.Push(node);
        return node;
    }

    /// <summary>
    /// Регистрирует выход из вызова функции и сохраняет возвращённое значение.
    /// </summary>
    public void ExitCall(CallTreeNode? node, object? result = null)
    {
        if (node == null) return;

        if (result != null)
        {
            node.ReturnValueText = FormatValue(result);
        }

        if (_callStack.Count > 0 && _callStack.Peek() == node)
        {
            _callStack.Pop();
        }
    }

    public void Dispose()
    {
        if (_current.Value == this)
        {
            _current.Value = null;
        }
    }

    /// <summary>
    /// Форматирует значение параметра/результата для аккуратного отображения в дереве.
    /// </summary>
    public static string FormatValue(object? val)
    {
        if (val == null) return "null";

        if (val is Array arr)
        {
            if (arr.Length > 10)
            {
                var slice = new List<string>();
                for (int i = 0; i < 6; i++) slice.Add(arr.GetValue(i)?.ToString() ?? "null");
                return $"[{string.Join(", ", slice)}, ... ({arr.Length} эл.)]";
            }
            var items = new List<string>();
            foreach (var item in arr) items.Add(item?.ToString() ?? "null");
            return $"[{string.Join(", ", items)}]";
        }

        if (val is IEnumerable enumerable && !(val is string))
        {
            var items = new List<string>();
            int cnt = 0;
            foreach (var item in enumerable)
            {
                if (cnt++ > 8) { items.Add("..."); break; }
                items.Add(item?.ToString() ?? "null");
            }
            return $"[{string.Join(", ", items)}]";
        }

        return val.ToString() ?? "";
    }
}
