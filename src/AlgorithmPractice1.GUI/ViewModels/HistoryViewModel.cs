using System.Collections.ObjectModel;
using AlgorithmPractice1.Core.Data;
using AlgorithmPractice1.Core.Models;
using AlgorithmPractice1.Core.Registry;
using AlgorithmPractice1.GUI.Models;
using AlgorithmPractice1.GUI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Threading;

namespace AlgorithmPractice1.GUI.ViewModels;

public partial class HistoryViewModel : ViewModelBase
{
    private readonly ISessionRepository _sessionRepository;

    public ObservableCollection<SessionListItemViewModel> Sessions { get; } = new();
    public ObservableCollection<ChartDisplayModel> DisplayCharts { get; } = new();

    [ObservableProperty]
    private string _statusMessage = "Загрузка истории сессий...";

    [ObservableProperty]
    private bool _isComparisonMode;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _selectedSessionsCount;

    public HistoryViewModel(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task InitializeAsync()
    {
        await LoadSessionsAsync();
    }

    [RelayCommand]
    public async Task LoadSessionsAsync()
    {
        IsLoading = true;
        StatusMessage = "Загрузка списка сессий...";

        try
        {
            var sessions = await _sessionRepository.GetAllSessionsAsync();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Sessions.Clear();
                foreach (var s in sessions)
                {
                    Sessions.Add(new SessionListItemViewModel(s, OnSessionSelectionChanged));
                }

                if (Sessions.Count > 0)
                {
                    // По умолчанию выбираем самую последнюю сессию
                    Sessions[0].IsSelected = true;
                }
                else
                {
                    StatusMessage = "Сессий пока нет. Запустите эксперимент во вкладке 'Настройка'.";
                    DisplayCharts.Clear();
                }
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки сессий: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async void OnSessionSelectionChanged()
    {
        try
        {
            var selected = Sessions.Where(s => s.IsSelected).ToList();
            SelectedSessionsCount = selected.Count;

            if (selected.Count == 0)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    DisplayCharts.Clear();
                    IsComparisonMode = false;
                    StatusMessage = "Выберите одну или несколько сессий в списке слева для просмотра графиков.";
                });
                return;
            }

            IsLoading = true;

            if (selected.Count == 1)
            {
                // Режим одной сессии
                long sessionId = selected[0].Id;
                var fullSession = await _sessionRepository.GetSessionByIdAsync(sessionId);

                var newCharts = new List<ChartDisplayModel>();
                string status = string.Empty;

                if (fullSession != null)
                {
                    status = $"Сессия #{fullSession.Id} ({fullSession.Label ?? "без метки"}), {fullSession.Algorithms.Count} алгоритмов.";

                    foreach (var sa in fullSession.Algorithms)
                    {
                        var algMeta = AlgorithmRegistry.Instance.FindById(sa.AlgorithmId);
                        string displayName = algMeta?.DisplayName ?? sa.AlgorithmId;
                        var chartModel = ChartBuilderService.BuildSingleAlgorithmChart(sa, displayName);
                        newCharts.Add(chartModel);
                    }
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    IsComparisonMode = false;
                    StatusMessage = status;
                    DisplayCharts.Clear();
                    foreach (var c in newCharts)
                    {
                        DisplayCharts.Add(c);
                    }
                });
            }
            else
            {
                // Режим сравнения нескольких сессий
                var fullSessions = new List<BenchmarkSession>();
                foreach (var s in selected)
                {
                    var fs = await _sessionRepository.GetSessionByIdAsync(s.Id);
                    if (fs != null) fullSessions.Add(fs);
                }

                // Находим все алгоритмы, присутствующие в выбранных сессиях
                var allAlgIds = fullSessions
                    .SelectMany(s => s.Algorithms.Select(a => a.AlgorithmId))
                    .Distinct()
                    .ToList();

                var newCharts = new List<ChartDisplayModel>();

                foreach (var algId in allAlgIds)
                {
                    var instances = new List<(BenchmarkSession Session, SessionAlgorithm SessionAlg)>();
                    foreach (var sess in fullSessions)
                    {
                        var match = sess.Algorithms.FirstOrDefault(a => a.AlgorithmId == algId);
                        if (match != null)
                        {
                            instances.Add((sess, match));
                        }
                    }

                    if (instances.Count > 0)
                    {
                        var algMeta = AlgorithmRegistry.Instance.FindById(algId);
                        string displayName = algMeta?.DisplayName ?? algId;
                        var compChart = ChartBuilderService.BuildComparisonChart(algId, displayName, instances);
                        newCharts.Add(compChart);
                    }
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    IsComparisonMode = true;
                    StatusMessage = $"Режим сравнения: выбрано {selected.Count} сессий.";
                    DisplayCharts.Clear();
                    foreach (var c in newCharts)
                    {
                        DisplayCharts.Add(c);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusMessage = $"Ошибка загрузки графиков: {ex.Message}";
            });
        }
        finally
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                IsLoading = false;
            });
        }
    }

    [RelayCommand]
    private async Task DeleteSelectedSessions()
    {
        var selected = Sessions.Where(s => s.IsSelected).ToList();
        if (selected.Count == 0) return;

        foreach (var item in selected)
        {
            await _sessionRepository.DeleteSessionAsync(item.Id);
        }

        await LoadSessionsAsync();
    }

    [RelayCommand]
    private void SelectAllSessions()
    {
        foreach (var s in Sessions)
        {
            s.IsSelected = true;
        }
    }

    [RelayCommand]
    private void DeselectAllSessions()
    {
        foreach (var s in Sessions)
        {
            s.IsSelected = false;
        }
    }
}
