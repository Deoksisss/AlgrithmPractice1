using Avalonia.Controls;

namespace AvaloniaCharts.Controls;

/// <summary>
/// Утилита для запуска графиков в отдельном окне Avalonia.
/// </summary>
public static class ChartRunner
{
    public static Action<Control, string>? Launcher { get; set; }

    public static void Launch(Control control, string title)
    {
        if (Launcher != null)
        {
            Launcher(control, title);
            return;
        }

        var runnerType = Type.GetType("AvaloniaCharts.Runner.ChartWindow, AvaloniaCharts.Runner");
        var launchMethod = runnerType?.GetMethod("Launch", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        if (launchMethod != null)
        {
            launchMethod.Invoke(null, new object[] { control, title });
        }
        else
        {
            throw new InvalidOperationException("Не удалось найти AvaloniaCharts.Runner.ChartWindow.");
        }
    }
}
