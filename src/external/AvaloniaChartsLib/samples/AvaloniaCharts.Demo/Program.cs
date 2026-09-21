using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using AvaloniaCharts.Controls;
using AvaloniaCharts.Core;

namespace AvaloniaCharts.Demo;

internal class App : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

public class MainWindow : Window
{
    public MainWindow()
    {
        Title = "AvaloniaCharts - Demo App (Multi-Series)";
        Width = 1050;
        Height = 750;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        // 1. Данные для 2D мульти-графиков
        var sinePoints = new List<Point2D>();
        var approxPoints = new List<Point2D>();

        for (int i = 0; i < 100; i++)
        {
            double x = i * 0.1;
            sinePoints.Add(new Point2D(x, Math.Sin(x)));
            approxPoints.Add(new Point2D(x, Math.Cos(x) * 0.8));
        }

        var series2DSine = new Chart2DSeries
        {
            Name = "исходный сигнал",
            Points = sinePoints,
            ColorHex = "#000080", // Тёмно-синий
            LineStyle = LineStyle.Solid,
            LineThickness = 2.0
        };

        var series2DApprox = new Chart2DSeries
        {
            Name = "аппроксимация gnome sort",
            Points = approxPoints,
            ColorHex = "#FFC107", // Ярко-жёлтый
            LineStyle = LineStyle.Dashed,
            LineThickness = 2.5
        };

        var chart2DControl = new Chart2DControl();
        chart2DControl.SetParams(new Chart2DParams
        {
            Title = "2D Мульти-график (Сплошная vs Жёлтая прерывистая)",
            XLabel = "Время, с",
            YLabel = "Амплитуда"
        });
        chart2DControl.SetSeries(new[] { series2DSine, series2DApprox });

        // 2. Данные для 3D мульти-поверхностей
        int size = 40;
        double[] xVals = new double[size];
        double[] yVals = new double[size];
        double[,] zHat = new double[size, size];
        double[,] zWave = new double[size, size];

        for (int i = 0; i < size; i++) xVals[i] = -8.0 + i * (16.0 / (size - 1));
        for (int j = 0; j < size; j++) yVals[j] = -8.0 + j * (16.0 / (size - 1));

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                double r = Math.Sqrt(xVals[i] * xVals[i] + yVals[j] * yVals[j]);
                zHat[i, j] = Math.Abs(r) < 1e-9 ? 1.0 : Math.Sin(r) / r;
                zWave[i, j] = (Math.Cos(xVals[i]) + Math.Sin(yVals[j])) * 0.3 - 0.2;
            }
        }

        var surfaceHat = new SurfaceData { XValues = xVals, YValues = yVals, Z = zHat };
        var surfaceWave = new SurfaceData { XValues = xVals, YValues = yVals, Z = zWave };

        var series3DHat = new Chart3DSeries
        {
            Name = "Мексиканская шляпа (Сплошная)",
            Data = surfaceHat,
            ColorHex = "#007ACC", // Голубой
            Style = SurfaceStyle.SolidWithWireframe,
            Opacity = 0.7
        };

        var series3DWave = new Chart3DSeries
        {
            Name = "аппроксимация gnome sort (Жёлтая сетка)",
            Data = surfaceWave,
            ColorHex = "#FFC107", // Жёлтый
            Style = SurfaceStyle.Wireframe,
            Opacity = 0.9
        };

        var chart3DControl = new Chart3DControl();
        chart3DControl.SetParams(new Chart3DParams
        {
            Title = "3D Мульти-пространство (Полупрозрачная поверхность + Сетка)",
            XLabel = "X",
            YLabel = "Y",
            ZLabel = "Z"
        });
        chart3DControl.SetSeries(new[] { series3DHat, series3DWave });

        // Кнопки взаимодействия и запуска отдельного окна
        var btnRun2D = new Button { Content = "Запустить Chart2D.Run()", Margin = new Thickness(5) };
        btnRun2D.Click += (_, _) =>
        {
            var chart = new Chart2D();
            chart.SetParams(new Chart2DParams { Title = "Отдельное окно 2D с легендой", XLabel = "Время, с", YLabel = "Сигнал" });
            chart.SetSeries(new[] { series2DSine, series2DApprox });
            chart.Run();
        };

        var btnRun3D = new Button { Content = "Запустить Chart3D.Run()", Margin = new Thickness(5) };
        btnRun3D.Click += (_, _) =>
        {
            var chart3d = new Chart3D();
            chart3d.SetParams(new Chart3DParams { Title = "Отдельное окно 3D с 2 поверхностями", XLabel = "X", YLabel = "Y", ZLabel = "Z" });
            chart3d.SetSeries(new[] { series3DHat, series3DWave });
            chart3d.Run();
        };

        var btnToggleLineStyle = new Button { Content = "Переключить стиль линии 2D", Margin = new Thickness(5) };
        btnToggleLineStyle.Click += (_, _) =>
        {
            series2DApprox.LineStyle = series2DApprox.LineStyle == LineStyle.Dashed ? LineStyle.Dotted : LineStyle.Dashed;
            chart2DControl.SetSeries(new[] { series2DSine, series2DApprox });
        };

        var topControlPanel2D = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10),
            Children = { btnRun2D, btnToggleLineStyle }
        };

        var tab2DContent = new DockPanel
        {
            Children =
            {
                topControlPanel2D,
                chart2DControl
            }
        };
        DockPanel.SetDock(topControlPanel2D, Dock.Top);

        var topControlPanel3D = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10),
            Children = { btnRun3D }
        };

        var tab3DContent = new DockPanel
        {
            Children =
            {
                topControlPanel3D,
                chart3DControl
            }
        };
        DockPanel.SetDock(topControlPanel3D, Dock.Top);

        var tabControl = new TabControl
        {
            Items =
            {
                new TabItem { Header = "2D Графики (Мульти-серии + Легенда)", Content = tab2DContent },
                new TabItem { Header = "3D Графики (Мульти-поверхности)", Content = tab3DContent }
            }
        };

        Content = tabControl;
    }
}

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
