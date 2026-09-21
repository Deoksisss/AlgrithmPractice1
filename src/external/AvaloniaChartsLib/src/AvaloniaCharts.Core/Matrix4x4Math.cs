namespace AvaloniaCharts.Core;

/// <summary>
/// Матрица 4x4 высокой точности (double).
/// </summary>
public struct Matrix4x4Data
{
    public double M11, M12, M13, M14;
    public double M21, M22, M23, M24;
    public double M31, M32, M33, M34;
    public double M41, M42, M43, M44;

    public static Matrix4x4Data Identity => new()
    {
        M11 = 1, M22 = 1, M33 = 1, M44 = 1
    };
}

/// <summary>
/// Математические функции для 3D-матриц и векторных преобразований.
/// </summary>
public static class Matrix4x4Math
{
    /// <summary>
    /// Перемножение двух матриц 4x4.
    /// </summary>
    public static Matrix4x4Data Multiply(Matrix4x4Data a, Matrix4x4Data b)
    {
        return new Matrix4x4Data
        {
            M11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41,
            M12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42,
            M13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43,
            M14 = a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44,

            M21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41,
            M22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42,
            M23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43,
            M24 = a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44,

            M31 = a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41,
            M32 = a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42,
            M33 = a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43,
            M34 = a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44,

            M41 = a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41,
            M42 = a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42,
            M43 = a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43,
            M44 = a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44,
        };
    }

    /// <summary>
    /// Матрица вида LookAt.
    /// </summary>
    public static Matrix4x4Data CreateLookAt(Point3D eye, Point3D target, Point3D up)
    {
        double zx = eye.X - target.X;
        double zy = eye.Y - target.Y;
        double zz = eye.Z - target.Z;
        double zlen = Math.Sqrt(zx * zx + zy * zy + zz * zz);
        if (zlen > 1e-9) { zx /= zlen; zy /= zlen; zz /= zlen; } else { zz = 1; }

        double xx = up.Y * zz - up.Z * zy;
        double xy = up.Z * zx - up.X * zz;
        double xz = up.X * zy - up.Y * zx;
        double xlen = Math.Sqrt(xx * xx + xy * xy + xz * xz);
        if (xlen > 1e-9) { xx /= xlen; xy /= xlen; xz /= xlen; } else { xx = 1; }

        double yx = zy * xz - zz * xy;
        double yy = zz * xx - zx * xz;
        double yz = zx * xy - zy * xx;

        return new Matrix4x4Data
        {
            M11 = xx, M12 = yx, M13 = zx, M14 = 0,
            M21 = xy, M22 = yy, M23 = zy, M24 = 0,
            M31 = xz, M32 = yz, M33 = zz, M34 = 0,
            M41 = -(xx * eye.X + xy * eye.Y + xz * eye.Z),
            M42 = -(yx * eye.X + yy * eye.Y + yz * eye.Z),
            M43 = -(zx * eye.X + zy * eye.Y + zz * eye.Z),
            M44 = 1
        };
    }

    /// <summary>
    /// Перспективная матрица проекции.
    /// </summary>
    public static Matrix4x4Data CreatePerspectiveFieldOfView(double fovRad, double aspectRatio, double nearPlane, double farPlane)
    {
        double yScale = 1.0 / Math.Tan(fovRad * 0.5);
        double xScale = yScale / aspectRatio;

        return new Matrix4x4Data
        {
            M11 = xScale, M12 = 0, M13 = 0, M14 = 0,
            M21 = 0, M22 = yScale, M23 = 0, M24 = 0,
            M31 = 0, M32 = 0, M33 = farPlane / (nearPlane - farPlane), M34 = -1,
            M41 = 0, M42 = 0, M43 = (nearPlane * farPlane) / (nearPlane - farPlane), M44 = 0
        };
    }

    /// <summary>
    /// Матрица преобразования в экранные координаты (Viewport).
    /// </summary>
    public static Matrix4x4Data CreateViewport(double x, double y, double width, double height)
    {
        return new Matrix4x4Data
        {
            M11 = width * 0.5, M12 = 0, M13 = 0, M14 = 0,
            M21 = 0, M22 = -height * 0.5, M23 = 0, M24 = 0,
            M31 = 0, M32 = 0, M33 = 1, M34 = 0,
            M41 = x + width * 0.5, M42 = y + height * 0.5, M43 = 0, M44 = 1
        };
    }

    /// <summary>
    /// Преобразование 3D точки матрицей.
    /// </summary>
    public static Point3D TransformPoint(Matrix4x4Data m, Point3D p)
    {
        double x = p.X * m.M11 + p.Y * m.M21 + p.Z * m.M31 + m.M41;
        double y = p.X * m.M12 + p.Y * m.M22 + p.Z * m.M32 + m.M42;
        double z = p.X * m.M13 + p.Y * m.M23 + p.Z * m.M33 + m.M43;
        double w = p.X * m.M14 + p.Y * m.M24 + p.Z * m.M34 + m.M44;

        if (Math.Abs(w) > 1e-9 && Math.Abs(w - 1.0) > 1e-9)
        {
            x /= w;
            y /= w;
            z /= w;
        }

        return new Point3D(x, y, z);
    }
}
