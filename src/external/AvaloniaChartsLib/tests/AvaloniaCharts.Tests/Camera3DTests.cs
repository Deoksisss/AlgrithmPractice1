using AvaloniaCharts.Core;
using Xunit;

namespace AvaloniaCharts.Tests;

public class Camera3DTests
{
    [Fact]
    public void GetTransformMatrix_CenterPoint_ProjectsToScreenCenter()
    {
        Camera3D camera = new Camera3D
        {
            Target = new Point3D(0, 0, 0),
            Distance = 5.0,
            Yaw = 0,
            Pitch = 0
        };

        double viewportWidth = 800;
        double viewportHeight = 600;

        Matrix4x4Data matrix = camera.GetTransformMatrix(viewportWidth, viewportHeight);
        Point3D projectedCenter = Matrix4x4Math.TransformPoint(matrix, new Point3D(0, 0, 0));

        Assert.Equal(viewportWidth / 2.0, projectedCenter.X, precision: 1);
        Assert.Equal(viewportHeight / 2.0, projectedCenter.Y, precision: 1);
    }
}
