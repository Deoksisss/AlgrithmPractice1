using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AlgorithmPractice1.Core.Data;
using AlgorithmPractice1.Core.Models;
using AlgorithmPractice1.GUI.Models;
using AlgorithmPractice1.GUI.Services;
using AlgorithmPractice1.GUI.ViewModels;
using Xunit;

namespace AlgorithmPractice1.Core.Tests;

public class UnifiedChartAndHistoryTests
{
    private static BenchmarkSession CreateDummySession(long id, string label, params string[] algorithmIds)
    {
        var session = new BenchmarkSession
        {
            Id = id,
            Label = label,
            CreatedAt = DateTime.UtcNow.ToString("O")
        };

        foreach (var algId in algorithmIds)
        {
            var cfg = new ExperimentConfig
            {
                NStep = 10,
                NMax = 50,
                M = algId == "MatrixMultiply" ? 30 : null,
                MStep = algId == "MatrixMultiply" ? 10 : null,
                RunsPerN = 3
            };

            var sa = new SessionAlgorithm
            {
                Id = id * 100 + session.Algorithms.Count + 1,
                SessionId = id,
                AlgorithmId = algId,
                ConfigJson = JsonSerializer.Serialize(cfg),
                ConfigHash = cfg.ComputeConfigHash()
            };

            static long ToTicks(double ms) => (long)(ms / 1000.0 * System.Diagnostics.Stopwatch.Frequency);

            if (algId == "MatrixMultiply")
            {
                sa.Measurements.Add(new MeasurementPoint { N = 10, M = 10, ElapsedTicks = ToTicks(1.2) });
                sa.Measurements.Add(new MeasurementPoint { N = 20, M = 20, ElapsedTicks = ToTicks(8.5) });
                sa.Measurements.Add(new MeasurementPoint { N = 30, M = 30, ElapsedTicks = ToTicks(27.1) });
            }
            else
            {
                sa.Measurements.Add(new MeasurementPoint { N = 10, ElapsedTicks = ToTicks(0.5) });
                sa.Measurements.Add(new MeasurementPoint { N = 20, ElapsedTicks = ToTicks(1.1) });
                sa.Measurements.Add(new MeasurementPoint { N = 30, ElapsedTicks = ToTicks(1.8) });
            }

            session.Algorithms.Add(sa);
        }

        return session;
    }

    private class MockSessionRepository : ISessionRepository
    {
        public List<BenchmarkSession> Sessions { get; } = new();

        public Task<long> CreateSessionAsync(BenchmarkSession session)
        {
            Sessions.Add(session);
            return Task.FromResult(session.Id);
        }

        public Task<IReadOnlyList<BenchmarkSession>> GetAllSessionsAsync()
        {
            return Task.FromResult<IReadOnlyList<BenchmarkSession>>(Sessions.ToList());
        }

        public Task<BenchmarkSession?> GetSessionByIdAsync(long sessionId)
        {
            return Task.FromResult(Sessions.FirstOrDefault(s => s.Id == sessionId));
        }

        public Task DeleteSessionAsync(long sessionId)
        {
            Sessions.RemoveAll(s => s.Id == sessionId);
            return Task.CompletedTask;
        }

        public Task UpdateSessionLabelAsync(long sessionId, string? label)
        {
            return Task.CompletedTask;
        }
    }

    [Fact]
    public void SessionTestItemViewModel_PropertiesAndBadges()
    {
        var session = CreateDummySession(1, "Тест", "LinearSearch", "MatrixMultiply");

        var test2D = new SessionTestItemViewModel(session, session.Algorithms[0]);
        var test3D = new SessionTestItemViewModel(session, session.Algorithms[1]);

        Assert.False(test2D.Is3D);
        Assert.Equal("2D", test2D.DimensionBadge);
        Assert.Equal(1.0, test2D.Opacity);

        Assert.True(test3D.Is3D);
        Assert.Equal("3D", test3D.DimensionBadge);
        Assert.Equal(1.0, test3D.Opacity);

        test2D.IsEnabled = false;
        Assert.Equal(0.35, test2D.Opacity);

        test3D.IsEnabled = false;
        Assert.Equal(0.35, test3D.Opacity);
    }

