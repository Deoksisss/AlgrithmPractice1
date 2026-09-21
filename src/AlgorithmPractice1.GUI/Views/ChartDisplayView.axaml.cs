using System;
using AlgorithmPractice1.GUI.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaCharts.Controls;

namespace AlgorithmPractice1.GUI.Views;

public partial class ChartDisplayView : UserControl
{
    public ChartDisplayView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ChartDisplayModel model)
        {
            var host = GetChartHost();
            if (host != null && host.Child == null)
            {
                RenderChart(model);
            }
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is ChartDisplayModel model)
        {
            RenderChart(model);
        }
    }

    private Border? GetChartHost()
    {
        return this.FindControl<Border>("ChartHost") ?? ChartHost;
    }

    private void RenderChart(ChartDisplayModel model)
    {
        var host = GetChartHost();
        if (host == null) return;

        host.Child = null;

        try
        {
            if (model.Is3D)
            {
                var chart3D = new Chart3DControl();
                if (model.Params3D != null)
                {
                    chart3D.SetParams(model.Params3D);
                }
                if (model.Series3D != null)
                {
                    chart3D.SetSeries(model.Series3D);
                }
                host.Child = chart3D;
            }
            else
            {
                var chart2D = new Chart2DControl();
                if (model.Params2D != null)
                {
                    chart2D.SetParams(model.Params2D);
                }
                if (model.Series2D != null)
                {
                    chart2D.SetSeries(model.Series2D);
                }
                host.Child = chart2D;
            }
        }
        catch (Exception ex)
        {
            host.Child = new TextBlock
            {
                Text = $"Не удалось отобразить график: {ex.Message}",
                Foreground = Brushes.Crimson,
                Margin = new Thickness(16)
            };
        }
    }
}
