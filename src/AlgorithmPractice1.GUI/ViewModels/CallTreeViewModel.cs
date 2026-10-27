using System.Collections.ObjectModel;
using System.Diagnostics;
using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.CallTree;
using AlgorithmPractice1.Core.Registry;
using AlgorithmPractice1.GUI.Models;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgorithmPractice1.GUI.ViewModels;

/// <summary>
/// ViewModel страницы «Дерево рекурсивных вызовов».
/// </summary>
public partial class CallTreeViewModel : ViewModelBase
{
    private int _batchCounter;

    public ObservableCollection<CallTreeBatchItemViewModel> Batches { get; } = new();
    public ObservableCollection<CallTreeAlgorithmItemViewModel> AvailableAlgorithms { get; } = new();
    public ObservableCollection<CallTreeAlgorithmItemViewModel> DisplayedAlgorithms { get; } = new();

    [ObservableProperty]
    private CallTreeBatchItemViewModel? _selectedBatch;

    [ObservableProperty]
    private CallTreeAlgorithmItemViewModel? _selectedAlgorithm;

    [ObservableProperty]
    private int _inputN = 4;

    [ObservableProperty]
    private int _minN = 1;

    [ObservableProperty]
    private int _maxN = 10;

    [ObservableProperty]
    private bool _isGraphMode = true;

    [ObservableProperty]
    private CallTreeNode? _callTreeRoot;

    [ObservableProperty]
    private int _totalCalls;

    [ObservableProperty]
    private int _maxDepth;

    [ObservableProperty]
    private double _generationTimeMs;

    [ObservableProperty]
    private bool _isTruncated;

    [ObservableProperty]
    private bool _isBuilding;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public event Action? RequestCenterGraph;

    public CallTreeViewModel()
    {
        // Инициализируем список всех доступных в системе рекурсивных алгоритмов
        foreach (var alg in AlgorithmRegistry.Instance.All.Where(a => a.IsRecursive))
        {
            var item = new CallTreeAlgorithmItemViewModel(alg);
            AvailableAlgorithms.Add(item);
            DisplayedAlgorithms.Add(item);
        }

        if (DisplayedAlgorithms.Count > 0)
        {
            SelectAlgorithm(DisplayedAlgorithms[0]);
        }
    }

    /// <summary>
    /// Регистрирует завершённый пакет экспериментов и содержащиеся в нём рекурсивные алгоритмы.
    /// </summary>
    public void RegisterCompletedBatch(string batchTitle, IReadOnlyList<IAlgorithm> recursiveAlgorithms)
    {
        if (recursiveAlgorithms.Count == 0) return;

        _batchCounter++;
        var batchVm = new CallTreeBatchItemViewModel(_batchCounter, batchTitle, recursiveAlgorithms);
        Batches.Insert(0, batchVm);

        SelectedBatch = batchVm;
    }

    partial void OnSelectedBatchChanged(CallTreeBatchItemViewModel? value)
    {
        DisplayedAlgorithms.Clear();

        if (value != null && value.RecursiveAlgorithms.Count > 0)
        {
            foreach (var item in value.RecursiveAlgorithms)
            {
                DisplayedAlgorithms.Add(item);
            }
        }
        else
        {
            foreach (var item in AvailableAlgorithms)
            {
                DisplayedAlgorithms.Add(item);
            }
        }

        if (DisplayedAlgorithms.Count > 0)
        {
            SelectAlgorithm(DisplayedAlgorithms[0]);
        }
    }

    partial void OnSelectedAlgorithmChanged(CallTreeAlgorithmItemViewModel? value)
    {
        if (value == null) return;

        foreach (var item in DisplayedAlgorithms)
        {
            item.IsSelected = (item == value);
        }

        MinN = 1;
        MaxN = value.MaxAllowedN;
        InputN = Math.Clamp(value.SuggestedN, MinN, MaxN);

        RebuildTree();
    }

    partial void OnInputNChanged(int value)
    {
        RebuildTree();
    }

    [RelayCommand]
    public void SelectAlgorithm(CallTreeAlgorithmItemViewModel? item)
    {
        if (item == null) return;
        SelectedAlgorithm = item;
    }

    [RelayCommand]
    public void ShowAllAvailableAlgorithms()
    {
        SelectedBatch = null;
    }

    [RelayCommand]
    public void ToggleViewMode(string mode)
    {
        IsGraphMode = mode == "graph";
    }

    [RelayCommand]
    public void RebuildTree()
    {
        if (SelectedAlgorithm == null) return;

        IsBuilding = true;
        StatusMessage = "Построение дерева вызовов...";

        var alg = SelectedAlgorithm.Algorithm;
        int n = InputN;

        Task.Run(() =>
        {
            var sw = Stopwatch.StartNew();
            CallTreeNode? root = null;
            Exception? error = null;

            try
            {
                root = alg.GenerateCallTree(n);
            }
            catch (Exception ex)
            {
                error = ex;
            }
            sw.Stop();

            Dispatcher.UIThread.Post(() =>
            {
                IsBuilding = false;
                GenerationTimeMs = sw.Elapsed.TotalMilliseconds;

                if (error != null)
                {
                    StatusMessage = $"Ошибка при построении: {error.Message}";
                    CallTreeRoot = null;
                    TotalCalls = 0;
                    MaxDepth = 0;
                    IsTruncated = false;
                }
                else
                {
                    CallTreeRoot = root;
                    TotalCalls = root?.SubtreeCount ?? 0;
                    MaxDepth = CalculateMaxDepth(root);
                    IsTruncated = TotalCalls >= 2000;
                    StatusMessage = IsTruncated
                        ? "Дерево превысило порог в 2000 узлов и было частично усечено для плавности отображения."
                        : $"Дерево построено за {GenerationTimeMs:F1} мс ({TotalCalls} вызовов).";

                    RequestCenterGraph?.Invoke();
                }
            });
        });
    }

    private static int CalculateMaxDepth(CallTreeNode? node)
    {
        if (node == null) return 0;
        int max = node.Depth;
        foreach (var child in node.Children)
        {
            int d = CalculateMaxDepth(child);
            if (d > max) max = d;
        }
        return max;
    }
}
