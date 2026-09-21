# AvaloniaCharts

Компактная высокопроизводительная .NET 8 библиотека графиков 2D и 3D для Avalonia UI без внешних сторонних движков.

## Особенности

- **2D Графики (`Chart2D` / `Chart2DControl`)**:
  - Отображение **одного или нескольких графиков на одной плоскости**.
  - Настройка цвета в формате **HEX** (например, `"#FF5733"`, `"#007ACC"`, `"#FFC107"`) или имени цвета.
  - Настройка типа линии: сплошная (`LineStyle.Solid`), прерывистая (`LineStyle.Dashed`), пунктирная (`LineStyle.Dotted`).
  - **Сноска/легенда** в левом верхнем углу области графика с образцом линии и названием.
  - Авто-масштабирование по всем сериям данных с делениями {1, 2, 5} × 10ⁿ.
  - Интерактивность: панорамирование (зажатая ЛКМ) и масштабирование (колесо мыши).

- **3D Графики (`Chart3D` / `Chart3DControl`)**:
  - Отображение **одной или нескольких поверхностей на одном пространстве**.
  - Настройка типа поверхности: сплошная (`SurfaceStyle.Solid`), каркасная сетка (`SurfaceStyle.Wireframe`), сплошная с сеткой (`SurfaceStyle.SolidWithWireframe`).
  - Регулируемая полупрозрачность (`Opacity` от `0.0` до `1.0`), благодаря чему наложенные поверхности не закрывают друг друга.
  - Сноска/легенда в левом верхнем углу 3D-пространства.
  - Алгоритм художника (Painter's algorithm) для корректной сортировки глубин полигонов всех поверхностей.
  - Интерактивное орбитальное вращение сцены мышью (drag ЛКМ) и зум (колесо мыши).

---

## Структура решения

```
AvaloniaCharts.sln
├── src/
│   ├── AvaloniaCharts.Core/          — Модели данных (Point2D, Point3D, SurfaceData), математика 3D проекции (Matrix4x4Math, Camera3D), AxisScale, GridBinner
│   ├── AvaloniaCharts.Controls/      — Контролы Avalonia (Chart2DControl, Chart3DControl), стилизация (Chart2DSeries, Chart3DSeries) и API (Chart2D, Chart3D)
│   └── AvaloniaCharts.Runner/        — Запуск графиков в standalone окнах через ChartWindow / .Run()
├── samples/
│   └── AvaloniaCharts.Demo/          — Демонстрационное приложение (мульти-серии 2D и 3D)
└── tests/
    └── AvaloniaCharts.Tests/         — Unit-тесты для модуля Core и Controls
```

---

## Примеры использования

### 1. 2D График (Несколько серий с легендой)

```csharp
using AvaloniaCharts.Controls;
using AvaloniaCharts.Core;

// Серия 1: Сплошная тёмно-синяя линия
var series1 = new Chart2DSeries
{
    Name = "исходный сигнал",
    Points = new List<Point2D> { new(0, 0), new(1, 1), new(2, 0), new(3, -1), new(4, 0) },
    ColorHex = "#000080",
    LineStyle = LineStyle.Solid,
    LineThickness = 2.0
};

// Серия 2: Жёлтая прерывистая линия
var series2 = new Chart2DSeries
{
    Name = "аппроксимация gnome sort",
    Points = new List<Point2D> { new(0, 0.2), new(1, 0.8), new(2, 0.1), new(3, -0.9), new(4, 0.1) },
    ColorHex = "#FFC107",
    LineStyle = LineStyle.Dashed,
    LineThickness = 2.5
};

var chart = new Chart2D();
chart.SetParams(new Chart2DParams 
{ 
    Title = "2D Мульти-график",
    XLabel = "Время, с", 
    YLabel = "Амплитуда" 
});

chart.SetSeries(new[] { series1, series2 });

// Открыть графики в отдельном окне
chart.Run();
```

### 2. 3D Поверхности (Несколько поверхностей на одном пространстве)

```csharp
using AvaloniaCharts.Controls;
using AvaloniaCharts.Core;

// Поверхность 1: Сплошная полупрозрачная поверхность с сеткой
var surfaceSeries1 = new Chart3DSeries
{
    Name = "Поверхность 1 (Сигнал)",
    Data = surfaceData1,
    ColorHex = "#007ACC",
    Style = SurfaceStyle.SolidWithWireframe,
    Opacity = 0.75
};

// Поверхность 2: Жёлтая каркасная сетка
var surfaceSeries2 = new Chart3DSeries
{
    Name = "аппроксимация gnome sort",
    Data = surfaceData2,
    ColorHex = "#FFC107",
    Style = SurfaceStyle.Wireframe,
    Opacity = 0.9
};

var chart3d = new Chart3D();
chart3d.SetParams(new Chart3DParams 
{ 
    Title = "3D Пространство",
    XLabel = "X", 
    YLabel = "Y", 
    ZLabel = "Z" 
});

chart3d.SetSeries(new[] { surfaceSeries1, surfaceSeries2 });
chart3d.Run();
```

---

## Инструкция по встраиванию в другие Avalonia-проекты

Библиотеку можно легко встроить в существующий проект Avalonia как через C# код, так и через XAML.

### Вариант 1. Подключение через C# код

1. Добавьте ссылку на проект `AvaloniaCharts.Controls` (или собранные DLL `AvaloniaCharts.Core.dll` и `AvaloniaCharts.Controls.dll`) в ваш `.csproj`.

2. Разместите контрол `Chart2DControl` или `Chart3DControl` в контейнере (Grid, DockPanel и т.д.):

```csharp
using Avalonia.Controls;
using AvaloniaCharts.Controls;

public partial class MyView : UserControl
{
    public MyView()
    {
        InitializeComponent();

        var chartControl = new Chart2DControl();
        chartControl.SetParams(new Chart2DParams { Title = "Мой график", XLabel = "X", YLabel = "Y" });
        chartControl.SetSeries(new[] 
        {
            new Chart2DSeries
            {
                Name = "Эксперимент",
                Points = myPoints,
                ColorHex = "#FF5733",
                LineStyle = LineStyle.Solid
            }
        });

        // Добавляем контрол в элемент разметки
        MyContainerGrid.Children.Add(chartControl);
    }
}
```

### Вариант 2. Размещение в XAML

1. Объявите пространство имён XML в вашем файл разметки (`.axaml`):

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:charts="clr-namespace:AvaloniaCharts.Controls;assembly=AvaloniaCharts.Controls"
             x:Class="MyApp.Views.MyView">

    <Grid>
        <!-- 2D Контрол -->
        <charts:Chart2DControl x:Name="MyChart2D" />

        <!-- Или 3D Контрол -->
        <!-- <charts:Chart3DControl x:Name="MyChart3D" /> -->
    </Grid>
</UserControl>
```

2. В коде файла `MyView.axaml.cs` передайте данные:

```csharp
public partial class MyView : UserControl
{
    public MyView()
    {
        InitializeComponent();

        MyChart2D.SetParams(new Chart2DParams { Title = "График из XAML" });
        MyChart2D.SetSeries(new[] { ... });
    }
}
```

---

## Сборка и тесты

```bash
# Сборка решения
dotnet build AvaloniaCharts.sln

# Запуск unit-тестов
dotnet test tests/AvaloniaCharts.Tests/AvaloniaCharts.Tests.csproj

# Запуск демо-приложения
dotnet run --project samples/AvaloniaCharts.Demo/AvaloniaCharts.Demo.csproj
```
