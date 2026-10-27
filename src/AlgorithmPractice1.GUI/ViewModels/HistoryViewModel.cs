using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AlgorithmPractice1.Core.Data;
using AlgorithmPractice1.Core.Models;
using AlgorithmPractice1.GUI.Models;
using AlgorithmPractice1.GUI.Services;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgorithmPractice1.GUI.ViewModels;

public partial class HistoryViewModel : ViewModelBase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly Dictionary<long, BenchmarkSession> _loadedSessionCache = new();

    public ObservableCollection<SessionListItemViewModel> Sessions { get; } = new();

    [ObservableProperty]
    private ChartDisplayModel? _activeChart;

    [ObservableProperty]
    private bool _hasActiveChart;

    [ObservableProperty]
    private string _statusMessage = "Загрузка истории сессий...";

    [ObservableProperty]
    private bool _isComparisonMode;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _selectedTestsCount;

    [ObservableProperty]
    private string _chartModeBadge = string.Empty;

    [ObservableProperty]
    private bool _hasChartModeBadge;

    public Action<Action>? DispatcherInvoker { get; set; }

    private async Task RunOnUiAsync(Action action)
    {
        if (DispatcherInvoker != null)
        {
            DispatcherInvoker(action);
            return;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(action);
        }
    }

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
        _loadedSessionCache.Clear();

        try
        {
            var sessions = await _sessionRepository.GetAllSessionsAsync();

            await RunOnUiAsync(() =>
            {
                Sessions.Clear();
                foreach (var s in sessions)
                {
                    Sessions.Add(new SessionListItemViewModel(s, OnTestSelectionChanged));
                }

                if (Sessions.Count > 0)
                {
                    // Раскрываем самую свежую сессию
                    var latest = Sessions[0];
                    latest.IsExpanded = true;

                    // Выбираем по умолчанию 2D тесты свежей сессии (или 3D если 2D нет)
                    var initialTests = latest.Tests.Where(t => !t.Is3D).ToList();
                    if (initialTests.Count == 0)
                    {
                        initialTests = latest.Tests.Where(t => t.Is3D).ToList();
                    }

                    foreach (var t in initialTests)
                    {
                        t.IsSelected = true;
                    }

                    OnTestSelectionChanged();
                }
                else
                {
                    StatusMessage = "Сессий пока нет. Запустите эксперимент во вкладке 'Настройка'.";
                    ActiveChart = null;
                    HasActiveChart = false;
                    ChartModeBadge = string.Empty;
                    HasChartModeBadge = false;
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

    internal void OnTestSelectionChanged()
    {
        _ = ApplyTestSelectionAsync();
    }

    public async Task ApplyTestSelectionAsync()
    {
        try
        {
            // 1. Собираем все выбранные тесты
            var selectedTests = Sessions
                .SelectMany(s => s.Tests)
                .Where(t => t.IsSelected)
                .ToList();

            bool has2D = selectedTests.Any(t => !t.Is3D);
            bool has3D = selectedTests.Any(t => t.Is3D);

            // 2. Реализуем взаимное ограничение 2D и 3D (Requirement 4)
            foreach (var sessionVm in Sessions)
            {
                bool anyTestEnabled = false;
                foreach (var test in sessionVm.Tests)
                {
                    if (has2D)
                    {
                        test.IsEnabled = !test.Is3D;
                    }
                    else if (has3D)
                    {
                        test.IsEnabled = test.Is3D;
                    }
                    else
                    {
                        test.IsEnabled = true;
                    }

                    if (test.IsEnabled)
                    {
                        anyTestEnabled = true;
                    }
                }

                sessionVm.IsSessionEnabled = anyTestEnabled;
                sessionVm.UpdateCheckStateFromTests();
            }

            SelectedTestsCount = selectedTests.Count;

            if (selectedTests.Count == 0)
            {
                await RunOnUiAsync(() =>
                {
                    ActiveChart = null;
                    HasActiveChart = false;
                    IsComparisonMode = false;
                    ChartModeBadge = string.Empty;
                    HasChartModeBadge = false;
                    StatusMessage = "Выберите один или несколько тестов в списке слева для отображения на общем графике.";
                });
                return;
            }

            IsComparisonMode = selectedTests.Count > 1;
            IsLoading = true;

            // 3. Загружаем полные данные измерений для выбранных сессий
            var fullSelectedTests = new List<(BenchmarkSession Session, SessionAlgorithm SessionAlg)>();
            var bySession = selectedTests.GroupBy(t => t.SessionId);

            foreach (var group in bySession)
            {
                long sessionId = group.Key;
                if (!_loadedSessionCache.TryGetValue(sessionId, out var fullSession))
                {
                    fullSession = await _sessionRepository.GetSessionByIdAsync(sessionId);
                    if (fullSession != null)
                    {
                        _loadedSessionCache[sessionId] = fullSession;
                    }
                }

                if (fullSession != null)
                {
                    foreach (var testVm in group)
                    {
                        var fullSa = fullSession.Algorithms.FirstOrDefault(a => a.Id == testVm.Algorithm.Id)
                                     ?? fullSession.Algorithms.FirstOrDefault(a => a.AlgorithmId == testVm.AlgorithmId);
                        if (fullSa != null)
                        {
                            fullSelectedTests.Add((fullSession, fullSa));
                        }
                    }
                }
            }

            // 4. Строим единый общий график (Requirement 3)
            var chart = ChartBuilderService.BuildUnifiedChart(fullSelectedTests);

            await RunOnUiAsync(() =>
            {
                ActiveChart = chart;
                HasActiveChart = chart != null;

                if (has3D)
                {
                    ChartModeBadge = "3D режим";
                    HasChartModeBadge = true;
                    StatusMessage = selectedTests.Count == 1
                        ? $"3D график: {selectedTests[0].DisplayName} (2D тесты заблокированы)."
                        : $"3D график: сравнение {selectedTests.Count} тестов (2D тесты заблокированы).";
                }
                else
                {
                    ChartModeBadge = "2D режим";
                    HasChartModeBadge = true;
                    StatusMessage = selectedTests.Count == 1
                        ? $"2D график: {selectedTests[0].DisplayName} (3D тесты заблокированы)."
                        : $"2D график: {selectedTests.Count} тестов на одном полотне (3D тесты заблокированы).";
                }
            });
        }
        catch (Exception ex)
        {
            await RunOnUiAsync(() =>
            {
                StatusMessage = $"Ошибка загрузки графиков: {ex.Message}";
            });
        }
        finally
        {
            await RunOnUiAsync(() =>
            {
                IsLoading = false;
            });
        }
    }

    [RelayCommand]
    private async Task DeleteSelectedSessions()
    {
        var sessionsToDelete = Sessions
            .Where(s => s.IsSessionChecked == true || s.Tests.Any(t => t.IsSelected))
            .ToList();

        if (sessionsToDelete.Count == 0) return;

        foreach (var item in sessionsToDelete)
        {
            await _sessionRepository.DeleteSessionAsync(item.Id);
            _loadedSessionCache.Remove(item.Id);
        }

        await LoadSessionsAsync();
    }

    [RelayCommand]
    private void SelectAllSessions()
    {
        // Выбираем все активные/доступные тесты
        bool currently3D = Sessions.SelectMany(s => s.Tests).Any(t => t.IsSelected && t.Is3D);

        foreach (var s in Sessions)
        {
            foreach (var t in s.Tests)
            {
                if (currently3D)
                {
                    if (t.Is3D && t.IsEnabled) t.IsSelected = true;
                }
                else
                {
                    if (!t.Is3D && t.IsEnabled) t.IsSelected = true;
                }
            }
        }
    }

    [RelayCommand]
    private void DeselectAllSessions()
    {
        foreach (var s in Sessions)
        {
            foreach (var t in s.Tests)
            {
                t.IsSelected = false;
            }
        }
    }
}