    [Fact]
    public void SessionListItemViewModel_TriStateSelectionSync()
    {
        var session = CreateDummySession(1, "Тест", "LinearSearch", "QuickSort");
        int changeNotified = 0;
        var vm = new SessionListItemViewModel(session, () => changeNotified++);

        Assert.Equal(2, vm.Tests.Count);
        Assert.False(vm.IsSessionChecked);
        Assert.Equal("0/2", vm.SelectedTestsCountSummary);

        // 1. Выбираем 1 тест -> неопределённое состояние null
        vm.Tests[0].IsSelected = true;
        Assert.Null(vm.IsSessionChecked);
        Assert.Equal("1/2", vm.SelectedTestsCountSummary);
        Assert.True(changeNotified > 0);

        // 2. Выбираем второй тест -> сессия выбрана полностью true
        vm.Tests[1].IsSelected = true;
        Assert.True(vm.IsSessionChecked);
        Assert.Equal("2/2", vm.SelectedTestsCountSummary);

        // 3. Снимаем галочку всей сессии -> все тесты снимаются
        vm.IsSessionChecked = false;
        Assert.All(vm.Tests, t => Assert.False(t.IsSelected));
        Assert.Equal("0/2", vm.SelectedTestsCountSummary);

        // 4. Ставим галочку всей сессии -> все тесты выбираются
        vm.IsSessionChecked = true;
        Assert.All(vm.Tests, t => Assert.True(t.IsSelected));
        Assert.Equal("2/2", vm.SelectedTestsCountSummary);
    }

    [Fact]
    public void ChartBuilderService_BuildUnifiedChart_2D_SingleAndMulti()
    {
        var session1 = CreateDummySession(1, "Сессия 1", "LinearSearch", "BinarySearch");
        var session2 = CreateDummySession(2, "Сессия 2", "LinearSearch");

        // Null/empty
        Assert.Null(ChartBuilderService.BuildUnifiedChart(new List<(BenchmarkSession, SessionAlgorithm)>()));

        // Single 2D
        var singleSelected = new List<(BenchmarkSession, SessionAlgorithm)>
        {
            (session1, session1.Algorithms[0])
        };
        var singleChart = ChartBuilderService.BuildUnifiedChart(singleSelected);
        Assert.NotNull(singleChart);
        Assert.False(singleChart.Is3D);
        Assert.NotEmpty(singleChart.Series2D!);
        Assert.Equal("LinearSearch", singleChart.AlgorithmId);

        // Multi 2D across sessions
        var multiSelected = new List<(BenchmarkSession, SessionAlgorithm)>
        {
            (session1, session1.Algorithms[0]),
            (session2, session2.Algorithms[0])
        };
        var multiChart = ChartBuilderService.BuildUnifiedChart(multiSelected);
        Assert.NotNull(multiChart);
        Assert.False(multiChart.Is3D);
        Assert.Equal("Multiple", multiChart.AlgorithmId);
        // Должны содержать теги сессий в легенде
        Assert.Contains(multiChart.Series2D!, s => s.Name.Contains("[Сессия 1]"));
        Assert.Contains(multiChart.Series2D!, s => s.Name.Contains("[Сессия 2]"));
    }

    [Fact]
    public void ChartBuilderService_BuildUnifiedChart_3D_Multi()
    {
        var session1 = CreateDummySession(1, "Сессия 1", "MatrixMultiply");
        var session2 = CreateDummySession(2, "Сессия 2", "MatrixMultiply");

        var selected = new List<(BenchmarkSession, SessionAlgorithm)>
        {
            (session1, session1.Algorithms[0]),
            (session2, session2.Algorithms[0])
        };

        var chart = ChartBuilderService.BuildUnifiedChart(selected);
        Assert.NotNull(chart);
        Assert.True(chart.Is3D);
        Assert.Equal("MatrixMultiply", chart.AlgorithmId);
        Assert.NotEmpty(chart.Series3D!);
        // Должны быть поверхности для каждой сессии
        Assert.Contains(chart.Series3D!, s => s.Name.Contains("[Сессия 1]"));
        Assert.Contains(chart.Series3D!, s => s.Name.Contains("[Сессия 2]"));
    }

