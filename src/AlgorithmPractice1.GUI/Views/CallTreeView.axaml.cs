using AlgorithmPractice1.GUI.ViewModels;
using Avalonia.Controls;

namespace AlgorithmPractice1.GUI.Views;

public partial class CallTreeView : UserControl
{
    public CallTreeView()
    {
        InitializeComponent();

        ZoomInBtn.Click += (s, e) => GraphControl.ZoomIn();
        ZoomOutBtn.Click += (s, e) => GraphControl.ZoomOut();
        ResetZoomBtn.Click += (s, e) => GraphControl.ResetView();

        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is CallTreeViewModel vm)
        {
            vm.RequestCenterGraph += () => GraphControl.CenterView();
        }
    }
}
