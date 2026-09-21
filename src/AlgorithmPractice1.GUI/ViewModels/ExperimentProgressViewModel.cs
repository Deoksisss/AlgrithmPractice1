using System.Collections.ObjectModel;
using AlgorithmPractice1.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgorithmPractice1.GUI.ViewModels;

public partial class ExperimentProgressViewModel : ViewModelBase
{
    public ObservableCollection<AlgorithmProgressItem> Items { get; } = new();

    [ObservableProperty]
    private int _completedCount;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private string _fractionText = "0/0";

    [ObservableProperty]
    private double _percentage;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private bool _isCompleted;

    [ObservableProperty]
    private string _statusSummary = "Ожидание запуска эксперимента";

    public event Action? NavigateToResultsRequested;

    public void Reset(int totalAlgorithms)
    {
        Items.Clear();
        TotalCount = totalAlgorithms;
        CompletedCount = 0;
        FractionText = $"0/{totalAlgorithms}";
        Percentage = 0.0;
        IsRunning = true;
        IsCompleted = false;
        StatusSummary = $"Выполняется пакетный запуск {totalAlgorithms} алгоритмов...";
    }

    public void UpdateProgress(SessionProgressUpdate update)
    {
        TotalCount = update.TotalAlgorithms;
        CompletedCount = update.CompletedAlgorithms;
        FractionText = update.ProgressFraction;
        Percentage = update.Percentage;

        // Синхронизируем коллекцию элементов
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

        if (CompletedCount >= TotalCount && TotalCount > 0)
        {
            IsRunning = false;
            IsCompleted = true;
            StatusSummary = "Пакетный запуск успешно завершён!";
        }
    }

    [RelayCommand]
    private void GoToResults()
    {
        NavigateToResultsRequested?.Invoke();
    }
}
