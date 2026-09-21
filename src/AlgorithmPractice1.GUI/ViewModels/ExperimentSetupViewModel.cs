using System.Collections.ObjectModel;
using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Execution;
using AlgorithmPractice1.Core.Models;
using AlgorithmPractice1.Core.Registry;
using AlgorithmPractice1.GUI.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgorithmPractice1.GUI.ViewModels;

public partial class ExperimentSetupViewModel : ViewModelBase
{
    public ObservableCollection<AlgorithmSelectionItem> Algorithms { get; } = new();

    [ObservableProperty]
    private string _sessionLabel = string.Empty;

    [ObservableProperty]
    private AlgorithmSelectionItem? _selectedItem;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public event Action<IReadOnlyList<ExperimentRequest>, string?>? StartExperimentRequested;

    public ExperimentSetupViewModel()
    {
        foreach (var alg in AlgorithmRegistry.Instance.All)
        {
            Algorithms.Add(new AlgorithmSelectionItem(alg));
        }

        if (Algorithms.Count > 0)
        {
            SelectedItem = Algorithms[0];
        }
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var item in Algorithms)
        {
            item.IsSelected = true;
        }
    }

    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var item in Algorithms)
        {
            item.IsSelected = false;
        }
    }

    [RelayCommand]
    private void ResetDefaults()
    {
        if (SelectedItem != null)
        {
            SelectedItem.ResetToDefaults();
        }
    }

    [RelayCommand]
    private void ResetAllDefaults()
    {
        foreach (var item in Algorithms)
        {
            item.ResetToDefaults();
        }
    }

    [RelayCommand]
    private void RunExperiment()
    {
        ErrorMessage = string.Empty;

        var selected = Algorithms.Where(a => a.IsSelected).ToList();
        if (selected.Count == 0)
        {
            ErrorMessage = "Выберите хотя бы один алгоритм для проведения эксперимента.";
            return;
        }

        var requests = selected.Select(s => new ExperimentRequest(s.Algorithm, s.ToConfig())).ToList();
        string? label = string.IsNullOrWhiteSpace(SessionLabel) ? null : SessionLabel.Trim();

        StartExperimentRequested?.Invoke(requests, label);
    }
}
