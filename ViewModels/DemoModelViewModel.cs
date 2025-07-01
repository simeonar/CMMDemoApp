using System;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Diagnostics;

namespace CMMDemoApp.ViewModels
{
    /// <summary>
    /// ViewModel for the DemoModelWindow with 3D model display functionality
    /// </summary>
    public partial class DemoModelViewModel : ObservableObject
    {
        [ObservableProperty]
        private MeshGeometry3D? demoModelGeometry;

        [ObservableProperty]
        private Model3DGroup? measurementPointsModel;

        [ObservableProperty]
        private string modelInfo = "Demo Model: 60x30x15 mm rectangular block";

        /// <summary>
        /// Initializes a new instance of the DemoModelViewModel class
        /// </summary>
        public DemoModelViewModel()
        {
            Debug.WriteLine("[CMM] DemoModelViewModel constructor started");
            ResetViewCommand = new RelayCommand(ResetView);
            CloseWindowCommand = new RelayCommand(CloseWindow);
            CreateDemoModel();
            Debug.WriteLine("[CMM] DemoModelViewModel constructor completed");
        }

        /// <summary>
        /// Creates a demo 3D model for display
        /// </summary>
        private void CreateDemoModel()
        {
            Debug.WriteLine("[CMM] CreateDemoModel started");
            // Create demo cube with improved geometry
            var mesh = new MeshGeometry3D();
            
            // Cube vertices (8 corners)
            mesh.Positions.Add(new Point3D(-30, -15, -7.5)); // 0
            mesh.Positions.Add(new Point3D(30, -15, -7.5));  // 1
            mesh.Positions.Add(new Point3D(30, 15, -7.5));   // 2
            mesh.Positions.Add(new Point3D(-30, 15, -7.5));  // 3
            mesh.Positions.Add(new Point3D(-30, -15, 7.5));  // 4
            mesh.Positions.Add(new Point3D(30, -15, 7.5));   // 5
            mesh.Positions.Add(new Point3D(30, 15, 7.5));    // 6
            mesh.Positions.Add(new Point3D(-30, 15, 7.5));   // 7

            // Normals for better lighting
            var frontNormal = new Vector3D(0, 0, -1);
            var backNormal = new Vector3D(0, 0, 1);
            var leftNormal = new Vector3D(-1, 0, 0);
            var rightNormal = new Vector3D(1, 0, 0);
            var topNormal = new Vector3D(0, 1, 0);
            var bottomNormal = new Vector3D(0, -1, 0);

            // Extended vertices for each side (for correct normals)
            // Front side
            mesh.Positions.Add(new Point3D(-30, -15, -7.5)); mesh.Normals.Add(frontNormal); // 8
            mesh.Positions.Add(new Point3D(30, -15, -7.5));  mesh.Normals.Add(frontNormal); // 9
            mesh.Positions.Add(new Point3D(30, 15, -7.5));   mesh.Normals.Add(frontNormal); // 10
            mesh.Positions.Add(new Point3D(-30, 15, -7.5));  mesh.Normals.Add(frontNormal); // 11

            // Back side
            mesh.Positions.Add(new Point3D(30, -15, 7.5));   mesh.Normals.Add(backNormal); // 12
            mesh.Positions.Add(new Point3D(-30, -15, 7.5));  mesh.Normals.Add(backNormal); // 13
            mesh.Positions.Add(new Point3D(-30, 15, 7.5));   mesh.Normals.Add(backNormal); // 14
            mesh.Positions.Add(new Point3D(30, 15, 7.5));    mesh.Normals.Add(backNormal); // 15

            // Triangles with new indices
            // Front side
            mesh.TriangleIndices.Add(8); mesh.TriangleIndices.Add(9); mesh.TriangleIndices.Add(10);
            mesh.TriangleIndices.Add(8); mesh.TriangleIndices.Add(10); mesh.TriangleIndices.Add(11);
            
            // Back side
            mesh.TriangleIndices.Add(12); mesh.TriangleIndices.Add(13); mesh.TriangleIndices.Add(14);
            mesh.TriangleIndices.Add(12); mesh.TriangleIndices.Add(14); mesh.TriangleIndices.Add(15);
            
            // Use original vertices for other sides
            // Left side
            mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(4);
            
            // Right side
            mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(2);
            
            // Top side
            mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(7);
            
            // Bottom side
            mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(1);

            DemoModelGeometry = mesh;
            ModelInfo = "Demo Model: 60x30x15 mm rectangular block with improved rendering";
            
            Debug.WriteLine($"[CMM] CreateDemoModel completed, geometry created with {mesh.Positions.Count} positions and {mesh.TriangleIndices.Count / 3} triangles");
            Debug.WriteLine($"[CMM] DemoModelGeometry is now {(DemoModelGeometry != null ? "not null" : "null")}");
            Debug.WriteLine($"[CMM] DemoModelGeometry has freeze state: {DemoModelGeometry?.IsFrozen ?? false}");
            
            // Wenn DemoModelGeometry eingefroren ist, erstellen wir eine neue Kopie
            if (DemoModelGeometry?.IsFrozen == true)
            {
                Debug.WriteLine("[CMM] DemoModelGeometry is frozen, creating an unfrozen copy");
                var unfrozenMesh = new MeshGeometry3D();
                var frozenMesh = DemoModelGeometry;
                
                // Копируем данные
                foreach (var position in frozenMesh.Positions)
                    unfrozenMesh.Positions.Add(position);
                
                foreach (var normal in frozenMesh.Normals)
                    unfrozenMesh.Normals.Add(normal);
                
                foreach (var index in frozenMesh.TriangleIndices)
                    unfrozenMesh.TriangleIndices.Add(index);
                
                DemoModelGeometry = unfrozenMesh;
                Debug.WriteLine($"[CMM] New unfrozen DemoModelGeometry created with {unfrozenMesh.Positions.Count} positions");
            }
        }

        /// <summary>
        /// Resets the camera view to default position
        /// </summary>
        private void ResetView()
        {
            // This will be connected to the view to reset camera position
            CameraResetRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Closes the window containing this viewmodel
        /// </summary>
        private void CloseWindow()
        {
            // This will be handled by the view
            CloseWindowRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event that gets triggered when camera reset is requested
        /// </summary>
        public event EventHandler? CameraResetRequested;

        /// <summary>
        /// Event that gets triggered when window close is requested
        /// </summary>
        public event EventHandler? CloseWindowRequested;

        /// <summary>
        /// Command to reset the camera view
        /// </summary>
        public RelayCommand ResetViewCommand { get; }

        /// <summary>
        /// Command to close the window
        /// </summary>
        public RelayCommand CloseWindowCommand { get; }
    }
}
