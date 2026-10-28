using AlgorithmPractice1.Core.Algorithms.Part1_Vectors;
using AlgorithmPractice1.Core.Algorithms.Part3_Individual;
using AlgorithmPractice1.Core.Algorithms.Part4_Exponentiation;
using AlgorithmPractice1.Core.CallTree;
using AlgorithmPractice1.Core.Models;
using Xunit;

namespace AlgorithmPractice1.Core.Tests;

public class CallTreeTests
{
    [Fact]
    public void RecursiveAlgorithms_HaveIsRecursiveTrue()
    {
        Assert.True(new PermutationsAlgorithm().IsRecursive);
        Assert.True(new FibonacciNaiveRecursiveAlgorithm().IsRecursive);
        Assert.True(new FibonacciMemoizedAlgorithm().IsRecursive);
        Assert.True(new RecursiveExponentiationAlgorithm().IsRecursive);
        Assert.True(new BinaryExponentiationAlgorithm().IsRecursive);
        Assert.True(new QuickSortAlgorithm().IsRecursive);
        Assert.True(new InductiveSubsetsAlgorithm().IsRecursive);
        Assert.True(new RTreeSearchAlgorithm().IsRecursive);

        Assert.False(new BubbleSortAlgorithm().IsRecursive);
        Assert.False(new FibonacciIterativeAlgorithm().IsRecursive);
    }

    [Fact]
    public void RTreeSearch_BuildsCallTree_WithDepthGreaterThanTwo()
    {
        var alg = new RTreeSearchAlgorithm();
        var root = alg.GenerateCallTree(15);

        Assert.NotNull(root);
        Assert.Equal(alg.DisplayName, root.MethodName);
        Assert.NotEmpty(root.Children);

        // Проверяем, что глубина рекурсии больше 2 (для N=15 высота R-дерева достигает 3-4 уровней)
        int maxDepth = CalculateDepth(root);
        Assert.True(maxDepth > 2, $"Ожидалась глубина вызова рекурсии > 2, но получена {maxDepth}");
        Assert.True(root.SubtreeCount >= 4);
    }

    private static int CalculateDepth(CallTreeNode node)
    {
        if (node.Children.Count == 0) return node.Depth;
        return node.Children.Max(CalculateDepth);
    }

    [Fact]
    public void InductiveSubsets_BuildsCallTree()
    {
        var alg = new InductiveSubsetsAlgorithm();
        var root = alg.GenerateCallTree(3);

        Assert.NotNull(root);
        Assert.Equal(alg.DisplayName, root.MethodName);
        Assert.NotEmpty(root.Children);
        // Рекурсивные вызовы n=3 -> n=2 -> n=1 -> n=0
        Assert.True(root.SubtreeCount >= 4);
    }

    [Fact]
    public void PermutationsAlgorithm_BuildsCallTree()
    {
        var alg = new PermutationsAlgorithm();
        var root = alg.GenerateCallTree(3);

        Assert.NotNull(root);
        Assert.Equal(alg.DisplayName, root.MethodName);
        Assert.NotEmpty(root.Children);

        // Для n = 3 должно быть сгенерировано несколько уровней вызовов
        Assert.True(root.SubtreeCount > 3);
        Assert.NotNull(root.ReturnValueText);
        Assert.Equal("6", root.ReturnValueText); // 3! = 6
    }

    [Fact]
    public void FibonacciNaiveRecursive_BuildsBinaryTree()
    {
        var alg = new FibonacciNaiveRecursiveAlgorithm();
        var root = alg.GenerateCallTree(4);

        Assert.NotNull(root);
        Assert.Equal(alg.DisplayName, root.MethodName);
        Assert.NotEmpty(root.Children);

        // F(4) = 3
        Assert.Equal("3", root.ReturnValueText);
        // Количество вызовов для F(4) = 2*F(5) - 1 = 2*5 - 1 = 9 вызовов в поддереве Fib
        Assert.True(root.SubtreeCount >= 9);
    }

    [Fact]
    public void RecursiveExponentiation_BuildsLinearChain()
    {
        var alg = new RecursiveExponentiationAlgorithm();
        var config = new ExperimentConfig { X = 2.0 };
        var root = alg.GenerateCallTree(4, config);

        Assert.NotNull(root);
        Assert.Equal(alg.DisplayName, root.MethodName);
        Assert.NotEmpty(root.Children);
        Assert.Equal("16", root.ReturnValueText); // 2^4 = 16
    }

    [Fact]
    public void BinaryExponentiation_BuildsLogarithmicTree()
    {
        var alg = new BinaryExponentiationAlgorithm();
        var config = new ExperimentConfig { X = 2.0 };
        var root = alg.GenerateCallTree(8, config);

        Assert.NotNull(root);
        Assert.Equal(alg.DisplayName, root.MethodName);
        Assert.NotEmpty(root.Children);
        Assert.Equal("256", root.ReturnValueText); // 2^8 = 256
    }

    [Fact]
    public void QuickSort_BuildsDivideAndConquerTree()
    {
        var alg = new QuickSortAlgorithm();
        var root = alg.GenerateCallTree(6);

        Assert.NotNull(root);
        Assert.Equal(alg.DisplayName, root.MethodName);
        Assert.NotEmpty(root.Children);
    }

    [Fact]
    public void CallTreeViewModel_InitializesWithAvailableRecursiveAlgorithms()
    {
        var vm = new AlgorithmPractice1.GUI.ViewModels.CallTreeViewModel();
        Assert.NotEmpty(vm.AvailableAlgorithms);
        Assert.All(vm.AvailableAlgorithms, a => Assert.True(a.Algorithm.IsRecursive));
        Assert.NotNull(vm.SelectedAlgorithm);
        Assert.True(vm.SelectedAlgorithm.IsSelected);
        Assert.True(vm.IsGraphMode);
    }

    [Fact]
    public void CallTreeViewModel_SingleSelectionWorks()
    {
        var vm = new AlgorithmPractice1.GUI.ViewModels.CallTreeViewModel();
        Assert.True(vm.DisplayedAlgorithms.Count >= 2);

        var first = vm.DisplayedAlgorithms[0];
        var second = vm.DisplayedAlgorithms[1];

        vm.SelectAlgorithm(second);

        Assert.Equal(second, vm.SelectedAlgorithm);
        Assert.True(second.IsSelected);
        Assert.False(first.IsSelected);
    }

    [Fact]
    public void CallTreeViewModel_RegisterBatch_FiltersToBatchAlgorithms()
    {
        var vm = new AlgorithmPractice1.GUI.ViewModels.CallTreeViewModel();
        var algs = new List<AlgorithmPractice1.Core.Abstractions.IAlgorithm>
        {
            new PermutationsAlgorithm(),
            new FibonacciNaiveRecursiveAlgorithm()
        };

        vm.RegisterCompletedBatch("Пакет #1", algs);

        Assert.Single(vm.Batches);
        Assert.Equal("Пакет #1", vm.Batches[0].Title);
        Assert.Equal(vm.Batches[0], vm.SelectedBatch);
        Assert.Equal(2, vm.DisplayedAlgorithms.Count);
        Assert.NotNull(vm.SelectedAlgorithm);
        Assert.Equal("Permutations", vm.SelectedAlgorithm.Id);
    }
}
