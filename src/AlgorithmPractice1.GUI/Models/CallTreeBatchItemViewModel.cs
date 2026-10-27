using System.Collections.ObjectModel;
using AlgorithmPractice1.Core.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AlgorithmPractice1.GUI.Models;

/// <summary>
/// Представление выполненного пакета со списком обнаруженных рекурсивных алгоритмов.
/// </summary>
public partial class CallTreeBatchItemViewModel : ObservableObject
{
    public int BatchId { get; }
    public string Title { get; }
    public DateTime CreatedAt { get; }

    public ObservableCollection<CallTreeAlgorithmItemViewModel> RecursiveAlgorithms { get; } = new();

    public string DisplayText => $"{Title} ({CreatedAt:HH:mm:ss}) — {RecursiveAlgorithms.Count} рекурсивн.";

    public CallTreeBatchItemViewModel(int batchId, string title, IEnumerable<IAlgorithm> recursiveAlgorithms)
    {
        BatchId = batchId;
        Title = title;
        CreatedAt = DateTime.Now;

        foreach (var alg in recursiveAlgorithms)
        {
            RecursiveAlgorithms.Add(new CallTreeAlgorithmItemViewModel(alg));
        }
    }
}
