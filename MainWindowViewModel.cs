using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;
using System.Windows;
using CMMDemoApp.Models;

namespace CMMDemoApp;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<MeasurementResult> MeasurementResults { get; } = new();

    [ObservableProperty]
    private string selectedPointInfo = "Wählen Sie einen Punkt für Details...";

    [ObservableProperty]
    private PerspectiveCamera? camera;

    [ObservableProperty]
    private Model3D? gridLinesModel;

    [ObservableProperty]
    private Model3D? model3D;

    [ObservableProperty]
    private Model3D? probeModel;

    public MainWindowViewModel()
    {
        InitializeScene();
        ShowDemoModelCommand = new RelayCommand(ShowDemoModel);
        AddSampleData();
    }

    private void InitializeScene()
    {
        Camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 100),
            LookDirection = new Vector3D(0, 0, -1),
            UpDirection = new Vector3D(0, 1, 0),
            FieldOfView = 45
        };
        // Для простоты: только проба и демо-модель
        CreateProbe();
    }

    private void CreateProbe()
    {
        var builder = new MeshBuilder(false, false);
        builder.AddSphere(new Point3D(0, 0, 0), 2, 12, 12);
        builder.AddCylinder(new Point3D(0, 0, 0), new Point3D(0, -20, 0), 1, 8);
        var mesh = builder.ToMesh();
        var material = Materials.Red;
        ProbeModel = new GeometryModel3D(mesh, material);
    }

    private void AddSampleData()
    {
        MeasurementResults.Add(new MeasurementResult { Name = "Punkt 1", Nominal = 25.000, Actual = 25.002 });
        MeasurementResults.Add(new MeasurementResult { Name = "Punkt 2", Nominal = 50.000, Actual = 49.998 });
        MeasurementResults.Add(new MeasurementResult { Name = "Durchmesser", Nominal = 20.000, Actual = 20.005 });
    }

    private void ShowDemoModel()
    {
        var builder = new MeshBuilder(false, false);
        builder.AddBox(new Point3D(0, 0, 0), 60, 30, 15);
        var mesh = builder.ToMesh();
        var material = Materials.Gray;
        Model3D = new GeometryModel3D(mesh, material);
        SelectedPointInfo = "Demo-Modell: 60x30x15 mm";
        System.Windows.MessageBox.Show("ShowDemoModel вызван!", "Debug", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }

    public RelayCommand ShowDemoModelCommand { get; }
}
