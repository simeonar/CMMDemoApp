using System.Windows;
using System;

using System.Windows.Input;
using System.Windows.Controls;
using System.ComponentModel;
using System.Diagnostics;
using CMMDemoApp.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                viewModel = Ioc.Default.GetService<MainWindowViewModel>();
                if (viewModel == null)
                {
                    throw new InvalidOperationException("Failed to resolve MainWindowViewModel from dependency injection.");
                }
                
                DataContext = viewModel;
                this.Loaded += MainWindow_Loaded;
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                
                // Ensure cleanup when window closes
                this.Closed += (s, e) => 
                {
                    if (viewModel != null)
                    {
                        viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                        this.Loaded -= MainWindow_Loaded;
                    }
                };
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
            // We can use this for other property changes if needed
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (viewModel != null)
            {
                Debug.WriteLine("[CMM] MainWindow loaded");
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
                        // Update measurement points through ViewModel's collection
                        viewModel.Parts.Clear();
                        var part = new PartMeasurement { Name = "Imported Points" };
                        foreach (var point in measurementPoints)
                        {
                            part.Points.Add(point);
                        }
                        viewModel.Parts.Add(part);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading or processing file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is MeasurementPoint point && viewModel != null)
            {
                viewModel.SelectedPoint = point;
            }
        }
    }
}