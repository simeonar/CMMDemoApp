using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Numerics;
using CMMDemoApp.Models;
using CMMDemoApp.Services;
using CMMDemoApp.ViewModels;
using CMMDemoApp.Views;

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
    private ObservableCollection<PartMeasurement> _parts = new();

    [ObservableProperty]
    private bool _isMeasurementInProgress;

    [ObservableProperty]
    private string _measurementStatus = string.Empty;

    [ObservableProperty]
    private string _selectedReportFormat = "PDF Report (*.pdf)";

    [ObservableProperty]
    private ReportOptions _reportOptions = new()
    {
        IncludeStatistics = true,
        IncludeGraphs = true,
        IncludeIndividualPoints = true,
        Include3DVisualization = false
    };

    public ObservableCollection<string> AvailableReportFormats { get; } = new()
    {
        "PDF Report (*.pdf)",
        "XML Data (*.xml)",
        "CSV Data (*.csv)",
        "HTML Report (*.html)"
    };

    private readonly IMeasurementService _measurementService;
    private readonly IMeasurementSimulationService _simulationService;
    private readonly IReportingService _reportingService;
    private readonly Random _random = new();

    // Commands
    public IAsyncRelayCommand LoadModelCommand { get; }
    public IAsyncRelayCommand StartMeasurementCommand { get; }
    public IRelayCommand ExportReportCommand { get; }
    public IRelayCommand ShowDemoModelCommand { get; }
    public IAsyncRelayCommand<MeasurementPoint> MeasurePointCommand { get; }
    public IRelayCommand ExpandAllCommand { get; }
    public IRelayCommand CollapseAllCommand { get; }

    // Visualization Commands
    public IRelayCommand OpenStatisticsSummaryCommand { get; }
    public IRelayCommand OpenPointDetailsCommand { get; }
    public IRelayCommand OpenToleranceAnalysisCommand { get; }
    public IRelayCommand OpenSuccessFailureStatusCommand { get; }
    public IRelayCommand OpenDeviationMeasurementsCommand { get; }
    public IRelayCommand OpenGraphsCommand { get; }

    // Export Commands
    public IRelayCommand ExportPdfReportCommand { get; }
    public IRelayCommand ExportXmlDataCommand { get; }
    public IRelayCommand ExportCsvDataCommand { get; }
    public IRelayCommand ExportHtmlReportCommand { get; }

    public ObservableCollection<MeasurementResult> MeasurementResults { get; } = new();

    [ObservableProperty]
    private MeasurementPoint? _selectedPoint;

    partial void OnSelectedPointChanged(MeasurementPoint? value)
    {
        // Clear selection from previously selected point
        if (SelectedPoint != null && SelectedPoint != value)
        {
            SelectedPoint.IsSelected = false;
        }

        if (value != null)
        {
            selectedPointInfo = $"Point: {value.Name}\n" +
                $"Status: {value.Status}\n" +
                $"Nominal Position: X={value.NominalX:F3}, Y={value.NominalY:F3}, Z={value.NominalZ:F3}\n" +
                $"Tolerance: {value.ToleranceMin:F3} to {value.ToleranceMax:F3}";
            
            // Set selection on new point
            value.IsSelected = true;
        }
        else
        {
            selectedPointInfo = "Select a point to view details...";
        }

        UpdateMeasurementResults();
    }

    private void UpdateMeasurementResults()
    {
        MeasurementResults.Clear();
        if (SelectedPoint == null) return;

        if (SelectedPoint.MeasuredX.HasValue)
        {
            MeasurementResults.Add(new MeasurementResult 
            { 
                Name = "X-Position",
                Nominal = new Vector3((float)SelectedPoint.NominalX, 0, 0),
                Actual = new Vector3((float)SelectedPoint.MeasuredX.Value, 0, 0),
                Deviation = Math.Abs(SelectedPoint.MeasuredX.Value - SelectedPoint.NominalX),
                ToleranceMin = SelectedPoint.ToleranceMin,
                ToleranceMax = SelectedPoint.ToleranceMax,
                Status = Math.Abs(SelectedPoint.MeasuredX.Value - SelectedPoint.NominalX) <= SelectedPoint.ToleranceMax ? 
                    Models.MeasurementStatus.Completed : Models.MeasurementStatus.Failed
            });
        }        if (SelectedPoint.MeasuredY.HasValue)
        {
            MeasurementResults.Add(new MeasurementResult 
            { 
                Name = "Y-Position",
                Nominal = new Vector3(0, (float)SelectedPoint.NominalY, 0),
                Actual = new Vector3(0, (float)SelectedPoint.MeasuredY.Value, 0),
                Deviation = Math.Abs(SelectedPoint.MeasuredY.Value - SelectedPoint.NominalY),
                ToleranceMin = SelectedPoint.ToleranceMin,
                ToleranceMax = SelectedPoint.ToleranceMax,
                Status = Math.Abs(SelectedPoint.MeasuredY.Value - SelectedPoint.NominalY) <= SelectedPoint.ToleranceMax ?
                    Models.MeasurementStatus.Completed : Models.MeasurementStatus.Failed
            });
        }        if (SelectedPoint.MeasuredZ.HasValue)
        {
            MeasurementResults.Add(new MeasurementResult 
            { 
                Name = "Z-Position",
                Nominal = new Vector3(0, 0, (float)SelectedPoint.NominalZ),
                Actual = new Vector3(0, 0, (float)SelectedPoint.MeasuredZ.Value),
                Deviation = Math.Abs(SelectedPoint.MeasuredZ.Value - SelectedPoint.NominalZ),
                ToleranceMin = SelectedPoint.ToleranceMin,
                ToleranceMax = SelectedPoint.ToleranceMax,
                Status = Math.Abs(SelectedPoint.MeasuredZ.Value - SelectedPoint.NominalZ) <= SelectedPoint.ToleranceMax ?
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

    public MainWindowViewModel(IMeasurementService measurementService, IMeasurementSimulationService simulationService, IReportingService reportingService)
    {
        _measurementService = measurementService ?? throw new ArgumentNullException(nameof(measurementService));
        _simulationService = simulationService ?? throw new ArgumentNullException(nameof(simulationService));
        _reportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        
        // Initialize commands
        LoadModelCommand = new AsyncRelayCommand(LoadModel);
        StartMeasurementCommand = new AsyncRelayCommand(StartMeasurementAsync);
        ShowDemoModelCommand = new RelayCommand(ShowDemoModel);
        MeasurePointCommand = new AsyncRelayCommand<MeasurementPoint>(SimulateMeasurementAsync);
        ExpandAllCommand = new RelayCommand(ExpandAll);
        CollapseAllCommand = new RelayCommand(CollapseAll);

        // Initialize visualization commands
        OpenStatisticsSummaryCommand = new RelayCommand(OpenStatisticsSummary);
        OpenPointDetailsCommand = new RelayCommand(OpenPointDetails);
        _reportingService = reportingService;

        LoadModelCommand = new AsyncRelayCommand(LoadModel);
        StartMeasurementCommand = new AsyncRelayCommand(StartMeasurementAsync);
        ExportReportCommand = new RelayCommand(() => ExportReport(ReportFormat.PDF));
        ShowDemoModelCommand = new RelayCommand(ShowDemoModel);
        MeasurePointCommand = new AsyncRelayCommand<MeasurementPoint>(MeasurePointAsync);
        ExpandAllCommand = new RelayCommand(ExpandAll);
        CollapseAllCommand = new RelayCommand(CollapseAll);
        
        OpenStatisticsSummaryCommand = new RelayCommand(OpenStatisticsSummary);
        OpenPointDetailsCommand = new RelayCommand(OpenPointDetails);
        OpenToleranceAnalysisCommand = new RelayCommand(OpenToleranceAnalysis);
        OpenSuccessFailureStatusCommand = new RelayCommand(OpenSuccessFailureStatus);
        OpenDeviationMeasurementsCommand = new RelayCommand(OpenDeviationMeasurements);
        OpenGraphsCommand = new RelayCommand(OpenGraphs);

        // Initialize export commands
        ExportPdfReportCommand = new RelayCommand(() => ExportReport(ReportFormat.PDF));
        ExportXmlDataCommand = new RelayCommand(() => ExportReport(ReportFormat.XML));
        ExportCsvDataCommand = new RelayCommand(() => ExportReport(ReportFormat.CSV));
        ExportHtmlReportCommand = new RelayCommand(() => ExportReport(ReportFormat.HTML));
        

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

    private void OpenStatisticsSummary() => 
        ShowVisualizationWindow("Statistics Summary", VisualizationType.Statistics);

    private void OpenPointDetails() => 
        ShowVisualizationWindow("Point Details", VisualizationType.PointDetails);

    private void OpenToleranceAnalysis() => 
        ShowVisualizationWindow("Tolerance Analysis", VisualizationType.Tolerance);

    private void OpenSuccessFailureStatus() => 
        ShowVisualizationWindow("Success/Failure Status", VisualizationType.Status);

    private void OpenDeviationMeasurements() => 
        ShowVisualizationWindow("Deviation Measurements", VisualizationType.Deviations);

    private void OpenGraphs() => 
        ShowVisualizationWindow("Measurement Graphs", VisualizationType.Graphs);

    private async void ExportReport(ReportFormat format)
    {
        if (Parts.Count == 0)
        {
            MessageBox.Show("No measurement data available to export.", "Export Report", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var options = new ReportOptions
        {
            IncludeStatistics = true,
            IncludeGraphs = format == ReportFormat.PDF || format == ReportFormat.HTML,
            IncludeIndividualPoints = true,
            Include3DVisualization = format == ReportFormat.PDF || format == ReportFormat.HTML
        };

        var extension = format switch
        {
            ReportFormat.PDF => ".pdf",
            ReportFormat.XML => ".xml",
            ReportFormat.CSV => ".csv",
            ReportFormat.HTML => ".html",
            _ => ".txt"
        };

        var fileType = format switch
        {
            ReportFormat.PDF => "PDF Report",
            ReportFormat.XML => "XML Data",
            ReportFormat.CSV => "CSV Data",
            ReportFormat.HTML => "HTML Report",
            _ => "Report"
        };

        // Open preview window first
        try
        {
            var previewWindow = new ReportPreviewWindow();
            var previewViewModel = new ReportPreviewViewModel(Parts, format, options);
            previewWindow.DataContext = previewViewModel;

            if (previewWindow.ShowDialog() == true)
            {
                await ExportReportAsync(Parts, extension, format, options);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error showing preview: {ex.Message}");
            MessageBox.Show("Failed to open report preview", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ExportReportAsync(IEnumerable<PartMeasurement> measurements, string extension, ReportFormat format, ReportOptions options)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Export Report",
            Filter = $"{format} Files (*{extension})|*{extension}",
            DefaultExt = extension,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var success = await _reportingService.ExportReportAsync(measurements, dialog.FileName, format, options);
                if (success)
                {
                    MessageBox.Show("Report exported successfully!", "Export Complete", 
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = dialog.FileName,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error opening exported report: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("Failed to export report.", "Export Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting report: {ex.Message}", "Export Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ShowVisualizationWindow(string title, VisualizationType type)
    {
        try
        {
            var window = new VisualizationWindow();
            var viewModel = new VisualizationViewModel(title, type, Parts);
            window.DataContext = viewModel;
            window.Show();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error showing visualization: {ex.Message}");
            MessageBox.Show("Failed to open visualization window", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

    // All 3D visualization is now handled in DemoModelWindow

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
            MeasurementResults.Clear(); // Clear previous results

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
                            SelectedPoint = point;
                            
                            // Simulate detailed measurement process
                            await SimulatePointMeasurementAsync(point);

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
                        }
                    }
                    else
                    {
                        // Point already measured, skip
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
            MeasurementStatus = $"Messe Punkt: {point.Name}";
            
            // Simulate detailed measurement process
            await SimulatePointMeasurementAsync(point);
            
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

    // Subscribe to changes in Parts collection
    partial void OnPartsChanged(ObservableCollection<PartMeasurement> value)
    {            if (value != null)
            {
                value.CollectionChanged += Parts_CollectionChanged;
                foreach (var part in value)
                {
                    SubscribeToPartEvents(part);
                }
            }
    }

    private void Parts_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (PartMeasurement part in e.OldItems)
            {
                UnsubscribeFromPartEvents(part);
            }
        }

        if (e.NewItems != null)
        {
            foreach (PartMeasurement part in e.NewItems)
            {
                SubscribeToPartEvents(part);
            }
        }
    }

    // All property change notifications now handled through ObservableObject and bindings
    private void SubscribeToPartEvents(PartMeasurement part)
    {
        part.Points.CollectionChanged += Points_CollectionChanged;
    }

    private void UnsubscribeFromPartEvents(PartMeasurement part)
    {
        part.Points.CollectionChanged -= Points_CollectionChanged;
    }

    private void Points_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Collection changes trigger UI updates through bindings
        OnPropertyChanged(nameof(Parts));
    }

    private async Task SimulatePointMeasurementAsync(MeasurementPoint point)
    {
        point.IsInProgress = true;
        point.MeasurementProgress = 0;

        // Simulate robot movement to point (30% of time)
        for (int i = 0; i <= 30; i += 2)
        {
            point.MeasurementProgress = i;
            await Task.Delay(_random.Next(50, 100)); // Random delay to simulate varying movement speed
        }

        // Simulate approach and touch (10% of time)
        for (int i = 30; i <= 40; i += 2)
        {
            point.MeasurementProgress = i;
            await Task.Delay(_random.Next(100, 150)); // Slower, more precise movement
        }

        // Simulate actual measurement process (40% of time)
        for (int i = 40; i <= 80; i += 2)
        {
            point.MeasurementProgress = i;
            await Task.Delay(_random.Next(75, 125)); // Regular measurement speed
        }

        // Simulate retraction and data processing (20% of time)
        for (int i = 80; i <= 100; i += 4)
        {
            point.MeasurementProgress = i;
            await Task.Delay(_random.Next(50, 75)); // Faster retraction movement
        }

        point.MeasurementProgress = 100;
        point.IsInProgress = false;
    }

    private void ShowDemoModel()
    {
        var window = new DemoModelWindow();
        window.Show();
    }


}
