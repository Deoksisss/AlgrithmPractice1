using AlgorithmPractice1.Core.Models;
using Xunit;

namespace AlgorithmPractice1.Core.Tests;

public class CacheAndConfigTests
{
    [Fact]
    public void ComputeConfigHash_IsDeterministic()
    {
        var config1 = new ExperimentConfig { NMax = 1000, NStep = 100, RunsPerN = 5, M = 50 };
        var config2 = new ExperimentConfig { NMax = 1000, NStep = 100, RunsPerN = 5, M = 50 };

        Assert.Equal(config1.ComputeConfigHash(), config2.ComputeConfigHash());
    }

    [Fact]
    public void ComputeConfigHash_IgnoresForceRecalculate()
    {
        var config1 = new ExperimentConfig { NMax = 1000, NStep = 100, RunsPerN = 5, ForceRecalculate = false };
        var config2 = new ExperimentConfig { NMax = 1000, NStep = 100, RunsPerN = 5, ForceRecalculate = true };

        Assert.Equal(config1.ComputeConfigHash(), config2.ComputeConfigHash());
    }

    [Fact]
    public void ComputeConfigHash_ChangesWhenParametersChange()
    {
        var config1 = new ExperimentConfig { NMax = 1000, NStep = 100, RunsPerN = 5 };
        var config2 = new ExperimentConfig { NMax = 2000, NStep = 100, RunsPerN = 5 };
        var config3 = new ExperimentConfig { NMax = 1000, NStep = 200, RunsPerN = 5 };
        var config4 = new ExperimentConfig { NMax = 1000, NStep = 100, RunsPerN = 10 };
        var config5 = new ExperimentConfig { NMax = 1000, NStep = 100, RunsPerN = 5, M = 50 };
        var config6 = new ExperimentConfig { NMax = 1000, NStep = 100, RunsPerN = 5, M = 50, MStep = 10 };

        Assert.NotEqual(config1.ComputeConfigHash(), config2.ComputeConfigHash());
        Assert.NotEqual(config1.ComputeConfigHash(), config3.ComputeConfigHash());
        Assert.NotEqual(config1.ComputeConfigHash(), config4.ComputeConfigHash());
        Assert.NotEqual(config1.ComputeConfigHash(), config5.ComputeConfigHash());
        Assert.NotEqual(config5.ComputeConfigHash(), config6.ComputeConfigHash());
    }
}
