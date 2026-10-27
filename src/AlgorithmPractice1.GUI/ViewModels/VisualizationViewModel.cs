using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Algorithms.Part3_Individual;
using AlgorithmPractice1.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgorithmPractice1.GUI.ViewModels;

public partial class VisualizationViewModel : ViewModelBase
{
    // Список алгоритмов, поддерживающих дерево
    public ObservableCollection<ICallTreeProvider> AvailableAlgorithms { get; } = new();
    
    [ObservableProperty] 
    private ICallTreeProvider? _selectedAlgorithm;

    [ObservableProperty] 
    private int _n = 3;
    
    [ObservableProperty] 
    private string _currentArrayText = "";
    
    public List<AlgorithmCallNode> TreeNodes { get; private set; } = new();
    public int CurrentNodeId { get; private set; } = -1;
    
    public event System.Action<List<AlgorithmCallNode>, int>? TreeUpdated;
    public event System.Action<int>? CurrentNodeChanged; 
    public event System.Action? RequestResetView; // Событие для сброса зума

    public VisualizationViewModel()
    {
        // Пока регистрируем только перестановки. Если добавишь другие — добавь сюда.
        AvailableAlgorithms.Add(new PermutationsAlgorithm());
        
        SelectedAlgorithm = AvailableAlgorithms.FirstOrDefault();
    }

    [RelayCommand]
    private void BuildTree()
    {
        if (SelectedAlgorithm == null) return;

        TreeNodes = SelectedAlgorithm.GenerateCallTree(N);
        
        if (TreeNodes.Any())
        {
            CurrentNodeId = TreeNodes[0].Id;
            UpdateCurrentNodeText();
            RequestResetView?.Invoke(); // Сбрасываем зум перед отрисовкой
        }
        
        TreeUpdated?.Invoke(TreeNodes, CurrentNodeId);
    }

    [RelayCommand]
    private void NextNode()
    {
        if (TreeNodes.Count == 0) return;
        var currentIndex = TreeNodes.FindIndex(n => n.Id == CurrentNodeId);
        if (currentIndex < TreeNodes.Count - 1)
        {
            CurrentNodeId = TreeNodes[currentIndex + 1].Id;
            UpdateCurrentNodeText();
            CurrentNodeChanged?.Invoke(CurrentNodeId);
        }
    }

    [RelayCommand]
    private void PrevNode()
    {
        if (TreeNodes.Count == 0) return;
        var currentIndex = TreeNodes.FindIndex(n => n.Id == CurrentNodeId);
        if (currentIndex > 0)
        {
            CurrentNodeId = TreeNodes[currentIndex - 1].Id;
            UpdateCurrentNodeText();
            CurrentNodeChanged?.Invoke(CurrentNodeId);
        }
    }
    
    [RelayCommand]
    private void Reset()
    {
        if (TreeNodes.Any())
        {
            CurrentNodeId = TreeNodes[0].Id;
            UpdateCurrentNodeText();
            CurrentNodeChanged?.Invoke(CurrentNodeId);
            RequestResetView?.Invoke();
        }
    }

    private void UpdateCurrentNodeText()
    {
        var node = TreeNodes.FirstOrDefault(n => n.Id == CurrentNodeId);
        if (node != null)
        {
            CurrentArrayText = $"[{string.Join(", ", node.ArrayState)}]";
        }
    }
}