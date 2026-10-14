using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AlgorithmPractice1.Core.Execution;
using AlgorithmPractice1.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgorithmPractice1.GUI.Models;

/// <summary>
/// Модель представления отдельного пакета (запущенной сессии экспериментов) в очереди выполнения.
/// </summary>
public partial class BatchProgressItemViewModel : ObservableObject
{
    public int BatchId { get; }
    public string SessionLabel { get; }
    public string FormattedTime { get; }

    [ObservableProperty]
    private string _statusSummary = "В очереди на выполнение...";

    [ObservableProperty]
    private string _fractionText = "0/0";

    [ObservableProperty]
    private double _percentage = 0.0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BadgeText))]
    [NotifyPropertyChangedFor(nameof(BadgeBackground))]
    [NotifyPropertyChangedFor(nameof(BadgeForeground))]
    private bool _isRunning;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BadgeText))]
    [NotifyPropertyChangedFor(nameof(BadgeBackground))]
    [NotifyPropertyChangedFor(nameof(BadgeForeground))]
    private bool _isCompleted;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BadgeText))]
    [NotifyPropertyChangedFor(nameof(BadgeBackground))]
    [NotifyPropertyChangedFor(nameof(BadgeForeground))]
    private bool _isQueued = true;

    public string BadgeText => IsCompleted ? "Завершён" : (IsRunning ? "Выполняется" : "В очереди");
    public string BadgeBackground => IsCompleted ? "#DCFFE4" : (IsRunning ? "#DDF4FF" : "#F6F8FA");
    public string BadgeForeground => IsCompleted ? "#1A7F37" : (IsRunning ? "#0969DA" : "#57606A");

    public ObservableCollection<AlgorithmProgressItem> Items { get; } = new();

    public event Action? RequestNavigateToResults;

    public BatchProgressItemViewModel(int batchId, string? sessionLabel, IReadOnlyList<ExperimentRequest> requests)
    {
        BatchId = batchId;
        FormattedTime = DateTime.Now.ToString("HH:mm:ss");
        SessionLabel = string.IsNullOrWhiteSpace(sessionLabel)
            ? $"Пакет #{batchId} ({FormattedTime})"
            : $"{sessionLabel} [Пакет #{batchId}]";

        FractionText = $"0/{requests.Count}";
        StatusSummary = "Ожидание своей очереди в пуле задач...";

        foreach (var req in requests)
        {
            Items.Add(new AlgorithmProgressItem
            {
                AlgorithmId = req.Algorithm.Id,
                AlgorithmName = req.Algorithm.DisplayName,
                Status = AlgorithmExecutionStatus.Queued,
                CurrentN = 0,
                TotalNCount = 100,
                Details = "В очереди пакета"
            });
        }
    }

    public void MarkStarted()
    {
        IsQueued = false;
        IsRunning = true;
        IsCompleted = false;
        StatusSummary = $"Выполняется пакет #{BatchId}...";
    }

    public void MarkCompleted()
    {
        IsQueued = false;
        IsRunning = false;
        IsCompleted = true;
        Percentage = 100.0;
        StatusSummary = $"Пакет #{BatchId} успешно завершён!";
    }

    public void MarkFailed(string error)
    {
        IsQueued = false;
        IsRunning = false;
        IsCompleted = false;
        StatusSummary = $"Ошибка пакета #{BatchId}: {error}";
    }

    public void UpdateProgress(SessionProgressUpdate update)
    {
        FractionText = update.ProgressFraction;
        Percentage = update.Percentage;

        if (Items.Count != update.Items.Count)
        {
            Items.Clear();
            foreach (var itm in update.Items)
            {
                Items.Add(itm);
            }
        }
        else
        {
            for (int i = 0; i < update.Items.Count; i++)
            {
                var src = update.Items[i];
                var dst = Items[i];
                dst.Status = src.Status;
                dst.CurrentN = src.CurrentN;
                dst.TotalNCount = src.TotalNCount;
                dst.Details = src.Details;
            }
        }

        if (update.CompletedAlgorithms >= update.TotalAlgorithms && update.TotalAlgorithms > 0)
        {
            MarkCompleted();
        }
        else if (IsRunning)
        {
            StatusSummary = $"Выполняется: {update.CurrentRunningAlgorithmId ?? "алгоритм"} ({update.CompletedAlgorithms}/{update.TotalAlgorithms} завершено)";
        }
    }

    [RelayCommand]
    private void GoToResults()
    {
        RequestNavigateToResults?.Invoke();
    }
}
