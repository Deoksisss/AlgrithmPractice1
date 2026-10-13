using System;
using System.Collections.ObjectModel;
using System.Linq;
using AlgorithmPractice1.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AlgorithmPractice1.GUI.Models;

public partial class SessionListItemViewModel : ObservableObject
{
    private bool _isUpdatingFromTests;

    public BenchmarkSession Session { get; }
    public ObservableCollection<SessionTestItemViewModel> Tests { get; } = new();

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private bool? _isSessionChecked = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SessionOpacity))]
    private bool _isSessionEnabled = true;

    public double SessionOpacity => IsSessionEnabled ? 1.0 : 0.4;

    public long Id => Session.Id;
    public string DisplayLabel => string.IsNullOrWhiteSpace(Session.Label) ? $"Сессия #{Session.Id}" : Session.Label;
    public string FormattedDate => DateTime.TryParse(Session.CreatedAt, out var dt)
        ? dt.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss")
        : Session.CreatedAt;

    public string SelectedTestsCountSummary
    {
        get
        {
            int sel = Tests.Count(t => t.IsSelected);
            return $"{sel}/{Tests.Count}";
        }
    }

    public string AlgorithmsSummary => $"{Session.Algorithms.Count} тестов: " +
        string.Join(", ", Session.Algorithms.Select(a => a.AlgorithmId));

    public SessionListItemViewModel(
        BenchmarkSession session,
        Action? onTestSelectionChanged = null)
    {
        Session = session;

        foreach (var sa in session.Algorithms)
        {
            var testVm = new SessionTestItemViewModel(session, sa, () =>
            {
                UpdateCheckStateFromTests();
                onTestSelectionChanged?.Invoke();
            });
            Tests.Add(testVm);
        }

        UpdateCheckStateFromTests();
    }

    public void UpdateCheckStateFromTests()
    {
        _isUpdatingFromTests = true;
        try
        {
            var enabledTests = Tests.Where(t => t.IsEnabled).ToList();
            if (enabledTests.Count == 0)
            {
                IsSessionChecked = false;
            }
            else if (enabledTests.All(t => t.IsSelected))
            {
                IsSessionChecked = true;
            }
            else if (enabledTests.Any(t => t.IsSelected))
            {
                IsSessionChecked = null;
            }
            else
            {
                IsSessionChecked = false;
            }

            OnPropertyChanged(nameof(SelectedTestsCountSummary));
        }
        finally
        {
            _isUpdatingFromTests = false;
        }
    }

    partial void OnIsSessionCheckedChanged(bool? value)
    {
        if (_isUpdatingFromTests) return;

        if (value == true)
        {
            foreach (var t in Tests.Where(t => t.IsEnabled))
            {
                t.IsSelected = true;
            }
        }
        else if (value == false)
        {
            foreach (var t in Tests)
            {
                t.IsSelected = false;
            }
        }
    }
}
