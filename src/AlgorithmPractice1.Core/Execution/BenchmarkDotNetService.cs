using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;

namespace AlgorithmPractice1.Core.Execution;

/// <summary>
/// Сервис контрольного бенчмаркинга через библиотеку BenchmarkDotNet (согласно разделу 3.2 ТЗ).
/// Использует InProcessNoEmitToolchain для выполнения бенчмарков внутри текущего процесса без спавна внешних дочерних процессов.
/// </summary>
public sealed class BenchmarkDotNetService
{
    public static Summary RunSampleBenchmark<TBenchmark>() where TBenchmark : class
    {
        var config = ManualConfig.Create(DefaultConfig.Instance)
            .AddJob(Job.Dry
                .WithToolchain(InProcessNoEmitToolchain.Instance)
                .WithIterationCount(5)
                .WithWarmupCount(1))
            .WithOptions(ConfigOptions.DisableOptimizationsValidator);

        return BenchmarkRunner.Run<TBenchmark>(config);
    }
}

/// <summary>
/// Пример эталонного бенчмарка для BenchmarkDotNet.
/// </summary>
[Config(typeof(SampleBenchmarkConfig))]
public class SampleSortingBenchmark
{
    private int[] _data = Array.Empty<int>();

    [Params(1000, 5000)]
    public int N { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);
        _data = new int[N];
        for (int i = 0; i < N; i++) _data[i] = rng.Next();
    }

    [Benchmark]
    public int[] DotNetSort()
    {
        var copy = (int[])_data.Clone();
        Array.Sort(copy);
        return copy;
    }
}

public class SampleBenchmarkConfig : ManualConfig
{
    public SampleBenchmarkConfig()
    {
        AddJob(Job.Dry
            .WithToolchain(InProcessNoEmitToolchain.Instance)
            .WithIterationCount(5)
            .WithWarmupCount(1));
        WithOptions(ConfigOptions.DisableOptimizationsValidator);
    }
}
