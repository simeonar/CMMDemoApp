using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CMMDemoApp.Models;
using CMMDemoApp.Services;
using CMMDemoApp.ViewModels;
using CMMDemoApp.Views;
using Newtonsoft.Json;
using System.IO;

namespace CMMDemoApp;

/// <summary>
/// Main ViewModel for the CMM application that manages the measurement workflow.
/// This ViewModel is responsible for:
/// 1. Managing the primary tree/table-based visualization of measurements
/// 2. Coordinating measurement operations
/// 3. Managing measurement data and status
/// 4. Launching the separate 3D visualization window when needed
/// 
/// The separation between tree/table view and 3D visualization is an intentional architectural decision that:
/// - Keeps the primary workflow simple and efficient
/// - Allows users to work with measurements while having 3D view on another screen
/// - Improves application performance by separating heavy 3D rendering
/// - Makes the application more maintainable by clear separation of concerns
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IMeasurementService _measurementService;
    private readonly IMeasurementSimulationService _simulationService;
    
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
    private ObservableCollection<PartMeasurement> parts = new();

    private MeasurementPoint? selectedPoint;
    public MeasurementPoint? SelectedPoint
    {
        get => selectedPoint;
        set
        {
            if (SetProperty(ref selectedPoint, value))
            {
                UpdateSelectedPointInfo();
                UpdateMeasurementResults();
            }
        }
    }

    private void UpdateSelectedPointInfo()
    {
        if (selectedPoint == null)
        {
            SelectedPointInfo = "Wählen Sie einen Punkt für Details...";
            return;
        }

        SelectedPointInfo = $"Punkt: {selectedPoint.Name}\n" +
            $"Status: {selectedPoint.Status}\n" +
            $"Erwartete Position: X={selectedPoint.ExpectedX:F3}, Y={selectedPoint.ExpectedY:F3}, Z={selectedPoint.ExpectedZ:F3}\n" +
            $"Toleranz: ±{selectedPoint.Tolerance:F3}";
    }

    private void UpdateMeasurementResults()
    {
        MeasurementResults.Clear();
        if (selectedPoint == null) return;

        if (selectedPoint.MeasuredX.HasValue)
        {
            MeasurementResults.Add(new MeasurementResult 
            { 
                Name = "X-Position",
                Nominal = selectedPoint.ExpectedX,
                Actual = selectedPoint.MeasuredX.Value,
                Deviation = selectedPoint.MeasuredX.Value - selectedPoint.ExpectedX
            });
        }
        if (selectedPoint.MeasuredY.HasValue)
        {
            MeasurementResults.Add(new MeasurementResult 
            { 
                Name = "Y-Position",
                Nominal = selectedPoint.ExpectedY,
                Actual = selectedPoint.MeasuredY.Value,
                Deviation = selectedPoint.MeasuredY.Value - selectedPoint.ExpectedY
            });
        }
        if (selectedPoint.MeasuredZ.HasValue)
        {
            MeasurementResults.Add(new MeasurementResult 
            { 
                Name = "Z-Position",
                Nominal = selectedPoint.ExpectedZ,
                Actual = selectedPoint.MeasuredZ.Value,
                Deviation = selectedPoint.MeasuredZ.Value - selectedPoint.ExpectedZ
            });
        }
    }

    // Removed as replaced by LoadDemoDataAsync

        Parts.Add(demoPart);
    }

    private async Task LoadDemoDataAsync()
    {
        try
        {
            var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "input_data", "sample_part.json");
            if (!File.Exists(jsonPath))
            {
                Debug.WriteLine($"Demo data file not found at {jsonPath}");
                return;
            }

            var jsonContent = await File.ReadAllTextAsync(jsonPath);
            var demoData = JsonConvert.DeserializeObject<PartMeasurement>(jsonContent);
            
            if (demoData != null)
            {
                Parts.Add(demoData);
                OnPropertyChanged(nameof(Parts));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading demo data: {ex.Message}");
            MessageBox.Show("Error loading demo data: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public IAsyncRelayCommand<MeasurementPoint> MeasurePointCommand { get; }
    public IAsyncRelayCommand StartMeasurementCommand { get; }
    public IRelayCommand LoadModelCommand { get; }
    public IRelayCommand ExportReportCommand { get; }
    public IRelayCommand ShowDemoModelCommand { get; }
    public IRelayCommand ExpandAllCommand { get; }
    public IRelayCommand CollapseAllCommand { get; }

    public MainWindowViewModel(IMeasurementService measurementService, IMeasurementSimulationService simulationService)
    {
        _measurementService = measurementService;
        _simulationService = simulationService;
        
        LoadModelCommand = new RelayCommand(LoadModel);
        StartMeasurementCommand = new AsyncRelayCommand(StartMeasurementAsync);
        ExportReportCommand = new RelayCommand(ExportReport);
        ShowDemoModelCommand = new RelayCommand(ShowDemoModel);
        
        MeasurePointCommand = new AsyncRelayCommand<MeasurementPoint>(SimulateMeasurementAsync);
        ExpandAllCommand = new RelayCommand(ExpandAll);
        CollapseAllCommand = new RelayCommand(CollapseAll);
        
        // Update time every minute
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        timer.Tick += (s, e) => Now = DateTime.Now.ToString("HH:mm");
        timer.Start();

        // Load demo data
        _ = LoadDemoDataAsync();
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

    public void ClearMeasurementPoints()
    {
        MeasurementPointsModel?.Children.Clear();
    }

    private Model3D ConvertMeasurementPointToModel3D(MeasurementPoint point)
    {
        var sphere = new MeshGeometry3D();
        var material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));

        // Example: Create a sphere at the point's coordinates
        var transform = new TranslateTransform3D(point.ExpectedX, point.ExpectedY, point.ExpectedZ);
        var geometryModel = new GeometryModel3D(sphere, material)
        {
            Transform = transform
        };

        return geometryModel;
    }

    public void AddMeasurementPoints(IEnumerable<MeasurementPoint> points)
    {
        if (MeasurementPointsModel != null)
        {
            foreach (var point in points)
            {
                var model3D = ConvertMeasurementPointToModel3D(point);
                MeasurementPointsModel.Children.Add(model3D);
            }
        }
    }

    // Commands moved to the top of the file

    private async Task StartMeasurementAsync()
    {
        foreach (var part in Parts)
        {
            foreach (var point in part.Points)
            {
                if (point.Status == MeasurementStatus.NotStarted || point.Status == MeasurementStatus.Failed)
                {
                    await SimulateMeasurementAsync(point);
                    // Add a small delay between measurements to make it more realistic
                    await Task.Delay(500);
                }
            }
        }
        
        MessageBox.Show("Measurement process completed!", "CMM System", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task SimulateMeasurementAsync(MeasurementPoint? point)
    {
        if (point == null) return;

        try
        {
            point.Status = MeasurementStatus.InProgress;
            var result = await _simulationService.SimulateMeasurementAsync(point);
            MeasurementResults.Add(result);
            point.Status = result.IsWithinTolerance ? MeasurementStatus.Completed : MeasurementStatus.Failed;
            SelectedPointInfo = $"Point: {point.Name}\nNominal: {result.Nominal}\nActual: {result.Actual}\nDeviation: {result.Deviation}\nStatus: {result.Status}";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error during measurement simulation: {ex.Message}");
            point.Status = MeasurementStatus.Failed;
            MessageBox.Show("Error during measurement: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
