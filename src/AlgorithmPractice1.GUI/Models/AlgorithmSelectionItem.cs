using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AlgorithmPractice1.GUI.Models;

public partial class AlgorithmSelectionItem : ObservableObject
{
    public IAlgorithm Algorithm { get; }

    [ObservableProperty]
    private bool _isSelected = true;

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
    }

    partial void OnNStepChanged(int value)
    {
        if (HasM)
        {
            MStep = value;
        }
    }

    public AlgorithmSelectionItem(IAlgorithm algorithm)
    {
        Algorithm = algorithm;
        ResetToDefaults();
    }

    public void ResetToDefaults()
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
            K = HasK ? K : null,
            X = HasX ? X : null,
            ForceRecalculate = ForceRecalculate
        };
    }
}