    [Fact]
    public async Task HistoryViewModel_MutualExclusion_2D_And_3D()
    {
        var repo = new MockSessionRepository();
        var s1 = CreateDummySession(1, "Смешанная сессия", "LinearSearch", "MatrixMultiply");
        var s2 = CreateDummySession(2, "Вторая сессия", "QuickSort", "MatrixMultiply");
        repo.Sessions.Add(s1);
        repo.Sessions.Add(s2);

        var vm = new HistoryViewModel(repo)
        {
            DispatcherInvoker = action => action()
        };

        await vm.LoadSessionsAsync();

        Assert.Equal(2, vm.Sessions.Count);

        // Снимаем выбор со всех
        vm.DeselectAllSessionsCommand.Execute(null);
        await vm.ApplyTestSelectionAsync();

        // Проверяем, что когда ничего не выбрано — все тесты активны (IsEnabled=true, Opacity=1.0)
        var allTests = vm.Sessions.SelectMany(s => s.Tests).ToList();
        Assert.All(allTests, t =>
        {
            Assert.True(t.IsEnabled);
            Assert.Equal(1.0, t.Opacity);
        });
        Assert.False(vm.HasActiveChart);

        // 1. Выбираем 2D тест (LinearSearch в сессии 1)
        var linearSearchTest = vm.Sessions[0].Tests.First(t => t.AlgorithmId == "LinearSearch");
        linearSearchTest.IsSelected = true;
        await vm.ApplyTestSelectionAsync();

        // Все 3D тесты должны стать неактивными (IsEnabled=false, Opacity=0.35)
        var tests3D = allTests.Where(t => t.Is3D).ToList();
        var tests2D = allTests.Where(t => !t.Is3D).ToList();

        Assert.All(tests3D, t =>
        {
            Assert.False(t.IsEnabled);
            Assert.Equal(0.35, t.Opacity);
        });
        Assert.All(tests2D, t =>
        {
            Assert.True(t.IsEnabled);
            Assert.Equal(1.0, t.Opacity);
        });
        Assert.True(vm.HasActiveChart);
        Assert.False(vm.ActiveChart!.Is3D);
        Assert.Equal("2D режим", vm.ChartModeBadge);

        // 2. Снимаем 2D тест и выбираем 3D тест (MatrixMultiply)
        linearSearchTest.IsSelected = false;
        var matrixTest = vm.Sessions[0].Tests.First(t => t.AlgorithmId == "MatrixMultiply");
        matrixTest.IsSelected = true;
        await vm.ApplyTestSelectionAsync();

        // Все 2D тесты должны стать неактивными (IsEnabled=false, Opacity=0.35)
        Assert.All(tests2D, t =>
        {
            Assert.False(t.IsEnabled);
            Assert.Equal(0.35, t.Opacity);
        });
        // Все 3D тесты должны быть активны
        Assert.All(tests3D, t =>
        {
            Assert.True(t.IsEnabled);
            Assert.Equal(1.0, t.Opacity);
        });
        Assert.True(vm.HasActiveChart);
        Assert.True(vm.ActiveChart!.Is3D);
        Assert.Equal("3D режим", vm.ChartModeBadge);

        // 3. Снимаем все тесты — взаимная блокировка снимается
        matrixTest.IsSelected = false;
        await vm.ApplyTestSelectionAsync();

        Assert.All(allTests, t =>
        {
            Assert.True(t.IsEnabled);
            Assert.Equal(1.0, t.Opacity);
        });
        Assert.False(vm.HasActiveChart);
    }
}
