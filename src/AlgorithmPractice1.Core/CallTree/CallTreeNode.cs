namespace AlgorithmPractice1.Core.CallTree;

/// <summary>
/// Аргумент вызова метода.
/// </summary>
public sealed class CallArgument
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public CallArgument() { }

    public CallArgument(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public override string ToString() => $"{Name} = {Value}";
}

/// <summary>
/// Узел дерева рекурсивных вызовов.
/// </summary>
public sealed class CallTreeNode
{
    public int CallId { get; set; }
    public string MethodName { get; set; } = string.Empty;
    public List<CallArgument> Arguments { get; set; } = new();
    public string? ReturnValueText { get; set; }
    public int Depth { get; set; }
    public CallTreeNode? Parent { get; set; }
    public List<CallTreeNode> Children { get; set; } = new();

    /// <summary>
    /// Строковое представление параметров через запятую.
    /// </summary>
    public string ArgumentsSummary => string.Join(", ", Arguments.Select(a => $"{a.Name}: {a.Value}"));

    /// <summary>
    /// Полный текст вызова для отображения в интерфейсе.
    /// </summary>
    public string DisplayText
    {
        get
        {
            var call = string.IsNullOrEmpty(ArgumentsSummary)
                ? $"{MethodName}()"
                : $"{MethodName}({ArgumentsSummary})";

            return ReturnValueText != null ? $"{call} ➔ {ReturnValueText}" : call;
        }
    }

    /// <summary>
    /// Полное количество узлов в поддереве (включая текущий узел).
    /// </summary>
    public int SubtreeCount => 1 + Children.Sum(c => c.SubtreeCount);
}
