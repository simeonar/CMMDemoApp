using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using Media3D = System.Windows.Media.Media3D;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using CMMDemoApp.Models;

namespace CMMDemoApp;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<MeasurementResult> MeasurementResults { get; } = new();

    [ObservableProperty]
    private string selectedPointInfo = "Wählen Sie einen Punkt für Details...";

    [ObservableProperty]
    private Camera? camera;

    [ObservableProperty]
    private IEffectsManager? effectsManager;

    [ObservableProperty]
    private LineGeometry3D? gridLinesGeometry;

    [ObservableProperty]
    private MeshGeometry3D? modelGeometry;

    [ObservableProperty]
    private Material? modelMaterial;

    [ObservableProperty]
    private MeshGeometry3D? probeGeometry;

    [ObservableProperty]
    private Material? probeMaterial;

    [ObservableProperty]
    private Media3D.Transform3D? probeTransform;

    public MainWindowViewModel()
    {
        InitializeScene();
        LoadModelCommand = new RelayCommand(LoadModel);
        StartMeasurementCommand = new RelayCommand(StartMeasurement);
        ExportReportCommand = new RelayCommand(ExportReport);
        
        // Füge Demo-Daten hinzu
        AddSampleData();
    }

    private void InitializeScene()
    {
        // Kamera-Einstellung
        Camera = new PerspectiveCamera
        {
            Position = new Media3D.Point3D(0, 0, 100),
            LookDirection = new Media3D.Vector3D(0, 0, -1),
            UpDirection = new Media3D.Vector3D(0, 1, 0),
            FieldOfView = 45
        };

        // Effekt-Manager
        EffectsManager = new DefaultEffectsManager();

        // Erstelle Koordinatengitter
        CreateGrid();

        // Erstelle Tastkopf
        CreateProbe();

        // Material für Modell
        ModelMaterial = DiffuseMaterials.Blue;
    }

    private void CreateGrid()
    {
        var builder = new LineBuilder();
        var gridSize = 100f;
        var gridStep = 10f;

        // Linien entlang X
        for (float i = -gridSize; i <= gridSize; i += gridStep)
        {
            builder.AddLine(new Vector3(i, 0, -gridSize), new Vector3(i, 0, gridSize));
        }

        // Linien entlang Z
        for (float i = -gridSize; i <= gridSize; i += gridStep)
        {
            builder.AddLine(new Vector3(-gridSize, 0, i), new Vector3(gridSize, 0, i));
        }

        GridLinesGeometry = builder.ToLineGeometry3D();
    }

    private void CreateProbe()
    {
        var builder = new MeshBuilder();
        
        // Erstelle einen einfachen Tastkopf (Kugel + Stab)
        builder.AddSphere(new Vector3(0, 0, 0), 2f, 12, 12);
        builder.AddCylinder(new Vector3(0, 0, 0), new Vector3(0, -20, 0), 1f, 8);

        ProbeGeometry = builder.ToMeshGeometry3D();
        ProbeMaterial = DiffuseMaterials.Red;
        ProbeTransform = new Media3D.TranslateTransform3D(0, 50, 0);
    }

    private void AddSampleData()
    {
        MeasurementResults.Add(new MeasurementResult { Name = "Punkt 1", Nominal = 25.000, Actual = 25.002 });
        MeasurementResults.Add(new MeasurementResult { Name = "Punkt 2", Nominal = 50.000, Actual = 49.998 });
        MeasurementResults.Add(new MeasurementResult { Name = "Durchmesser", Nominal = 20.000, Actual = 20.005 });
    }

    private void LoadModel()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "STL Files (*.stl)|*.stl|All Files (*.*)|*.*",
            Title = "STL-Modell auswählen"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                // Hier wird die STL-Datei-Ladelogik implementiert
                // Erstelle vorerst eine einfache Geometrie als Beispiel
                CreateSampleModel();
                
                MessageBox.Show($"Modell geladen: {Path.GetFileName(dialog.FileName)}", 
                               "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden des Modells: {ex.Message}", 
                               "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void CreateSampleModel()
    {
        var builder = new MeshBuilder();
        
        // Erstelle ein einfaches Testteil - Quader mit Bohrung
        builder.AddBox(new Vector3(0, 0, 0), 40, 20, 10);
        
        ModelGeometry = builder.ToMeshGeometry3D();
        ModelMaterial = DiffuseMaterials.Silver;
    }

    private void StartMeasurement()
    {
        // Simulation des Messprozesses
        MessageBox.Show("Messung gestartet! (Demo-Modus)", "KMM-System", MessageBoxButton.OK, MessageBoxImage.Information);
        
        // Füge einen neuen Messpunkt hinzu
        var random = new Random();
        var newMeasurement = new MeasurementResult 
        { 
            Name = $"Punkt {MeasurementResults.Count + 1}", 
            Nominal = 30.000, 
            Actual = 30.000 + (random.NextDouble() - 0.5) * 0.02  // ±0.01mm Abweichung
        };
        MeasurementResults.Add(newMeasurement);
    }

    private void ExportReport()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
            Title = "Messbericht speichern",
            FileName = $"KMM_Bericht_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Name,Sollwert,Istwert,Abweichung");
                
                foreach (var result in MeasurementResults)
                {
                    csv.AppendLine($"{result.Name},{result.Nominal:F3},{result.Actual:F3},{result.Deviation:F3}");
                }
                
                File.WriteAllText(dialog.FileName, csv.ToString());
                
                MessageBox.Show($"Bericht gespeichert: {dialog.FileName}", 
                               "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern des Berichts: {ex.Message}", 
                               "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public RelayCommand LoadModelCommand { get; }
    public RelayCommand StartMeasurementCommand { get; }
    public RelayCommand ExportReportCommand { get; }
}
