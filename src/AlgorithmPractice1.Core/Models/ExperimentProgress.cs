namespace AlgorithmPractice1.Core.Models;

public enum AlgorithmExecutionStatus
{
    Queued,
    Running,
    CompletedCached,
    CompletedCalculated,
    Failed
}

public static class AlgorithmExecutionStatusExtensions
{
    public static string ToDisplayString(this AlgorithmExecutionStatus status) => status switch
    {
        AlgorithmExecutionStatus.Queued => "В очереди",
        AlgorithmExecutionStatus.Running => "Выполняется...",
        AlgorithmExecutionStatus.CompletedCached => "Готово (из кэша)",
        AlgorithmExecutionStatus.CompletedCalculated => "Готово (посчитано)",
        AlgorithmExecutionStatus.Failed => "Ошибка",
        _ => "В очереди"
    };

    public static string ToIcon(this AlgorithmExecutionStatus status) => status switch
    {
        AlgorithmExecutionStatus.Queued => "⏳",
        AlgorithmExecutionStatus.Running => "⚡",
        AlgorithmExecutionStatus.CompletedCached => "💾",
        AlgorithmExecutionStatus.CompletedCalculated => "✅",
        AlgorithmExecutionStatus.Failed => "❌",
        _ => "⏳"
    };
}

public class AlgorithmProgressItem : System.ComponentModel.INotifyPropertyChanged
{
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    public string AlgorithmId { get; init; } = string.Empty;
    public string AlgorithmName { get; init; } = string.Empty;

    private AlgorithmExecutionStatus _status = AlgorithmExecutionStatus.Queued;
    public AlgorithmExecutionStatus Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(StatusDisplayName));
                OnPropertyChanged(nameof(StatusIcon));
            }
        }
    }

    private int _currentN;
    public int CurrentN
    {
        get => _currentN;
        set
        {
            if (_currentN != value)
            {
                _currentN = value;
                OnPropertyChanged(nameof(CurrentN));
            }
        }
    }

    private int _totalNCount;
    public int TotalNCount
    {
        get => _totalNCount;
        set
        {
            if (_totalNCount != value)
            {
                _totalNCount = value;
                OnPropertyChanged(nameof(TotalNCount));
            }
        }
    }

    private string? _details;
    public string? Details
    {
        get => _details;
        set
        {
            if (_details != value)
            {
                _details = value;
                OnPropertyChanged(nameof(Details));
            }
        }
    }

    public string StatusDisplayName => Status.ToDisplayString();
    public string StatusIcon => Status.ToIcon();

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
}

public record SessionProgressUpdate
{
    public int CompletedAlgorithms { get; init; }
    public int TotalAlgorithms { get; init; }
    public IReadOnlyList<AlgorithmProgressItem> Items { get; init; } = Array.Empty<AlgorithmProgressItem>();
    public string? CurrentRunningAlgorithmId { get; init; }

    public string ProgressFraction => $"{CompletedAlgorithms}/{TotalAlgorithms}";
    public double Percentage => TotalAlgorithms > 0 ? (double)CompletedAlgorithms / TotalAlgorithms * 100.0 : 0.0;
}
