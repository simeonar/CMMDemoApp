using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using System.Diagnostics;
using System.Linq;
using CMMDemoApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace CMMDemoApp.Views
{
    /// <summary>
    /// Interaction logic for DemoModelWindow.xaml
    /// </summary>
    /// <summary>
    /// Separate window for 3D visualization of measurement models and points.
    /// This is intentionally kept as a separate window to:
    /// 1. Maintain clear separation between primary measurement workflow (tree/table view) and 3D visualization
    /// 2. Allow users to work with measurements in the main window while keeping 3D view visible on another screen
    /// 3. Provide better performance by rendering 3D content independently from the main UI
    /// </summary>
    public partial class DemoModelWindow : Window
    {
        private DemoModelViewModel _viewModel = null!;
        private PerspectiveCamera? _sceneCamera;
        private Viewport3D? _demoViewport;
        private double _cameraDistance = 150;
        private double _cameraTheta = Math.PI / 4; // 45 degrees
        private double _cameraPhi = Math.PI / 6;   // 30 degrees
        private Point _lastMousePosition;
        private bool _isMouseCaptured = false;

        public DemoModelWindow()
        {
            try
            {
                InitializeComponent();
                
                // Initialisierung des ViewModels
                try
                {
                    var vmFromDi = Ioc.Default.GetService<DemoModelViewModel>();
                    if (vmFromDi == null)
                    {
                        Debug.WriteLine("[CMM] DemoModelViewModel not found in DI, creating new instance");
                        _viewModel = new DemoModelViewModel();
                    }
                    else
                    {
                        Debug.WriteLine("[CMM] DemoModelViewModel retrieved from DI");
                        _viewModel = vmFromDi;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CMM] Error getting ViewModel from DI: {ex.Message}");
                    _viewModel = new DemoModelViewModel();
                }
                
                DataContext = _viewModel;
                Debug.WriteLine("[CMM] DataContext set to DemoModelViewModel");
                
                this.Loaded += DemoModelWindow_Loaded;
                
                // Subscribe to view model events
                _viewModel.CameraResetRequested += (s, e) => ResetCamera();
                _viewModel.CloseWindowRequested += (s, e) => this.Close();
                
                Debug.WriteLine("[CMM] DemoModelWindow constructor completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CMM] Error in DemoModelWindow constructor: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Error initializing demo window: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DemoModelWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[CMM] DemoModelWindow loaded");
            
            // Find UI elements
            _sceneCamera = this.FindName("SceneCamera") as PerspectiveCamera;
            _demoViewport = this.FindName("DemoViewport") as Viewport3D;
            
            Debug.WriteLine($"[CMM] SceneCamera: {(_sceneCamera != null ? "found" : "not found")}");                Debug.WriteLine($"[CMM] DemoViewport: {(_demoViewport != null ? "found" : "not found")}");
                Debug.WriteLine($"[CMM] DemoModelGeometry: {(_viewModel.DemoModelGeometry != null ? "not null" : "null")}");
                
                if (_demoViewport != null)
                {
                    _demoViewport.MouseWheel += DemoViewport_MouseWheel;
                    _demoViewport.MouseLeftButtonDown += DemoViewport_MouseLeftButtonDown;
                    _demoViewport.MouseLeftButtonUp += DemoViewport_MouseLeftButtonUp;
                    _demoViewport.MouseMove += DemoViewport_MouseMove;
                    Debug.WriteLine("[CMM] Added mouse event handlers to viewport");
                    
                    // Überprüfen aller untergeordneten Elemente des Viewport3D
                    var modelVisuals = _demoViewport.Children.OfType<ModelVisual3D>().ToList();
                    Debug.WriteLine($"[CMM] Viewport contains {modelVisuals.Count} ModelVisual3D elements");
                    
                    // Container des Modells finden
                    var demoModelContainer = this.FindName("DemoModelContainer") as ModelVisual3D;
                    Debug.WriteLine($"[CMM] DemoModelContainer: {(demoModelContainer != null ? "found" : "not found")}");
                    
                    // DemoModelGeometry Bindung überprüfen
                    var demoModel = this.FindName("DemoModel") as GeometryModel3D;
                    if (demoModel != null)
                    {
                        Debug.WriteLine($"[CMM] DemoModel: found, geometry is {(demoModel.Geometry != null ? "not null" : "null")}");
                        
                        // Wenn die Geometrie nicht über Binding gesetzt wurde, direkt setzen
                        if (demoModel.Geometry == null && _viewModel.DemoModelGeometry != null)
                        {
                            Debug.WriteLine("[CMM] Setting DemoModel geometry directly");
                            demoModel.Geometry = _viewModel.DemoModelGeometry;
                        }
                        else if (demoModel.Geometry == null)
                        {
                            Debug.WriteLine("[CMM] Creating fallback geometry directly");
                            
                            // Wenn keine Geometrie im ViewModel vorhanden ist, einfache Geometrie erstellen
                            var mesh = new MeshGeometry3D();
                        
                        // Простой куб размером 60x30x15
                        mesh.Positions.Add(new Point3D(-30, -15, -7.5)); // 0
                        mesh.Positions.Add(new Point3D(30, -15, -7.5));  // 1
                        mesh.Positions.Add(new Point3D(30, 15, -7.5));   // 2
                        mesh.Positions.Add(new Point3D(-30, 15, -7.5));  // 3
                        mesh.Positions.Add(new Point3D(-30, -15, 7.5));  // 4
                        mesh.Positions.Add(new Point3D(30, -15, 7.5));   // 5
                        mesh.Positions.Add(new Point3D(30, 15, 7.5));    // 6
                        mesh.Positions.Add(new Point3D(-30, 15, 7.5));   // 7
                        
                        // Передняя грань
                        mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(2);
                        mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(3);
                        
                        // Задняя грань
                        mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(7);
                        mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(6);
                        
                        // Geometrie direkt setzen
                        demoModel.Geometry = mesh;
                        Debug.WriteLine("[CMM] Fallback geometry created and set directly");
                    }
                }
                else
                {
                    Debug.WriteLine("[CMM] DemoModel not found");
                }
            }
            else
            {
                Debug.WriteLine("[CMM] Error: Failed to find DemoViewport in loaded window");
            }
            
            InitializeCamera();
        }

        private void InitializeCamera()
        {
            if (_sceneCamera != null)
            {
                // Set initial camera parameters
                _cameraDistance = 200;
                _cameraTheta = Math.PI / 4; // 45 degrees
                _cameraPhi = Math.PI / 6;   // 30 degrees
                UpdateCameraPosition();
                Debug.WriteLine("[CMM] Camera initialized");
            }
            else
            {
                Debug.WriteLine("[CMM] Error: Failed to find SceneCamera in loaded window");
            }
        }

        private void ResetCamera()
        {
            _cameraDistance = 200;
            _cameraTheta = Math.PI / 4; // 45 degrees
            _cameraPhi = Math.PI / 6;   // 30 degrees
            UpdateCameraPosition();
        }

        private void UpdateCameraPosition()
        {
            if (_sceneCamera == null) 
            {
                Debug.WriteLine("[CMM] Error: SceneCamera is null in UpdateCameraPosition");
                return;
            }

            // Center of the object (cube 60x30x15)
            var target = new Point3D(0, 0, 0);
            
            double x = target.X + _cameraDistance * Math.Sin(_cameraTheta) * Math.Cos(_cameraPhi);
            double y = target.Y + _cameraDistance * Math.Sin(_cameraPhi);
            double z = target.Z + _cameraDistance * Math.Cos(_cameraTheta) * Math.Cos(_cameraPhi);

            _sceneCamera.Position = new Point3D(x, y, z);
            _sceneCamera.LookDirection = new Vector3D(target.X - x, target.Y - y, target.Z - z);
            _sceneCamera.UpDirection = new Vector3D(0, 1, 0);
        }
        
        #region Mouse Event Handlers
        
        private void DemoViewport_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_demoViewport == null) return;
            
            _lastMousePosition = e.GetPosition(_demoViewport);
            _isMouseCaptured = true;
            _demoViewport.CaptureMouse();
            e.Handled = true;
        }

        private void DemoViewport_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_demoViewport == null) return;
            
            _isMouseCaptured = false;
            _demoViewport.ReleaseMouseCapture();
            e.Handled = true;
        }

        private void DemoViewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMouseCaptured || _demoViewport == null) return;
            
            Point currentPosition = e.GetPosition(_demoViewport);
            double deltaX = currentPosition.X - _lastMousePosition.X;
            double deltaY = currentPosition.Y - _lastMousePosition.Y;

            // Adjust rotation speeds
            const double rotationSpeed = 0.01;
            
            // Update camera angles
            _cameraTheta -= deltaX * rotationSpeed;
            _cameraPhi += deltaY * rotationSpeed;
            
            // Clamp phi to avoid camera flipping
            _cameraPhi = Math.Max(-Math.PI / 2 + 0.1, Math.Min(Math.PI / 2 - 0.1, _cameraPhi));
            
            UpdateCameraPosition();
            _lastMousePosition = currentPosition;
            e.Handled = true;
        }

        private void DemoViewport_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Adjust zoom speed
            const double zoomSpeed = 10.0;
            
            // Update camera distance
            _cameraDistance -= e.Delta * zoomSpeed / 120.0;
            
            // Clamp distance to reasonable values
            _cameraDistance = Math.Max(50, Math.Min(500, _cameraDistance));
            
            UpdateCameraPosition();
            e.Handled = true;
        }
        
        private void Window_TargetUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            Debug.WriteLine($"[CMM] Target updated: {e.Property.Name} on {e.TargetObject.GetType().Name}");
            
            // Wenn die Geometrie aktualisiert wurde, überprüfen wir sie
            if (e.Property.Name == "Geometry" && e.TargetObject is GeometryModel3D)
            {
                var model = e.TargetObject as GeometryModel3D;
                Debug.WriteLine($"[CMM] Geometry binding updated. Current geometry: {(model?.Geometry != null ? "not null" : "null")}");
            }
        }
        
        #endregion
    }
}
