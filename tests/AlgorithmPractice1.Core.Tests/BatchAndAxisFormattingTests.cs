using System;
using System.Collections.Generic;
using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Execution;
using AlgorithmPractice1.Core.Models;
using AlgorithmPractice1.GUI.Models;
using AlgorithmPractice1.GUI.ViewModels;
using AvaloniaCharts.Controls;
using Xunit;

namespace AlgorithmPractice1.Core.Tests;

public class BatchAndAxisFormattingTests
{
    [Theory]
    [InlineData(0.0, "0")]
    [InlineData(5000.0, "5000")]
    [InlineData(10000.0, "10000")]
    [InlineData(120000000.0, "120000000")]
    [InlineData(250.0, "250")]
    [InlineData(0.5, "0.5")]
    [InlineData(12.345, "12.345")]
    public void FormatNumber_PrefersDecimalNotation(double input, string expected)
    {
        string actual = Chart2DControl.FormatNumber(input);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void FormatNumber_AstronomicalUsesScientific()
    {
        string big = Chart2DControl.FormatNumber(1e14);
        Assert.Contains("E", big, StringComparison.OrdinalIgnoreCase);

        string tiny = Chart2DControl.FormatNumber(1e-6);
        Assert.Contains("E", tiny, StringComparison.OrdinalIgnoreCase);
    }

    private class DummyAlgorithm : IAlgorithm
    {
        public string Id => "Dummy";
        public string DisplayName => "Тестовый алгоритм";
        public string Description => "Описание";
        public AlgorithmCategory Category => AlgorithmCategory.Vectors;
        public MeasurementType MeasurementType => MeasurementType.Time;
        public ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Linear;
        public ExperimentConfig DefaultConfig => new();
        public bool IsRecursive => false;
        public object GenerateInput(int n, ExperimentConfig config) => new double[n];
        public object Execute(object input, MeasurementContext? context) => 1.0;
        public AlgorithmPractice1.Core.CallTree.CallTreeNode? GenerateCallTree(int n, ExperimentConfig? config = null) => null;
    }

    [Fact]
    public void BatchProgressItemViewModel_LifecycleAndProperties()
    {
        var dummyAlg = new DummyAlgorithm();
        var requests = new List<ExperimentRequest>
        {
            new(dummyAlg, new ExperimentConfig()),
            new(dummyAlg, new ExperimentConfig())
        };

        var batch = new BatchProgressItemViewModel(1, "Тестовая сессия", requests);

        Assert.Equal(1, batch.BatchId);
        Assert.True(batch.IsQueued);
        Assert.False(batch.IsRunning);
        Assert.False(batch.IsCompleted);
        Assert.Equal("В очереди", batch.BadgeText);
        Assert.Equal("0/2", batch.FractionText);

        batch.MarkStarted();
        Assert.False(batch.IsQueued);
        Assert.True(batch.IsRunning);
        Assert.False(batch.IsCompleted);
        Assert.Equal("Выполняется", batch.BadgeText);

        batch.UpdateProgress(new SessionProgressUpdate
        {
            CompletedAlgorithms = 1,
            TotalAlgorithms = 2,
            Items = new List<AlgorithmProgressItem>
            {
                new() { AlgorithmId = "Dummy", Status = AlgorithmExecutionStatus.CompletedCalculated },
                new() { AlgorithmId = "Dummy", Status = AlgorithmExecutionStatus.Running }
            }
        });

        Assert.Equal("1/2", batch.FractionText);
        Assert.Equal(50.0, batch.Percentage);

        batch.MarkCompleted();
        Assert.False(batch.IsRunning);
        Assert.True(batch.IsCompleted);
        Assert.Equal("Завершён", batch.BadgeText);
    }

    [Fact]
    public void ExperimentProgressViewModel_MultiBatchQueueManagement()
    {
        var vm = new ExperimentProgressViewModel();
        Assert.False(vm.HasBatches);
        Assert.Empty(vm.Batches);

        var dummyAlg = new DummyAlgorithm();
        var requests = new List<ExperimentRequest> { new(dummyAlg, new ExperimentConfig()) };

        // 1. Добавляем 1-й пакет
        var b1 = vm.AddBatch("Пакет 1", requests);
        Assert.True(vm.HasBatches);
        Assert.Single(vm.Batches);
        Assert.Equal(1, b1.BatchId);

        // 2. Добавляем 2-й пакет (одновременный запуск нескольких пакетов)
        var b2 = vm.AddBatch("Пакет 2", requests);
        Assert.Equal(2, vm.Batches.Count);
        Assert.Equal(2, b2.BatchId);

        // Пакеты идут последовательно друг под другом в коллекции Batches
        Assert.Equal(b1, vm.Batches[0]);
        Assert.Equal(b2, vm.Batches[1]);

        // 3. Завершаем первый пакет
        b1.MarkCompleted();
        Assert.True(b1.IsCompleted);
        Assert.True(b2.IsQueued);

        // 4. Очистка завершённых пакетов
        vm.ClearCompletedBatches();
        Assert.Single(vm.Batches);
        Assert.Equal(b2, vm.Batches[0]);
    }
}
