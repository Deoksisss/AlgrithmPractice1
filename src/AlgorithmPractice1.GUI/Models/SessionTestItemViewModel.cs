using System;
using AlgorithmPractice1.Core.Models;
using AlgorithmPractice1.Core.Registry;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AlgorithmPractice1.GUI.Models;

/// <summary>
/// Модель представления конкретного теста (прогона алгоритма) внутри сессии.
/// </summary>
public partial class SessionTestItemViewModel : ObservableObject
{
    public BenchmarkSession Session { get; }
    public SessionAlgorithm Algorithm { get; }

    public long SessionId => Session.Id;
    public string AlgorithmId => Algorithm.AlgorithmId;

    public string DisplayName { get; }
    public bool Is3D => AlgorithmId == "MatrixMultiply";

    public string DimensionBadge => Is3D ? "3D" : "2D";
    public string BadgeBackground => Is3D ? "#FFF1E5" : "#DDF4FF";
    public string BadgeForeground => Is3D ? "#BC4C00" : "#0969DA";

    public string ConfigSummary { get; }

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Opacity))]
    private bool _isEnabled = true;

    public double Opacity => IsEnabled ? 1.0 : 0.35;

    public SessionTestItemViewModel(
        BenchmarkSession session,
        SessionAlgorithm algorithm,
        Action? onSelectionChanged = null)
    {
        Session = session;
        Algorithm = algorithm;

        var meta = AlgorithmRegistry.Instance.FindById(algorithm.AlgorithmId);
        DisplayName = meta?.DisplayName ?? algorithm.AlgorithmId;

        var cfg = algorithm.Config;
        if (Is3D)
        {
            int mMax = cfg.M.GetValueOrDefault(cfg.NMax);
            int mStep = cfg.MStep.GetValueOrDefault(cfg.NStep);
            ConfigSummary = $"N: {cfg.NStep}..{cfg.NMax}, M: {mStep}..{mMax}";
        }
        else
        {
            string extra = string.Empty;
            if (cfg.K.HasValue) extra += $", K={cfg.K.Value}";
            if (cfg.X.HasValue) extra += $", X={cfg.X.Value:F1}";
            ConfigSummary = $"N: {cfg.NStep}..{cfg.NMax}, повторов: {cfg.RunsPerN}{extra}";
        }

        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IsSelected))
            {
                onSelectionChanged?.Invoke();
            }
        };
    }
}
