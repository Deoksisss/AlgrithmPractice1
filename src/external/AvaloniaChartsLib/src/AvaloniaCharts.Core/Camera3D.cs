namespace AvaloniaCharts.Core;

/// <summary>
/// Камера для 3D-сцены с поддержкой орбитального вращения (Yaw, Pitch) и зума (Distance).
/// </summary>
public sealed class Camera3D
{
    /// <summary>
    /// Точка назначения камеры (центр вращения).
    /// </summary>
    public Point3D Target { get; set; } = new(0, 0, 0);

    /// <summary>
    /// Угол поворота по горизонтали (в радианах).
    /// </summary>
    public double Yaw { get; set; } = Math.PI / 4; // 45 градусов

    /// <summary>
    /// Угол подъёма по вертикали (в радианах).
    /// </summary>
    public double Pitch { get; set; } = Math.PI / 6; // 30 градусов

    /// <summary>
    /// Расстояние от целевой точки до камеры.
    /// </summary>
    public double Distance { get; set; } = 5.0;

    /// <summary>
    /// Угол обзора в радианах (FOV).
    /// </summary>
    public double FieldOfView { get; set; } = Math.PI / 4; // 45 градусов

    /// <summary>
    /// Ближняя плоскость отсечения.
    /// </summary>
    public double NearPlane { get; set; } = 0.1;

    /// <summary>
    /// Дальняя плоскость отсечения.
    /// </summary>
    public double FarPlane { get; set; } = 1000.0;

    /// <summary>
    /// Вычисляет текущие 3D-координаты положения камеры.
    /// </summary>
    public Point3D GetEyePosition()
    {
        double clampedPitch = Math.Clamp(Pitch, -Math.PI / 2.0 + 0.01, Math.PI / 2.0 - 0.01);
        double cosPitch = Math.Cos(clampedPitch);
        double sinPitch = Math.Sin(clampedPitch);
        double sinYaw = Math.Sin(Yaw);
        double cosYaw = Math.Cos(Yaw);

        double x = Target.X + Distance * cosPitch * sinYaw;
        double y = Target.Y + Distance * sinPitch;
        double z = Target.Z + Distance * cosPitch * cosYaw;

        return new Point3D(x, y, z);
    }

    /// <summary>
    /// Создаёт итоговую матрицу полного преобразования (View * Projection * Viewport).
    /// </summary>
    public Matrix4x4Data GetTransformMatrix(double viewportWidth, double viewportHeight)
    {
        Point3D eye = GetEyePosition();
        Point3D up = new(0, 1, 0);

        Matrix4x4Data view = Matrix4x4Math.CreateLookAt(eye, Target, up);
        double aspect = Math.Max(0.001, viewportWidth / Math.Max(1.0, viewportHeight));
        Matrix4x4Data proj = Matrix4x4Math.CreatePerspectiveFieldOfView(FieldOfView, aspect, NearPlane, FarPlane);
        Matrix4x4Data viewport = Matrix4x4Math.CreateViewport(0, 0, viewportWidth, viewportHeight);

        Matrix4x4Data viewProj = Matrix4x4Math.Multiply(view, proj);
        return Matrix4x4Math.Multiply(viewProj, viewport);
    }
}
