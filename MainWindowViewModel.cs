using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Media.Media3D;
using System.Windows;
using CMMDemoApp.Models;
using System.Windows.Media;
using System;

namespace CMMDemoApp;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<MeasurementResult> MeasurementResults { get; } = new();

    [ObservableProperty]
    private string selectedPointInfo = "Wählen Sie einen Punkt für Details...";

    [ObservableProperty]
    private MeshGeometry3D? demoModelGeometry;

    [ObservableProperty]
    private Model3DGroup? measurementPointsModel;

    public MainWindowViewModel()
    {
        LoadModelCommand = new RelayCommand(LoadModel);
        StartMeasurementCommand = new RelayCommand(StartMeasurement);
        ExportReportCommand = new RelayCommand(ExportReport);
        ShowDemoModelCommand = new RelayCommand(ShowDemoModel);
        AddSampleData();
    }

    private void LoadModel()
    {
        MessageBox.Show("STL-Modell laden (Demo)", "KMM-System", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void StartMeasurement()
    {
        MessageBox.Show("Messung gestartet! (Demo-Modus)", "KMM-System", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ExportReport()
    {
        MessageBox.Show("Bericht exportiert! (Demo-Modus)", "KMM-System", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ShowDemoModel()
    {
        // Erstelle Demo-Würfel mit verbesserter Geometrie
        var mesh = new MeshGeometry3D();
        
        // Würfel-Vertices (8 Eckpunkte)
        mesh.Positions.Add(new Point3D(-30, -15, -7.5)); // 0
        mesh.Positions.Add(new Point3D(30, -15, -7.5));  // 1
        mesh.Positions.Add(new Point3D(30, 15, -7.5));   // 2
        mesh.Positions.Add(new Point3D(-30, 15, -7.5));  // 3
        mesh.Positions.Add(new Point3D(-30, -15, 7.5));  // 4
        mesh.Positions.Add(new Point3D(30, -15, 7.5));   // 5
        mesh.Positions.Add(new Point3D(30, 15, 7.5));    // 6
        mesh.Positions.Add(new Point3D(-30, 15, 7.5));   // 7

        // Normale für besseres Licht
        var frontNormal = new Vector3D(0, 0, -1);
        var backNormal = new Vector3D(0, 0, 1);
        var leftNormal = new Vector3D(-1, 0, 0);
        var rightNormal = new Vector3D(1, 0, 0);
        var topNormal = new Vector3D(0, 1, 0);
        var bottomNormal = new Vector3D(0, -1, 0);

        // Erweiterte Vertices für jede Seite (für korrekte Normalen)
        // Front face
        mesh.Positions.Add(new Point3D(-30, -15, -7.5)); mesh.Normals.Add(frontNormal); // 8
        mesh.Positions.Add(new Point3D(30, -15, -7.5));  mesh.Normals.Add(frontNormal); // 9
        mesh.Positions.Add(new Point3D(30, 15, -7.5));   mesh.Normals.Add(frontNormal); // 10
        mesh.Positions.Add(new Point3D(-30, 15, -7.5));  mesh.Normals.Add(frontNormal); // 11

        // Back face
        mesh.Positions.Add(new Point3D(30, -15, 7.5));   mesh.Normals.Add(backNormal); // 12
        mesh.Positions.Add(new Point3D(-30, -15, 7.5));  mesh.Normals.Add(backNormal); // 13
        mesh.Positions.Add(new Point3D(-30, 15, 7.5));   mesh.Normals.Add(backNormal); // 14
        mesh.Positions.Add(new Point3D(30, 15, 7.5));    mesh.Normals.Add(backNormal); // 15

        // Triangles mit neuen Indices
        // Front face
        mesh.TriangleIndices.Add(8); mesh.TriangleIndices.Add(9); mesh.TriangleIndices.Add(10);
        mesh.TriangleIndices.Add(8); mesh.TriangleIndices.Add(10); mesh.TriangleIndices.Add(11);
        
        // Back face
        mesh.TriangleIndices.Add(12); mesh.TriangleIndices.Add(13); mesh.TriangleIndices.Add(14);
        mesh.TriangleIndices.Add(12); mesh.TriangleIndices.Add(14); mesh.TriangleIndices.Add(15);
        
        // Verwende ursprüngliche Vertices für andere Seiten
        // Left face
        mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(7);
        mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(4);
        
        // Right face
        mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(6);
        mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(2);
        
        // Top face
        mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(6);
        mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(7);
        
        // Bottom face
        mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(5);
        mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(1);

        DemoModelGeometry = mesh;
        CreateMeasurementPoints();
        SelectedPointInfo = "Demo-Modell: 60x30x15 mm Rechteckiger Block mit verbesserter Darstellung";
    }

    private void CreateMeasurementPoints()
    {
        var pointsGroup = new Model3DGroup();
        
        foreach (var point in MeasurementResults)
        {
            var sphereMesh = CreateSphere(new Point3D(point.X, point.Y, point.Z), 3); // Уменьшаем размер до разумного
            var material = Math.Abs(point.Deviation) < 0.15 
                ? new DiffuseMaterial(Brushes.Lime) // Яркий зеленый
                : new DiffuseMaterial(Brushes.Red);  // Красный
            
            // Добавляем блеск для лучшей видимости
            var materialGroup = new MaterialGroup();
            materialGroup.Children.Add(material);
            materialGroup.Children.Add(new SpecularMaterial(Brushes.White, 50));
            
            pointsGroup.Children.Add(new GeometryModel3D(sphereMesh, materialGroup));
        }
        
        MeasurementPointsModel = pointsGroup;
    }

    private MeshGeometry3D CreateSphere(Point3D center, double radius)
    {
        var mesh = new MeshGeometry3D();
        int segments = 8;
        
        for (int i = 0; i <= segments; i++)
        {
            for (int j = 0; j <= segments; j++)
            {
                double theta = i * Math.PI / segments;
                double phi = j * 2 * Math.PI / segments;
                
                double x = center.X + radius * Math.Sin(theta) * Math.Cos(phi);
                double y = center.Y + radius * Math.Sin(theta) * Math.Sin(phi);
                double z = center.Z + radius * Math.Cos(theta);
                
                mesh.Positions.Add(new Point3D(x, y, z));
            }
        }
        
        for (int i = 0; i < segments; i++)
        {
            for (int j = 0; j < segments; j++)
            {
                int p1 = i * (segments + 1) + j;
                int p2 = p1 + segments + 1;
                int p3 = p1 + 1;
                int p4 = p2 + 1;
                
                mesh.TriangleIndices.Add(p1); mesh.TriangleIndices.Add(p2); mesh.TriangleIndices.Add(p3);
                mesh.TriangleIndices.Add(p3); mesh.TriangleIndices.Add(p2); mesh.TriangleIndices.Add(p4);
            }
        }
        
        return mesh;
    }

    private void AddSampleData()
    {
        // Точки ВОКРУГ куба (60x30x15), а не НА нем для лучшей видимости
        // Куб: от -30 до +30 по X, от -15 до +15 по Y, от -7.5 до +7.5 по Z
        
        // Точки над кубом
        MeasurementResults.Add(new MeasurementResult { Name = "Punkt 1", X = -25, Y = 20, Z = 0, Nominal = 25.0, Actual = 25.0 });
        MeasurementResults.Add(new MeasurementResult { Name = "Punkt 2", X = 0, Y = 20, Z = 0, Nominal = 25.0, Actual = 24.8 });
        MeasurementResults.Add(new MeasurementResult { Name = "Punkt 3", X = 25, Y = 20, Z = 0, Nominal = 25.0, Actual = 25.2 });
        
        // Точки перед кубом
        MeasurementResults.Add(new MeasurementResult { Name = "Punkt 4", X = -20, Y = 0, Z = -20, Nominal = 30.0, Actual = 29.9 });
        MeasurementResults.Add(new MeasurementResult { Name = "Punkt 5", X = 20, Y = 0, Z = -20, Nominal = 30.0, Actual = 30.1 });
        
        // Точки сбоку от куба
        MeasurementResults.Add(new MeasurementResult { Name = "Punkt 6", X = 45, Y = 0, Z = 5, Nominal = 15.0, Actual = 15.3 }); // Красная точка (отклонение > 0.15)
        MeasurementResults.Add(new MeasurementResult { Name = "Punkt 7", X = -45, Y = 0, Z = -5, Nominal = 15.0, Actual = 14.9 });
    }

    public RelayCommand LoadModelCommand { get; }
    public RelayCommand StartMeasurementCommand { get; }
    public RelayCommand ExportReportCommand { get; }
    public RelayCommand ShowDemoModelCommand { get; }
}
