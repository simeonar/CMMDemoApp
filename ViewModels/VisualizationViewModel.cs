using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ScottPlot;
using ScottPlot.WPF;
using CommunityToolkit.Mvvm.ComponentModel;
using CMMDemoApp.Models;
using Orientation = System.Windows.Controls.Orientation;

namespace CMMDemoApp.ViewModels
{
    public partial class VisualizationViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _windowTitle;

        [ObservableProperty]
        private string _description;

        [ObservableProperty]
        private object _visualizationContent;

        public VisualizationViewModel(string title, VisualizationType type, IEnumerable<PartMeasurement> measurements)
        {
            WindowTitle = title;
            Description = GetDescription(type);
            VisualizationContent = CreateVisualizationContent(type, measurements);
        }

        private string GetDescription(VisualizationType type) => type switch
        {
            VisualizationType.Statistics => "Overview of all measurement statistics and analysis",
            VisualizationType.PointDetails => "Detailed view of individual measurement points",
            VisualizationType.Tolerance => "Analysis of tolerance zones and deviations",
            VisualizationType.Status => "Overview of measurement success and failure rates",
            VisualizationType.Deviations => "Analysis of measurement deviations and patterns",
            VisualizationType.Graphs => "Statistical graphs and visualizations",
            _ => string.Empty
        };

        private object CreateVisualizationContent(VisualizationType type, IEnumerable<PartMeasurement> measurements)
        {
            return type switch
            {
                VisualizationType.Statistics => CreateStatisticsView(measurements),
                VisualizationType.PointDetails => CreatePointDetailsView(measurements),
                VisualizationType.Tolerance => CreateToleranceView(measurements),
                VisualizationType.Status => CreateStatusView(measurements),
                VisualizationType.Deviations => CreateDeviationsView(measurements),
                VisualizationType.Graphs => CreateGraphsView(measurements),
                _ => new TextBlock { Text = "Unsupported visualization type" }
            };
        }

        private object CreateStatisticsView(IEnumerable<PartMeasurement> measurements)
        {
            var statsPanel = new StackPanel { Orientation = Orientation.Vertical };
            
            foreach (var part in measurements)
            {
                var completedPoints = part.Points.Count(p => p.Status == MeasurementStatus.Completed);
                var failedPoints = part.Points.Count(p => p.Status == MeasurementStatus.Failed);
                var withinTolerance = part.Points.Count(p => p.IsWithinTolerance);

                var partStats = new TextBlock
                {
                    Text = $"""
                        Part: {part.Name} ({part.PartId})
                        Total Points: {part.Points.Count}
                        Completed: {completedPoints}
                        Failed: {failedPoints}
                        Within Tolerance: {withinTolerance}
                        Completion: {part.OverallProgress:F1}%
                        """,
                    Margin = new Thickness(10),
                    TextWrapping = TextWrapping.Wrap
                };

                statsPanel.Children.Add(partStats);
                statsPanel.Children.Add(new Separator());
            }

            return new ScrollViewer { Content = statsPanel };
        }

        private object CreatePointDetailsView(IEnumerable<PartMeasurement> measurements)
        {
            var detailsPanel = new StackPanel { Orientation = Orientation.Vertical };
            
            foreach (var part in measurements)
            {
                foreach (var point in part.Points)
                {
                    var deviation = point.Deviation.HasValue ? $"{point.Deviation.Value:F3}" : "N/A";
                    var status = point.IsWithinTolerance ? "Within Tolerance" : "Out of Tolerance";

                    var pointDetails = new TextBlock
                    {
                        Text = $"""
                            Point: {point.Name} ({point.PointId})
                            Status: {point.Status} - {status}
                            Nominal: ({point.NominalX:F3}, {point.NominalY:F3}, {point.NominalZ:F3})
                            Measured: {(point.MeasuredX.HasValue ? $"({point.MeasuredX:F3}, {point.MeasuredY:F3}, {point.MeasuredZ:F3})" : "Not measured")}
                            Deviation: {deviation}
                            Tolerance: {point.ToleranceRange}
                            """,
                        Margin = new Thickness(10),
                        TextWrapping = TextWrapping.Wrap
                    };

                    detailsPanel.Children.Add(pointDetails);
                    detailsPanel.Children.Add(new Separator());
                }
            }

            return new ScrollViewer { Content = detailsPanel };
        }

        private object CreateToleranceView(IEnumerable<PartMeasurement> measurements)
        {
            var plot = new WpfPlot();
            
            var deviations = measurements.SelectMany(p => p.Points)
                .Where(p => p.Deviation.HasValue)
                .Select(p => p.Deviation!.Value)
                .ToArray();

