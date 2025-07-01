using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CMMDemoApp.Models;
using CMMDemoApp.Services;
using CMMDemoApp.ViewModels;
using CMMDemoApp.Views;

namespace CMMDemoApp;

/// <summary>
/// Main ViewModel for the application
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IMeasurementService _measurementService;
    
    public ObservableCollection<MeasurementResult> MeasurementResults { get; } = new();

    [ObservableProperty]
    private string selectedPointInfo = "Wählen Sie einen Punkt für Details...";

    [ObservableProperty]
    private MeshGeometry3D? demoModelGeometry;

    [ObservableProperty]
    private Model3DGroup? measurementPointsModel;

    [ObservableProperty]
    private string now = DateTime.Now.ToString("HH:mm");
    
    [ObservableProperty]
    private ObservableCollection<PartMeasurement> _parts = new();

    public IRelayCommand<MeasurementPoint> MeasurePointCommand { get; }
    public IRelayCommand ExpandAllCommand { get; }
    public IRelayCommand CollapseAllCommand { get; }

    public MainWindowViewModel(IMeasurementService measurementService)
    {
        _measurementService = measurementService;
        
        LoadModelCommand = new RelayCommand(LoadModel);
        StartMeasurementCommand = new RelayCommand(StartMeasurement);
        ExportReportCommand = new RelayCommand(ExportReport);
        ShowDemoModelCommand = new RelayCommand(ShowDemoModel);
        
        MeasurePointCommand = new AsyncRelayCommand<MeasurementPoint>(MeasurePointAsync);
        ExpandAllCommand = new RelayCommand(ExpandAll);
        CollapseAllCommand = new RelayCommand(CollapseAll);
        
        // Update time every minute
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        timer.Tick += (s, e) => Now = DateTime.Now.ToString("HH:mm");
        timer.Start();
        
        LoadInitialData();
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

    /// <summary>
    /// Opens a separate window to display the demo model
    /// </summary>
    public void ShowDemoModel()
    {
        Debug.WriteLine("[CMM] ShowDemoModel command executed");
        
        try
        {
            // Create ViewModel instance directly, not relying on DI
            var demoViewModel = new DemoModelViewModel();
            Debug.WriteLine("[CMM] Created new DemoModelViewModel instance directly");
            
            // Create and display 3D model window
            var demoWindow = new DemoModelWindow();
            demoWindow.DataContext = demoViewModel;
            demoWindow.Show();
            Debug.WriteLine("[CMM] DemoModelWindow created and shown");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CMM] Error in ShowDemoModel: {ex.Message}");
            MessageBox.Show($"Error displaying demo model: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void LoadInitialData()
    {
        try
        {
            var part = await _measurementService.LoadExpectedDataAsync(@"TestData\input_data\part001_expected.xml");
            if (part != null)
            {
                Parts.Add(part);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CMM] Error loading initial data: {ex.Message}");
            MessageBox.Show("Failed to load initial measurement data", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task MeasurePointAsync(MeasurementPoint? point)
    {
        if (point == null) return;
        
        try
        {
            await _measurementService.SimulateMeasurementAsync(point);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CMM] Error measuring point: {ex.Message}");
            MessageBox.Show($"Failed to measure point: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void ExpandAll()
    {
        foreach (var part in Parts)
        {
            foreach (var point in part.Points)
            {
                point.IsExpanded = true;
            }
        }
    }
    
    private void CollapseAll()
    {
        foreach (var part in Parts)
        {
            foreach (var point in part.Points)
            {
                point.IsExpanded = false;
            }
        }
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
