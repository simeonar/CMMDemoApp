using System.Windows;
using System;
using System.Windows.Media.Media3D;
using System.Windows.Input;
using System.Windows.Controls;
using System.ComponentModel;
using System.Diagnostics;
using CMMDemoApp.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using CMMDemoApp.Models;

namespace CMMDemoApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// <summary>
    /// Main window of the CMM application that provides the primary measurement workflow interface.
    /// The main window intentionally uses a tree/table-based visualization as the primary interface because:
    /// 1. It provides a clear, hierarchical view of parts and measurement points
    /// 2. Allows for efficient navigation and management of large numbers of measurement points
    /// 3. Provides immediate access to measurement data and status
    /// 4. Separates the workflow management from the 3D visualization (available in a separate window)
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? viewModel;
        private bool isMouseDown = false;
        private Point lastMousePosition;
        private PerspectiveCamera? camera;
        private double cameraDistance = 150;
        private double cameraTheta = Math.PI / 4; // 45 degrees
        private double cameraPhi = Math.PI / 6;   // 30 degrees

        private ModelVisual3D? measurementPointsContainer;
        private PerspectiveCamera? sceneCamera;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                viewModel = Ioc.Default.GetService<MainWindowViewModel>();
                DataContext = viewModel;

                this.Loaded += MainWindow_Loaded;

                // Subscribe to property changes to update 3D content
                if (viewModel != null)
                {
                    viewModel.PropertyChanged += ViewModel_PropertyChanged;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Initialisieren: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindowViewModel.MeasurementPointsModel))
            {
                UpdateMeasurementPoints();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Suche Viewport3D und initialisiere camera
            var viewport = FindChild<Viewport3D>(this);
            if (viewport != null)
            {
                sceneCamera = viewport.Camera as PerspectiveCamera;
                camera = sceneCamera;
                
                Debug.WriteLine("[CMM] Viewport3D gefunden, erstelle MeasurementPointsContainer neu");
                
                // Statt bestehenden Container zu suchen, erstellen wir einen neuen
                measurementPointsContainer = new ModelVisual3D();
                viewport.Children.Add(measurementPointsContainer); // Container direkt hinzufügen
                
                Debug.WriteLine("[CMM] Neuer MeasurementPointsContainer erstellt und hinzugefügt");
            }
            
            InitializeCamera();
            
            // Очистка MeasurementPointsModel при старте
            if (viewModel != null)
            {
                viewModel.ClearMeasurementPoints();
            }

            Debug.WriteLine("[CMM] MeasurementPointsModel очищен при старте");

            // Messpunkte anzeigen, falls vorhanden
            UpdateMeasurementPoints();
            
            // Debug-Information über Container
            Debug.WriteLine($"[CMM] MeasurementPointsContainer valid: {measurementPointsContainer != null}");
        }

        // Hilfsmethode für VisualTree-Suche
        private T? FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;
                var childOfChild = FindChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void InitializeCamera()
        {
            // Verwende die benannte Kamera aus XAML
            if (camera != null)
            {
                // Setze die anfänglichen Kameraparameter
                cameraDistance = 200; // Vergrößere die Entfernung
                cameraTheta = Math.PI / 4; // 45 degrees
                cameraPhi = Math.PI / 6;   // 30 degrees
                UpdateCameraPosition();
            }
        }

        private void UpdateCameraPosition()
        {
            if (camera == null) return;

            // Zentrum des Objekts (Würfel 60x30x15)
            var target = new Point3D(0, 0, 0);
            
            double x = target.X + cameraDistance * Math.Sin(cameraTheta) * Math.Cos(cameraPhi);
            double y = target.Y + cameraDistance * Math.Sin(cameraPhi);
            double z = target.Z + cameraDistance * Math.Cos(cameraTheta) * Math.Cos(cameraPhi);

            camera.Position = new Point3D(x, y, z);
            camera.LookDirection = new Vector3D(target.X - x, target.Y - y, target.Z - z);
            camera.UpDirection = new Vector3D(0, 1, 0);
        }

        private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isMouseDown = true;
                lastMousePosition = e.GetPosition(sender as IInputElement);
                ((FrameworkElement)sender).CaptureMouse();
            }
        }

        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(sender as IInputElement);
                double deltaX = currentPosition.X - lastMousePosition.X;
                double deltaY = currentPosition.Y - lastMousePosition.Y;

                // Rotate around Y axis (horizontal mouse movement)
                cameraTheta += deltaX * 0.01;
                
                // Rotate around X axis (vertical mouse movement)
                cameraPhi += deltaY * 0.01;
                
                // Limit vertical rotation
                cameraPhi = Math.Max(-Math.PI / 2 + 0.1, Math.Min(Math.PI / 2 - 0.1, cameraPhi));

                UpdateCameraPosition();
                lastMousePosition = currentPosition;
            }
        }

        private void Viewport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isMouseDown)
            {
                isMouseDown = false;
                ((FrameworkElement)sender).ReleaseMouseCapture();
            }
        }

        private void Viewport_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Zoom in/out
            cameraDistance -= e.Delta * 0.1;
            cameraDistance = Math.Max(50, Math.Min(500, cameraDistance));
            UpdateCameraPosition();
        }

        private void UpdateMeasurementPoints()
        {
            if (viewModel?.MeasurementPointsModel != null && measurementPointsContainer != null)
            {
                measurementPointsContainer.Children.Clear();
                int count = 0;
                foreach (var model in viewModel.MeasurementPointsModel.Children)
                {
                    var visual = new ModelVisual3D { Content = model };
                    measurementPointsContainer.Children.Add(visual);
                    count++;
                }
                Debug.WriteLine($"[CMM] UpdateMeasurementPoints: {count} Punkte hinzugefügt.");
            }
            else
            {
                Debug.WriteLine("[CMM] UpdateMeasurementPoints: ViewModel oder Container ist null.");
            }
        }

        private void LoadMeasurementPoints_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                Title = "Select Measurement Points File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string fileContent = File.ReadAllText(openFileDialog.FileName);
                    var measurementPoints = JsonConvert.DeserializeObject<List<MeasurementPoint>>(fileContent);

                    if (viewModel != null && measurementPoints != null)
                    {
                        viewModel.ClearMeasurementPoints();
                        viewModel.AddMeasurementPoints(measurementPoints);
                        UpdateMeasurementPoints(); // Trigger 3D update
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading or processing file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}