using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms.Integration;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.GLControl;
using System.Windows.Forms;
using System.Collections.Generic;
using CMMDemoApp.Models;

namespace CMMDemoApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private GLControl? glControl;
    private bool showDemoCube = false;

    private List<MeasurementResult> demoPoints = new List<MeasurementResult>
    {
        new MeasurementResult { Name = "Punkt 1", X = -10, Y = 0, Z = 0, Nominal = 25.0, Actual = 25.0 },
        new MeasurementResult { Name = "Punkt 2", X = 0, Y = 0, Z = 0, Nominal = 25.0, Actual = 24.8 },
        new MeasurementResult { Name = "Punkt 3", X = 10, Y = 0, Z = 0, Nominal = 25.0, Actual = 25.2 }
    };

    public MainWindow()
    {
        // If not using XAML, comment out or remove InitializeComponent
        // InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        glControl = new GLControl(); // Без GraphicsMode для совместимости
        glControl.Dock = DockStyle.Fill;
        glControl.Paint += GlControl_Paint;
        glControl.Resize += GlControl_Resize;
        ((WindowsFormsHost)FindName("OpenGLHost")).Child = glControl;
    }

    private void GlControl_Resize(object sender, EventArgs e)
    {
        if (glControl == null) return;
        GL.Viewport(0, 0, glControl.Width, glControl.Height);
    }

    private void ShowDemoModel_Click(object sender, RoutedEventArgs e)
    {
        showDemoCube = true;
        glControl?.Invalidate();
    }

    private void GlControl_Paint(object sender, PaintEventArgs e)
    {
        GL.ClearColor(0.95f, 0.97f, 0.98f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        if (showDemoCube)
        {
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(1.0, 0.5, 0.0); // Orange
            // Front face
            GL.Vertex3(-1.0, -1.0,  1.0);
            GL.Vertex3( 1.0, -1.0,  1.0);
            GL.Vertex3( 1.0,  1.0,  1.0);
            GL.Vertex3(-1.0,  1.0,  1.0);
            // Back face
            GL.Vertex3(-1.0, -1.0, -1.0);
            GL.Vertex3(-1.0,  1.0, -1.0);
            GL.Vertex3( 1.0,  1.0, -1.0);
            GL.Vertex3( 1.0, -1.0, -1.0);
            // Left face
            GL.Vertex3(-1.0, -1.0, -1.0);
            GL.Vertex3(-1.0, -1.0,  1.0);
            GL.Vertex3(-1.0,  1.0,  1.0);
            GL.Vertex3(-1.0,  1.0, -1.0);
            // Right face
            GL.Vertex3(1.0, -1.0, -1.0);
            GL.Vertex3(1.0,  1.0, -1.0);
            GL.Vertex3(1.0,  1.0,  1.0);
            GL.Vertex3(1.0, -1.0,  1.0);
            // Top face
            GL.Vertex3(-1.0, 1.0, -1.0);
            GL.Vertex3(-1.0, 1.0,  1.0);
            GL.Vertex3( 1.0, 1.0,  1.0);
            GL.Vertex3( 1.0, 1.0, -1.0);
            // Bottom face
            GL.Vertex3(-1.0, -1.0, -1.0);
            GL.Vertex3( 1.0, -1.0, -1.0);
            GL.Vertex3( 1.0, -1.0,  1.0);
            GL.Vertex3(-1.0, -1.0,  1.0);
            GL.End();
        }
        // Рисуем точки
        GL.PointSize(10f);
        GL.Begin(PrimitiveType.Points);
        foreach (var pt in demoPoints)
        {
            if (Math.Abs(pt.Deviation) < 0.15)
                GL.Color3(0.2, 0.8, 0.2); // зелёный
            else
                GL.Color3(0.9, 0.2, 0.2); // красный
            GL.Vertex3(pt.X / 30.0, pt.Y / 30.0, pt.Z / 30.0); // нормализация для куба
        }
        GL.End();
        glControl?.SwapBuffers();
    }
}