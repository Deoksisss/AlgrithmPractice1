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

    public ObservableCollection<AlgorithmSelectionItem> VectorAlgorithms { get; } = new();
    public ObservableCollection<AlgorithmSelectionItem> MatrixAlgorithms { get; } = new();
    public ObservableCollection<AlgorithmSelectionItem> IndividualAlgorithms { get; } = new();
    public ObservableCollection<AlgorithmSelectionItem> ExponentiationAlgorithms { get; } = new();

    [ObservableProperty]
    private string _sessionLabel = string.Empty;

    [ObservableProperty]
    private AlgorithmSelectionItem? _selectedIndividualAlgorithm;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _selectedCountText = string.Empty;

    // Общие параметры для всех алгоритмов
    [ObservableProperty]
    private int _commonNMax = 150;

    [ObservableProperty]
    private int _commonNStep = 25;

    [ObservableProperty]
    private int _commonRunsPerN = 3;

    [ObservableProperty]
    private int _commonK = 20;

    [ObservableProperty]
    private double _commonX = 1.5;

    [ObservableProperty]
    private bool _commonForceRecalculate = false;

    // Флаг использования индивидуальных настроек вместо общих
    [ObservableProperty]
    private bool _useIndividualSettings = false;

    public event Action<IReadOnlyList<ExperimentRequest>, string?>? StartExperimentRequested;

    public ExperimentSetupViewModel()
    {
        foreach (var alg in AlgorithmRegistry.Instance.All)
        {
            var item = new AlgorithmSelectionItem(alg);
            item.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AlgorithmSelectionItem.IsSelected))
                {
                    UpdateSelectedCountText();
                }
            };

            Algorithms.Add(item);

            switch (alg.Category)
            {
                case AlgorithmCategory.Vectors:
                    VectorAlgorithms.Add(item);
                    break;
                case AlgorithmCategory.Matrices:
                    MatrixAlgorithms.Add(item);
                    break;
                case AlgorithmCategory.Individual:
                    IndividualAlgorithms.Add(item);
                    break;
                case AlgorithmCategory.Exponentiation:
                    ExponentiationAlgorithms.Add(item);
                    break;
            }
        }

        if (Algorithms.Count > 0)
        {
            SelectedIndividualAlgorithm = Algorithms[0];
        }

        UpdateSelectedCountText();
    }

    private void UpdateSelectedCountText()
    {
        int selected = Algorithms.Count(a => a.IsSelected);
        SelectedCountText = $"Выбрано: {selected} из {Algorithms.Count}";
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var item in Algorithms) item.IsSelected = true;
        UpdateSelectedCountText();
    }

    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var item in Algorithms) item.IsSelected = false;
        UpdateSelectedCountText();
    }

    [RelayCommand]
    private void SelectCategory(string categoryName)
    {
        if (Enum.TryParse<AlgorithmCategory>(categoryName, true, out var cat))
        {
            foreach (var item in Algorithms)
            {
                item.IsSelected = (item.Algorithm.Category == cat);
            }
            UpdateSelectedCountText();
        }
    }

    [RelayCommand]
    private void ResetDefaults()
    {
        if (SelectedIndividualAlgorithm != null)
        {
            SelectedIndividualAlgorithm.ResetToDefaults();
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
    private void ApplyCommonToAll()
    {
        foreach (var item in Algorithms)
        {
            item.NMax = CommonNMax;
            item.NStep = CommonNStep;
            item.RunsPerN = CommonRunsPerN;
            item.ForceRecalculate = CommonForceRecalculate;

            if (item.HasM)
            {
                item.M = CommonNMax;
                item.MStep = CommonNStep;
            }

            if (item.HasK)
            {
                item.K = CommonK;
            }

            if (item.HasX)
            {
                item.X = CommonX;
            }
        }
    }

    [RelayCommand]
    private void ResetCommonDefaults()
    {
        CommonNMax = 150;
        CommonNStep = 25;
        CommonRunsPerN = 3;
        CommonK = 20;
        CommonX = 1.5;
        CommonForceRecalculate = false;
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

        List<ExperimentRequest> requests;
        if (!UseIndividualSettings)
        {
            int nMax = Math.Max(1, CommonNMax);
            int nStep = Math.Max(1, CommonNStep);
            int runs = Math.Max(1, CommonRunsPerN);

            requests = selected.Select(s =>
            {
                var config = new ExperimentConfig
                {
                    NMax = nMax,
                    NStep = nStep,
                    RunsPerN = runs,
                    M = s.HasM ? nMax : null,
                    MStep = s.HasM ? nStep : null,
                    K = s.HasK ? CommonK : null,
                    X = s.HasX ? CommonX : null,
                    ForceRecalculate = CommonForceRecalculate
                };
                return new ExperimentRequest(s.Algorithm, config);
            }).ToList();
        }
        else
        {
            requests = selected.Select(s => new ExperimentRequest(s.Algorithm, s.ToConfig())).ToList();
        }

        string? label = string.IsNullOrWhiteSpace(SessionLabel) ? null : SessionLabel.Trim();
        StartExperimentRequested?.Invoke(requests, label);
    }
}
