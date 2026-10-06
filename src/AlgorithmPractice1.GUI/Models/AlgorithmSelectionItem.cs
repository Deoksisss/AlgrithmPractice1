using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgorithmPractice1.GUI.Models;

public partial class AlgorithmSelectionItem : ObservableObject
{
    private bool _isResetting;

    public IAlgorithm Algorithm { get; }

    [ObservableProperty]
    private bool _isSelected = true;

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private bool _useCustomSettings;

    [ObservableProperty]
    private int _nMax;

    [ObservableProperty]
    private int _nStep;

    [ObservableProperty]
    private int _runsPerN;

    [ObservableProperty]
    private int? _m;

    [ObservableProperty]
    private int? _mStep;

    [ObservableProperty]
    private int? _k;

    [ObservableProperty]
    private double? _x;

    [ObservableProperty]
    private bool _forceRecalculate;

    public string CategoryDisplayName => Algorithm.Category switch
    {
        AlgorithmCategory.Vectors => "Часть I. Векторы",
        AlgorithmCategory.Matrices => "Часть II. Матрицы",
        AlgorithmCategory.Individual => "Часть III. Индивидуальные",
        AlgorithmCategory.Exponentiation => "Часть IV. Возведение в степень",
        _ => "Алгоритмы"
    };

    public string ComplexityDisplayName => Algorithm.TheoreticalComplexity.ToDisplayName();

    public bool HasM => Algorithm.Category == AlgorithmCategory.Matrices;
    public bool HasK => Algorithm.Id == "MillerRabin";
    public bool HasX => Algorithm.Category == AlgorithmCategory.Exponentiation || Algorithm.Id.Contains("Polynomial");

    partial void OnNMaxChanged(int value)
    {
        if (HasM)
        {
            M = value;
        }
        if (!_isResetting)
        {
            UseCustomSettings = true;
        }
    }

    partial void OnNStepChanged(int value)
    {
        if (HasM)
        {
            MStep = value;
        }
        if (!_isResetting)
        {
            UseCustomSettings = true;
        }
    }

    partial void OnRunsPerNChanged(int value)
    {
        if (!_isResetting)
        {
            UseCustomSettings = true;
        }
    }

    partial void OnForceRecalculateChanged(bool value)
    {
        if (!_isResetting)
        {
            UseCustomSettings = true;
        }
    }

    public AlgorithmSelectionItem(IAlgorithm algorithm)
    {
        Algorithm = algorithm;
        ResetToDefaults();
    }

    [RelayCommand]
    public void ResetDefaults() => ResetToDefaults();

    public void ResetToDefaults()
    {
        _isResetting = true;
        try
        {
            var def = Algorithm.DefaultConfig;
            NMax = def.NMax;
            NStep = def.NStep;
            RunsPerN = def.RunsPerN;
            M = HasM ? def.NMax : def.M;
            MStep = HasM ? def.NStep : def.MStep;
            K = def.K;
            X = def.X;
            ForceRecalculate = false;
            UseCustomSettings = false;
        }
        finally
        {
            _isResetting = false;
        }
    }

    public ExperimentConfig ToConfig()
    {
        int effectiveNMax = NMax > 0 ? NMax : 100;
        int effectiveNStep = NStep > 0 ? NStep : 10;
        return new ExperimentConfig
        {
            NMax = effectiveNMax,
            NStep = effectiveNStep,
            RunsPerN = RunsPerN > 0 ? RunsPerN : 1,
            M = HasM ? effectiveNMax : null,
            MStep = HasM ? effectiveNStep : null,
            K = HasK ? (K ?? 20) : null,
            X = HasX ? (X ?? 1.5) : null,
            ForceRecalculate = ForceRecalculate
        };
    }

    public ExperimentConfig ToEffectiveConfig(int commonNMax, int commonNStep, int commonRuns, double commonX, bool commonForce)
    {
        if (UseCustomSettings)
        {
            return ToConfig();
        }

        int effectiveNMax = commonNMax > 0 ? commonNMax : 100;
        int effectiveNStep = commonNStep > 0 ? commonNStep : 10;
        int effectiveRuns = commonRuns > 0 ? commonRuns : 1;

        return new ExperimentConfig
        {
            NMax = effectiveNMax,
            NStep = effectiveNStep,
            RunsPerN = effectiveRuns,
            M = HasM ? effectiveNMax : null,
            MStep = HasM ? effectiveNStep : null,
            K = HasK ? (K ?? 20) : null,
            X = HasX ? (X ?? commonX) : null,
            ForceRecalculate = commonForce
        };
    }
}
