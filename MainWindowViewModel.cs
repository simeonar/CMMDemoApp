using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Numerics;
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
    [ObservableProperty]
    private string selectedPointInfo = "Wählen Sie einen Punkt für Details...";

    [ObservableProperty]
    private MeshGeometry3D? demoModelGeometry;

    [ObservableProperty]
    private Model3DGroup? measurementPointsModel;

    [ObservableProperty]
    private string _now = DateTime.Now.ToString("HH:mm");
    
    [ObservableProperty]
    private ObservableCollection<PartMeasurement> _parts = new();

    [ObservableProperty]
    private bool _isMeasurementInProgress;

    [ObservableProperty]
    private string _measurementStatus = string.Empty;

    [ObservableProperty]
    private double _overallProgress;

    private readonly IMeasurementService _measurementService;
    private readonly IMeasurementSimulationService _simulationService;

    // Commands
    public IAsyncRelayCommand LoadModelCommand { get; }
    public IAsyncRelayCommand StartMeasurementCommand { get; }
    public IRelayCommand ExportReportCommand { get; }
    public IRelayCommand ShowDemoModelCommand { get; }
    public IAsyncRelayCommand<MeasurementPoint> MeasurePointCommand { get; }
    public IRelayCommand ExpandAllCommand { get; }
    public IRelayCommand CollapseAllCommand { get; }

    public ObservableCollection<MeasurementResult> MeasurementResults { get; } = new();

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
            $"Nominal Position: X={selectedPoint.NominalX:F3}, Y={selectedPoint.NominalY:F3}, Z={selectedPoint.NominalZ:F3}\n" +
            $"Tolerance: {selectedPoint.ToleranceMin:F3} to {selectedPoint.ToleranceMax:F3}";
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
                Nominal = new Vector3((float)selectedPoint.NominalX, 0, 0),
                Actual = new Vector3((float)selectedPoint.MeasuredX.Value, 0, 0),
                Deviation = Math.Abs(selectedPoint.MeasuredX.Value - selectedPoint.NominalX),
                ToleranceMin = selectedPoint.ToleranceMin,
                ToleranceMax = selectedPoint.ToleranceMax,
                Status = Math.Abs(selectedPoint.MeasuredX.Value - selectedPoint.NominalX) <= selectedPoint.ToleranceMax ? 
                    Models.MeasurementStatus.Completed : Models.MeasurementStatus.Failed
            });
        }
        if (selectedPoint.MeasuredY.HasValue)
        {
            MeasurementResults.Add(new MeasurementResult 
            { 
                Name = "Y-Position",
                Nominal = new Vector3(0, (float)selectedPoint.NominalY, 0),
                Actual = new Vector3(0, (float)selectedPoint.MeasuredY.Value, 0),
                Deviation = Math.Abs(selectedPoint.MeasuredY.Value - selectedPoint.NominalY),
                ToleranceMin = selectedPoint.ToleranceMin,
                ToleranceMax = selectedPoint.ToleranceMax,
                Status = Math.Abs(selectedPoint.MeasuredY.Value - selectedPoint.NominalY) <= selectedPoint.ToleranceMax ? 
                    Models.MeasurementStatus.Completed : Models.MeasurementStatus.Failed
            });
        }
        if (selectedPoint.MeasuredZ.HasValue)
        {
            MeasurementResults.Add(new MeasurementResult 
            { 
                Name = "Z-Position",
                Nominal = new Vector3(0, 0, (float)selectedPoint.NominalZ),
                Actual = new Vector3(0, 0, (float)selectedPoint.MeasuredZ.Value),
                Deviation = Math.Abs(selectedPoint.MeasuredZ.Value - selectedPoint.NominalZ),
                ToleranceMin = selectedPoint.ToleranceMin,
                ToleranceMax = selectedPoint.ToleranceMax,
                Status = Math.Abs(selectedPoint.MeasuredZ.Value - selectedPoint.NominalZ) <= selectedPoint.ToleranceMax ? 
                    Models.MeasurementStatus.Completed : Models.MeasurementStatus.Failed
            });
        }
    }

    // Removed as replaced by LoadDemoDataAsync

    private async Task LoadDemoDataAsync()
    {
        try
        {
            // Clear existing data
            Parts.Clear();

            // Use the project root directory directly
            var projectDir = @"c:\Users\arshy\source\repos\CMMDemoApp";
            var part1Path = Path.Combine(projectDir, "TestData", "input_data", "part001_expected.xml");
            var part2Path = Path.Combine(projectDir, "TestData", "input_data", "part002_expected.xml");

            Debug.WriteLine($"Loading demo data from:\n{part1Path}\n{part2Path}");

            if (!File.Exists(part1Path) || !File.Exists(part2Path))
            {
                throw new FileNotFoundException("Demo data files not found. Expected files:\n" + part1Path + "\n" + part2Path);
            }

            // Load both parts using the measurement service
            var part1 = await _measurementService.LoadExpectedDataAsync(part1Path);
            var part2 = await _measurementService.LoadExpectedDataAsync(part2Path);

            if (part1 != null) Parts.Add(part1);
            if (part2 != null) Parts.Add(part2);

            // Force UI update
            OnPropertyChanged(nameof(Parts));

            // Expand the first part if we have any
            if (Parts.Count > 0)
            {
                Parts[0].IsExpanded = true;
            }

            Debug.WriteLine($"Successfully loaded {Parts.Count} parts with {Parts.Sum(p => p.Points.Count)} total points");
        }
        catch (Exception ex)
        {
            var message = $"Error loading demo data: {ex.Message}";
            Debug.WriteLine(message);
            Debug.WriteLine(ex.StackTrace);
            await Task.Delay(100); // Ensure UI is ready
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public MainWindowViewModel(IMeasurementService measurementService, IMeasurementSimulationService simulationService)
    {
        _measurementService = measurementService ?? throw new ArgumentNullException(nameof(measurementService));
        _simulationService = simulationService ?? throw new ArgumentNullException(nameof(simulationService));
        
        LoadModelCommand = new AsyncRelayCommand(LoadModel);
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
    }

    private async Task LoadModel()
    {
        try
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Load Model or Measurement Data",
                Filter = "All Supported Files|*.xml;*.stl|XML Measurement Data|*.xml|STL Models|*.stl",
                InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "input_data")
            };

            if (dialog.ShowDialog() == true)
            {
                var extension = Path.GetExtension(dialog.FileName).ToLower();
                
                if (extension == ".xml")
                {
                    // Load measurement data
                    var part = await _measurementService.LoadExpectedDataAsync(dialog.FileName);
                    if (part != null)
                    {
                        Parts.Add(part);
                        part.IsExpanded = true;
                        Debug.WriteLine($"Successfully loaded part {part.Name} with {part.Points.Count} points");
                    }
                }
                else if (extension == ".stl")
                {
                    MessageBox.Show("STL model loading will be implemented in the next version", 
                        "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            var message = $"Error loading file: {ex.Message}";
            Debug.WriteLine(message);
            Debug.WriteLine(ex.StackTrace);
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
        // Results will be added during measurement simulation
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
        var transform = new TranslateTransform3D(point.NominalX, point.NominalY, point.NominalZ);
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
        if (IsMeasurementInProgress)
        {
            MessageBox.Show("Eine Messung läuft bereits.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (Parts.Count == 0)
        {
            MessageBox.Show("Bitte laden Sie zuerst ein Messprogramm.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            IsMeasurementInProgress = true;
            OverallProgress = 0;
            MeasurementResults.Clear(); // Clear previous results

            // Count total points for progress calculation
            int totalPoints = Parts.Sum(p => p.Points.Count);
            int completedPoints = 0;

            foreach (var part in Parts)
            {
                MeasurementStatus = $"Messe Teil: {part.Name}";
                part.IsExpanded = true;

                foreach (var point in part.Points)
                {
                    // Check if point needs measurement
                    if (point.Status is Models.MeasurementStatus.NotStarted or Models.MeasurementStatus.Failed)
                    {
                        try
                        {
                            point.MeasurementProgress = 0;
                            point.IsInProgress = true;
                            MeasurementStatus = $"Messe Punkt: {point.Name} in {part.Name}";

                            // Simulate measurement progress
                            for (int i = 0; i <= 100; i += 10)
                            {
                                point.MeasurementProgress = i;
                                await Task.Delay(50); // Simulate measurement steps
                            }

                            var result = await _simulationService.SimulateMeasurementAsync(point);
                            
                            // Update point with measurement results
                            point.MeasuredX = (double)result.Actual.X;
                            point.MeasuredY = (double)result.Actual.Y;
                            point.MeasuredZ = (double)result.Actual.Z;
                            
                            point.Status = result.IsWithinTolerance ? Models.MeasurementStatus.Completed : Models.MeasurementStatus.Failed;
                            
                            // Add result to the results table
                            MeasurementResults.Add(result);

                            // Update point info if this point is selected
                            if (point == SelectedPoint)
                            {
                                SelectedPointInfo = $"Point: {point.Name}\nNominal: {result.FormattedNominal}\nActual: {result.FormattedActual}\nDeviation: {result.FormattedDeviation}\nStatus: {result.Status}";
                            }
                        }
                        catch (Exception ex)
                        {
                            point.Status = Models.MeasurementStatus.Failed;
                            Debug.WriteLine($"Error measuring point {point.Name}: {ex.Message}");
                        }
                        finally
                        {
                            point.IsInProgress = false;
                            point.MeasurementProgress = 100;
                            completedPoints++;
                            OverallProgress = (completedPoints * 100.0) / totalPoints;
                        }
                    }
                    else
                    {
                        completedPoints++;
                        OverallProgress = (completedPoints * 100.0) / totalPoints;
                    }

                    if (point.IsSelected)
                    {
                        UpdateSelectedPointInfo();
                        UpdateMeasurementResults();
                    }
                }
            }

            MeasurementStatus = "Messung abgeschlossen";
            MessageBox.Show("Messung aller Punkte abgeschlossen.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error during measurement: {ex.Message}");
            MessageBox.Show($"Fehler während der Messung: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsMeasurementInProgress = false;
        }
    }

    private async Task SimulateMeasurementAsync(MeasurementPoint? point)
    {
        if (point == null) return;

        try
        {
            point.Status = Models.MeasurementStatus.InProgress;
            var result = await _simulationService.SimulateMeasurementAsync(point);
            MeasurementResults.Add(result);

            // Take over measured values from result
            point.MeasuredX = (double)result.Actual.X;
            point.MeasuredY = (double)result.Actual.Y;
            point.MeasuredZ = (double)result.Actual.Z;

            // Update point status based on result
            point.Status = result.IsWithinTolerance ? Models.MeasurementStatus.Completed : Models.MeasurementStatus.Failed;

            SelectedPointInfo = $"Point: {point.Name}\nNominal: {result.Nominal}\nActual: {result.Actual}\nDeviation: {result.Deviation}\nStatus: {result.Status}";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error during measurement simulation: {ex.Message}");
            point.Status = Models.MeasurementStatus.Failed;
            MessageBox.Show("Error during measurement: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