            if (deviations.Length > 0)
            {
                // Create histogram manually since the API changed
                int binCount = 20;
                double binWidth = (0.5 - (-0.5)) / binCount;
                var counts = new double[binCount];
                var bins = new double[binCount];
                
                for (int i = 0; i < binCount; i++)
                {
                    bins[i] = -0.5 + i * binWidth;
                    counts[i] = deviations.Count(d => d >= bins[i] && d < (bins[i] + binWidth));
                }
                
                plot.Plot.Add.Bars(counts, bins);
                
                var vline1 = plot.Plot.Add.VerticalLine(0.1);
                vline1.LinePattern = LinePattern.Dotted;
                
                var vline2 = plot.Plot.Add.VerticalLine(-0.1);
                vline2.LinePattern = LinePattern.Dotted;
                
                plot.Plot.Title("Deviation Distribution");
                plot.Plot.XLabel("Deviation");
                plot.Plot.YLabel("Count");
                plot.Refresh();
            }

            return plot;
        }

        private object CreateStatusView(IEnumerable<PartMeasurement> measurements)
        {
            var plot = new WpfPlot();
            
            var total = measurements.SelectMany(p => p.Points).Count();
            var completed = measurements.SelectMany(p => p.Points).Count(p => p.Status == MeasurementStatus.Completed);
            var failed = measurements.SelectMany(p => p.Points).Count(p => p.Status == MeasurementStatus.Failed);
            var inProgress = measurements.SelectMany(p => p.Points).Count(p => p.Status == MeasurementStatus.InProgress);
            var notStarted = total - completed - failed - inProgress;

            var values = new[] { completed, failed, inProgress, notStarted };
            var labels = new[] { "Completed", "Failed", "In Progress", "Not Started" };
            var slices = new List<PieSlice>();

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > 0)
                {
                    slices.Add(new PieSlice { 
                        Value = values[i], 
                        Label = labels[i] + $" ({values[i]})" 
                    });
                }
            }

            plot.Plot.Add.Pie(slices);
            plot.Plot.Title("Measurement Status Distribution");
            plot.Refresh();

            return plot;
        }

        private object CreateDeviationsView(IEnumerable<PartMeasurement> measurements)
        {
            var plot = new WpfPlot();
            
            var points = measurements.SelectMany(p => p.Points)
                .Where(p => p.MeasuredX.HasValue && p.MeasuredY.HasValue && p.MeasuredZ.HasValue)
                .ToList();

            if (points.Any())
            {
                var nominalX = points.Select(p => p.NominalX).ToArray();
                var nominalY = points.Select(p => p.NominalY).ToArray();
                var measuredX = points.Select(p => p.MeasuredX!.Value).ToArray();
                var measuredY = points.Select(p => p.MeasuredY!.Value).ToArray();

                var s1 = plot.Plot.Add.Scatter(nominalX, nominalY);
                s1.LegendText = "Nominal";
                
                var s2 = plot.Plot.Add.Scatter(measuredX, measuredY);
                s2.LegendText = "Measured";
                
                plot.Plot.Title("Nominal vs Measured Points (X-Y Plane)");
                plot.Plot.XLabel("X Coordinate");
                plot.Plot.YLabel("Y Coordinate");
                plot.Plot.ShowLegend();
                plot.Refresh();
            }

            return plot;
        }

        private object CreateGraphsView(IEnumerable<PartMeasurement> measurements)
        {
            var tabControl = new TabControl();
            
            // Deviation over time
            var timeGraph = new WpfPlot();
            var points = measurements.SelectMany(p => p.Points)
                .Where(p => p.Deviation.HasValue)
                .Select((p, i) => (Index: i, Deviation: p.Deviation!.Value))
                .ToList();

            if (points.Any())
            {
                var scatter = timeGraph.Plot.Add.Scatter(
                    points.Select(p => (double)p.Index).ToArray(),
                    points.Select(p => p.Deviation).ToArray());
                    
                timeGraph.Plot.Title("Deviation Over Time");
                timeGraph.Plot.XLabel("Measurement Index");
                timeGraph.Plot.YLabel("Deviation");
                timeGraph.Refresh();
            }

            tabControl.Items.Add(new TabItem 
            { 
                Header = "Deviation Trend", 
                Content = timeGraph 
            });

            // 2D scatter with size representing Z
            var scatter2D = new WpfPlot();
            var measured3D = measurements.SelectMany(p => p.Points)
                .Where(p => p.MeasuredX.HasValue && p.MeasuredY.HasValue && p.MeasuredZ.HasValue)
                .ToList();

            if (measured3D.Any())
            {
                var measuredX = measured3D.Select(p => p.MeasuredX!.Value).ToArray();
                var measuredY = measured3D.Select(p => p.MeasuredY!.Value).ToArray();
                var measuredZ = measured3D.Select(p => p.MeasuredZ!.Value).ToArray();
                var sizes = measured3D.Select(p => Math.Abs(p.MeasuredZ!.Value) * 10 + 5).ToArray();

                var scatter = scatter2D.Plot.Add.Scatter(measuredX, measuredY);
                scatter.MarkerSize = 10;
                scatter2D.Plot.Title("Point Cloud (Z as marker size)");
                scatter2D.Plot.XLabel("X Coordinate");
                scatter2D.Plot.YLabel("Y Coordinate");
                scatter2D.Refresh();
            }

            tabControl.Items.Add(new TabItem 
            { 
                Header = "Point Cloud", 
                Content = scatter2D 
            });

            return tabControl;
        }
    }
}
