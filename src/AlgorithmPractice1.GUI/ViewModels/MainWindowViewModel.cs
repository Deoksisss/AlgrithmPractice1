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

        Setup.StartExperimentRequested += OnStartExperiment;
        Progress.NavigateToResultsRequested += OnNavigateToResults;
    }

    public async Task InitializeAsync()
    {
        await History.InitializeAsync();
    }

    private void OnStartExperiment(IReadOnlyList<ExperimentRequest> requests, string? label)
    {
        // Переключаемся на вкладку прогресса
        SelectedTabIndex = 1;
        Progress.Reset(requests.Count);

        // Запуск длительной операции в фоне
        Task.Run(async () =>
        {
            var progressHandler = new Progress<SessionProgressUpdate>(update =>
            {
                Dispatcher.UIThread.Post(() => Progress.UpdateProgress(update));
            });

            try
            {
                var session = await _experimentRunner.RunBatchAsync(requests, label, progressHandler);

                // После завершения обновляем вкладку истории
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await History.LoadSessionsAsync();
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    Progress.StatusSummary = $"Ошибка при выполнении: {ex.Message}";
                    Progress.IsRunning = false;
                });
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
