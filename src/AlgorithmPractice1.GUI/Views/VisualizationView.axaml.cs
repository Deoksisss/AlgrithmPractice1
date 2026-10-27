using AlgorithmPractice1.Core.Models;
using Avalonia.Controls;
using AlgorithmPractice1.GUI.ViewModels;

namespace AlgorithmPractice1.GUI.Views;

public partial class VisualizationView : UserControl
{
    public VisualizationView()
    {
        InitializeComponent();
        DataContextChanged += (s, e) =>
        {
            if (DataContext is VisualizationViewModel vm)
            {
                // Отписываемся от старых
                vm.TreeUpdated -= OnTreeUpdated;
                vm.CurrentNodeChanged -= OnCurrentNodeChanged;
                vm.RequestResetView -= OnRequestResetView;

                // Подписываемся на новые
                vm.TreeUpdated += OnTreeUpdated;
                vm.CurrentNodeChanged += OnCurrentNodeChanged;
                vm.RequestResetView += OnRequestResetView;
            }
        };
    }

    private void OnTreeUpdated(List<AlgorithmCallNode> nodes, int currentId)
    {
        CanvasControl.SetTree(nodes);
        CanvasControl.SetCurrentNode(currentId);
    }

    private void OnCurrentNodeChanged(int currentId)
    {
        CanvasControl.SetCurrentNode(currentId);
    }

    private void OnRequestResetView()
    {
        CanvasControl.ResetView();
    }
}