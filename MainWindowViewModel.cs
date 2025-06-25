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

        // Normalen für besseres Licht
        var frontNormal = new Vector3D(0, 0, -1);
        var backNormal = new Vector3D(0, 0, 1);
        var leftNormal = new Vector3D(-1, 0, 0);
        var rightNormal = new Vector3D(1, 0, 0);
        var topNormal = new Vector3D(0, 1, 0);
        var bottomNormal = new Vector3D(0, -1, 0);

        // Erweiterte Vertices für jede Seite (für korrekte Normalen)
        // Vorderseite
        mesh.Positions.Add(new Point3D(-30, -15, -7.5)); mesh.Normals.Add(frontNormal); // 8
        mesh.Positions.Add(new Point3D(30, -15, -7.5));  mesh.Normals.Add(frontNormal); // 9
        mesh.Positions.Add(new Point3D(30, 15, -7.5));   mesh.Normals.Add(frontNormal); // 10
        mesh.Positions.Add(new Point3D(-30, 15, -7.5));  mesh.Normals.Add(frontNormal); // 11

        // Rückseite
        mesh.Positions.Add(new Point3D(30, -15, 7.5));   mesh.Normals.Add(backNormal); // 12
        mesh.Positions.Add(new Point3D(-30, -15, 7.5));  mesh.Normals.Add(backNormal); // 13
        mesh.Positions.Add(new Point3D(-30, 15, 7.5));   mesh.Normals.Add(backNormal); // 14
        mesh.Positions.Add(new Point3D(30, 15, 7.5));    mesh.Normals.Add(backNormal); // 15

        // Dreiecke mit neuen Indizes
        // Vorderseite
        mesh.TriangleIndices.Add(8); mesh.TriangleIndices.Add(9); mesh.TriangleIndices.Add(10);
        mesh.TriangleIndices.Add(8); mesh.TriangleIndices.Add(10); mesh.TriangleIndices.Add(11);
        
        // Rückseite
        mesh.TriangleIndices.Add(12); mesh.TriangleIndices.Add(13); mesh.TriangleIndices.Add(14);
        mesh.TriangleIndices.Add(12); mesh.TriangleIndices.Add(14); mesh.TriangleIndices.Add(15);
        
        // Verwende ursprüngliche Vertices für andere Seiten
        // Linke Seite
        mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(7);
        mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(4);
        
        // Rechte Seite
        mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(6);
        mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(2);
        
        // Oberseite
        mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(6);
        mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(7);
        
        // Unterseite
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
            // Finde Punkt auf der Würfeloberfläche für die Platzierung der Kugel
            var surfacePoint = GetSurfacePoint(new Point3D(point.X, point.Y, point.Z));
            
            // Erstelle sehr kleine Kugel direkt auf der Oberfläche
            var sphereMesh = CreateSphere(surfacePoint, 0.4); // Ganz kleine Punkte - Radius 0.4
            var material = Math.Abs(point.Deviation) < 0.15 
                ? new DiffuseMaterial(Brushes.Lime) // Helles Grün
                : new DiffuseMaterial(Brushes.Red);  // Rot
            
            // Füge Glanz für bessere Sichtbarkeit hinzu
            var materialGroup = new MaterialGroup();
            materialGroup.Children.Add(material);
            materialGroup.Children.Add(new SpecularMaterial(Brushes.White, 100));
            materialGroup.Children.Add(new EmissiveMaterial(material.Brush)); // Füge Leuchten für bessere Sichtbarkeit hinzu
            
            pointsGroup.Children.Add(new GeometryModel3D(sphereMesh, materialGroup));
        }
        
        MeasurementPointsModel = pointsGroup;
    }
    
    private Point3D GetSurfacePoint(Point3D referencePoint)
    {
        // Würfelabmessungen: 60x30x15 (von -30 bis +30 auf X, von -15 bis +15 auf Y, von -7.5 bis +7.5 auf Z)
        
        // Berechne Abstände zu jeder Fläche
        double[] distances = new double[6];
        Point3D[] candidatePoints = new Point3D[6];
        
        // Linke Fläche (X = -30)
        distances[0] = Math.Abs(referencePoint.X + 30);
        candidatePoints[0] = new Point3D(-30, Math.Max(-15, Math.Min(15, referencePoint.Y)), Math.Max(-7.5, Math.Min(7.5, referencePoint.Z)));
        
        // Rechte Fläche (X = 30)
        distances[1] = Math.Abs(referencePoint.X - 30);
        candidatePoints[1] = new Point3D(30, Math.Max(-15, Math.Min(15, referencePoint.Y)), Math.Max(-7.5, Math.Min(7.5, referencePoint.Z)));
        
        // Untere Fläche (Y = -15)
        distances[2] = Math.Abs(referencePoint.Y + 15);
        candidatePoints[2] = new Point3D(Math.Max(-30, Math.Min(30, referencePoint.X)), -15, Math.Max(-7.5, Math.Min(7.5, referencePoint.Z)));
        
        // Obere Fläche (Y = 15)
        distances[3] = Math.Abs(referencePoint.Y - 15);
        candidatePoints[3] = new Point3D(Math.Max(-30, Math.Min(30, referencePoint.X)), 15, Math.Max(-7.5, Math.Min(7.5, referencePoint.Z)));
        
        // Vordere Fläche (Z = -7.5)
        distances[4] = Math.Abs(referencePoint.Z + 7.5);
        candidatePoints[4] = new Point3D(Math.Max(-30, Math.Min(30, referencePoint.X)), Math.Max(-15, Math.Min(15, referencePoint.Y)), -7.5);
        
        // Hintere Fläche (Z = 7.5)
        distances[5] = Math.Abs(referencePoint.Z - 7.5);
        candidatePoints[5] = new Point3D(Math.Max(-30, Math.Min(30, referencePoint.X)), Math.Max(-15, Math.Min(15, referencePoint.Y)), 7.5);
        
        // Finde Fläche mit minimaler Entfernung
        int minIndex = 0;
        for (int i = 1; i < 6; i++)
        {
            if (distances[i] < distances[minIndex])
            {
                minIndex = i;
            }
        }
        
        return candidatePoints[minIndex];
    }

    private MeshGeometry3D CreateSphere(Point3D center, double radius)
    {
        var mesh = new MeshGeometry3D();
        int segments = 6; // Reduziere Segmente für kleine Kugeln
        
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
        // Messpunkte, die auf die Würfeloberflächen projiziert werden (60x30x15)
        // Würfel: von -30 bis +30 auf X, von -15 bis +15 auf Y, von -7.5 bis +7.5 auf Z
        
        // Punkte für obere Oberfläche
        MeasurementResults.Add(new MeasurementResult { Name = "Punkt 1", X = -20, Y = 20, Z = 0, Nominal = 25.0, Actual = 25.0 });
        MeasurementResults.Add(new MeasurementResult { Name = "Punkt 2", X = 0, Y = 20, Z = 0, Nominal = 25.0, Actual = 24.8 });
        MeasurementResults.Add(new MeasurementResult { Name = "Punkt 3", X = 20, Y = 20, Z = 0, Nominal = 25.0, Actual = 25.2 });
        
        // Punkte für vordere Oberfläche
        MeasurementResults.Add(new MeasurementResult { Name = "Punkt 4", X = -15, Y = 0, Z = -15, Nominal = 30.0, Actual = 29.9 });
        MeasurementResults.Add(new MeasurementResult { Name = "Punkt 5", X = 15, Y = 0, Z = -15, Nominal = 30.0, Actual = 30.1 });
        
        // Punkte für seitliche Oberflächen
        MeasurementResults.Add(new MeasurementResult { Name = "Punkt 6", X = 40, Y = 0, Z = 0, Nominal = 15.0, Actual = 15.3 }); // Roter Punkt (Abweichung > 0.15)
        MeasurementResults.Add(new MeasurementResult { Name = "Punkt 7", X = -40, Y = -5, Z = 0, Nominal = 15.0, Actual = 14.9 });
    }

    public RelayCommand LoadModelCommand { get; }
    public RelayCommand StartMeasurementCommand { get; }
    public RelayCommand ExportReportCommand { get; }
    public RelayCommand ShowDemoModelCommand { get; }
}
