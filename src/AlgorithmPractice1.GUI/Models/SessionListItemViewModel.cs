using AlgorithmPractice1.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AlgorithmPractice1.GUI.Models;

public partial class SessionListItemViewModel : ObservableObject
{
    public BenchmarkSession Session { get; }

    [ObservableProperty]
    private bool _isSelected;

    public long Id => Session.Id;
    public string DisplayLabel => string.IsNullOrWhiteSpace(Session.Label) ? $"Сессия #{Session.Id}" : Session.Label;
    public string FormattedDate => DateTime.TryParse(Session.CreatedAt, out var dt)
        ? dt.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss")
        : Session.CreatedAt;

    public string AlgorithmsSummary => $"{Session.Algorithms.Count} алгоритмов: " +
        string.Join(", ", Session.Algorithms.Select(a => a.AlgorithmId));

    public SessionListItemViewModel(BenchmarkSession session, Action? onSelectionChanged = null)
    {
        Session = session;
        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IsSelected))
            {
                onSelectionChanged?.Invoke();
            }
        };
    }
}
