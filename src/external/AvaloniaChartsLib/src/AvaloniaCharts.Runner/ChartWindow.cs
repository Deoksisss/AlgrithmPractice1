using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using AvaloniaCharts.Controls;

namespace AvaloniaCharts.Runner;

internal class MinimalApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }
}

/// <summary>
/// Минимальное окно Avalonia для отдельного запуска графиков через .Run().
/// </summary>
public class ChartWindow : Window
{
    static ChartWindow()
    {
        ChartRunner.Launcher = Launch;
    }

    public ChartWindow(Control chartControl, string title)
    {
        Title = title;
        Width = 900;
        Height = 650;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Brushes.White;
        Content = chartControl;
    }

    /// <summary>
    /// Запускает окно с указанным графиком и блокирует выполнение до его закрытия.
    /// </summary>
    public static void Launch(Control chartControl, string title)
    {
        if (Application.Current != null)
        {
            var window = new ChartWindow(chartControl, title);
            var desktop = Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            if (desktop?.MainWindow != null)
            {
                window.Show(desktop.MainWindow);
            }
            else
            {
                window.Show();
            }
        }
        else
        {
            AppBuilder.Configure<MinimalApp>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .StartWithClassicDesktopLifetime(Array.Empty<string>(), lifetime =>
                {
                    var window = new ChartWindow(chartControl, title);
                    lifetime.MainWindow = window;
                });
        }
    }
}
