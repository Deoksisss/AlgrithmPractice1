using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AlgorithmPractice1.Core.Execution;
using AlgorithmPractice1.Core.Models;
using AlgorithmPractice1.GUI.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgorithmPractice1.GUI.ViewModels;

public partial class ExperimentProgressViewModel : ViewModelBase
{
    private int _batchCounter;

    public ObservableCollection<BatchProgressItemViewModel> Batches { get; } = new();

    [ObservableProperty]
    private bool _hasBatches;

    [ObservableProperty]
    private string _queueSummary = "Нет запущенных пакетов.";

    public event Action? NavigateToResultsRequested;

    public BatchProgressItemViewModel AddBatch(string? label, IReadOnlyList<ExperimentRequest> requests)
    {
        _batchCounter++;
        var batchVm = new BatchProgressItemViewModel(_batchCounter, label, requests);
        batchVm.RequestNavigateToResults += () => NavigateToResultsRequested?.Invoke();

        Batches.Add(batchVm);
        HasBatches = true;
        UpdateQueueSummary();

        batchVm.PropertyChanged += (_, _) => UpdateQueueSummary();

        return batchVm;
    }

    public void UpdateQueueSummary()
    {
        int running = Batches.Count(b => b.IsRunning);
        int queued = Batches.Count(b => b.IsQueued);
        int completed = Batches.Count(b => b.IsCompleted);

        if (Batches.Count == 0)
        {
            QueueSummary = "Нет активных пакетов.";
        }
        else if (running > 0 || queued > 0)
        {
            QueueSummary = $"Активно: {running} выполняется, {queued} в очереди ожидания ({completed} завершено).";
        }
        else
        {
            QueueSummary = $"Все пакеты ({completed}) успешно выполнены!";
        }
    }

    [RelayCommand]
    public void ClearCompletedBatches()
    {
        var completed = Batches.Where(b => b.IsCompleted).ToList();
        foreach (var b in completed)
        {
            Batches.Remove(b);
        }
        HasBatches = Batches.Count > 0;
        UpdateQueueSummary();
    }

    [RelayCommand]
    private void GoToResults()
    {
        NavigateToResultsRequested?.Invoke();
    }
}
