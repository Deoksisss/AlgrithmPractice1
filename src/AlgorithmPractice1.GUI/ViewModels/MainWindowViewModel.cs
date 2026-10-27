using AlgorithmPractice1.Core.Data;
using AlgorithmPractice1.Core.Execution;
using AlgorithmPractice1.Core.Models;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgorithmPractice1.GUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ExperimentRunner _experimentRunner;

    public ExperimentSetupViewModel Setup { get; }
    public ExperimentProgressViewModel Progress { get; }
    public HistoryViewModel History { get; }
    public CallTreeViewModel CallTree { get; }

    [ObservableProperty]
    private int _selectedTabIndex;

    public MainWindowViewModel(
        ExperimentRunner experimentRunner,
        ISessionRepository sessionRepository)
    {
        _experimentRunner = experimentRunner;

        Setup = new ExperimentSetupViewModel();
        Progress = new ExperimentProgressViewModel();
        History = new HistoryViewModel(sessionRepository);
        CallTree = new CallTreeViewModel();

        Setup.StartExperimentRequested += OnStartExperiment;
        Progress.NavigateToResultsRequested += OnNavigateToResults;
    }

    public async Task InitializeAsync()
    {
        await History.InitializeAsync();
    }

    private readonly System.Threading.SemaphoreSlim _executionLock = new(1, 1);

    private void OnStartExperiment(IReadOnlyList<ExperimentRequest> requests, string? label)
    {
        // Создаем карточку пакета в списке прогресса и переключаемся на вкладку
        var batchVm = Progress.AddBatch(label, requests);
        SelectedTabIndex = 1;

        // Последовательное выполнение пакетов в фоне через семафор
        Task.Run(async () =>
        {
            await _executionLock.WaitAsync();
            try
            {
                Dispatcher.UIThread.Post(() => batchVm.MarkStarted());

                var progressHandler = new Progress<SessionProgressUpdate>(update =>
                {
                    Dispatcher.UIThread.Post(() => batchVm.UpdateProgress(update));
                });

                var session = await _experimentRunner.RunBatchAsync(requests, label, progressHandler);

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    batchVm.MarkCompleted();
                    await History.LoadSessionsAsync();

                    var recursiveAlgs = requests
                        .Where(r => r.Algorithm.IsRecursive)
                        .Select(r => r.Algorithm)
                        .Distinct()
                        .ToList();

                    if (recursiveAlgs.Count > 0)
                    {
                        CallTree.RegisterCompletedBatch(label ?? $"Пакет #{batchVm.BatchId}", recursiveAlgs);
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    batchVm.MarkFailed(ex.Message);
                });
            }
            finally
            {
                _executionLock.Release();
            }
        });
    }

    private void OnNavigateToResults()
    {
        SelectedTabIndex = 2;
    }

    [RelayCommand]
    private void SelectTab(int index)
    {
        SelectedTabIndex = index;
    }
}
