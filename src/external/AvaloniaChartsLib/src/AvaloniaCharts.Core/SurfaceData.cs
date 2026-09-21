namespace AvaloniaCharts.Core;

/// <summary>
/// Данные для 3D-поверхности: регулярная сетка X x Y с высотами Z.
/// </summary>
public sealed class SurfaceData
{
    private double[] _xValues = Array.Empty<double>();
    private double[] _yValues = Array.Empty<double>();
    private double[,] _z = new double[0, 0];

    /// <summary>
    /// Значения по оси X (по возрастанию). Длина = Width.
    /// </summary>
    public required double[] XValues
    {
        get => _xValues;
        init
        {
            _xValues = value ?? throw new ArgumentNullException(nameof(value));
            ValidateDimensions();
        }
    }

    /// <summary>
    /// Значения по оси Y (по возрастанию). Длина = Height.
    /// </summary>
    public required double[] YValues
    {
        get => _yValues;
        init
        {
            _yValues = value ?? throw new ArgumentNullException(nameof(value));
            ValidateDimensions();
        }
    }

    /// <summary>
    /// Двумерный массив высот Z размером [Width, Height].
    /// </summary>
    public required double[,] Z
    {
        get => _z;
        init
        {
            _z = value ?? throw new ArgumentNullException(nameof(value));
            ValidateDimensions();
        }
    }

    private void ValidateDimensions()
    {
        if (_xValues.Length > 0 && _z.GetLength(0) != 0 && _z.GetLength(0) != _xValues.Length)
        {
            throw new ArgumentException(
                $"Размерность Z по измерению 0 ({_z.GetLength(0)}) не совпадает с длиной XValues ({_xValues.Length}).");
        }

        if (_yValues.Length > 0 && _z.GetLength(1) != 0 && _z.GetLength(1) != _yValues.Length)
        {
            throw new ArgumentException(
                $"Размерность Z по измерению 1 ({_z.GetLength(1)}) не совпадает с длиной YValues ({_yValues.Length}).");
        }
    }
}
