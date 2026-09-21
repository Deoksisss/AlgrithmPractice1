using AlgorithmPractice1.Core.Data;
using AlgorithmPractice1.Core.Execution;
using AlgorithmPractice1.GUI.ViewModels;
using AlgorithmPractice1.GUI.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace AlgorithmPractice1.GUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // 1. Инициализация фабрики БД и создание схемы SQLite в корне решения
            var connectionFactory = SolutionPathResolver.CreateDefaultFactory();
            DatabaseInitializer.Initialize(connectionFactory);

            // 2. Инициализация репозиториев и сервисов
            var sessionRepo = new SessionRepository(connectionFactory);
            var measurementRepo = new MeasurementRepository(connectionFactory);
            var approxRepo = new ApproximationRepository(connectionFactory);

            var runner = new ExperimentRunner(sessionRepo, measurementRepo, approxRepo);
            var mainWindowVm = new MainWindowViewModel(runner, sessionRepo);

            _ = mainWindowVm.InitializeAsync();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowVm
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
