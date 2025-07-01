using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CMMDemoApp.Models
{
    public partial class PartMeasurement : ObservableObject
    {
        [ObservableProperty]
        private string _partId = string.Empty;

        [ObservableProperty]
        private string _name = string.Empty;

        private readonly ObservableCollection<MeasurementPoint> _points = new();
        public ObservableCollection<MeasurementPoint> Points
        {
            get => _points;
        }

        [ObservableProperty]
        private bool _isExpanded;

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private MeasurementStatus _overallStatus = MeasurementStatus.NotStarted;

        [ObservableProperty]
        private double _overallProgress;

        public PartMeasurement()
        {
            _points.CollectionChanged += (s, e) => UpdateOverallStatus();
        }

        private void UpdateOverallStatus()
        {
            if (Points.Count == 0)
            {
                OverallStatus = MeasurementStatus.NotStarted;
                OverallProgress = 0;
                return;
            }

            var completedCount = Points.Count(p => p.Status == MeasurementStatus.Completed);
            var failedCount = Points.Count(p => p.Status == MeasurementStatus.Failed);
            var inProgressCount = Points.Count(p => p.Status == MeasurementStatus.InProgress);

            if (inProgressCount > 0)
            {
                OverallStatus = MeasurementStatus.InProgress;
            }
            else if (failedCount > 0)
            {
                OverallStatus = MeasurementStatus.Failed;
            }
            else if (completedCount == Points.Count)
            {
                OverallStatus = MeasurementStatus.Completed;
            }
            else
            {
                OverallStatus = MeasurementStatus.NotStarted;
            }

            OverallProgress = (completedCount + failedCount) * 100.0 / Points.Count;
        }
    }

    public partial class PartMeasurement
    {
        partial void OnPointsChanged(ObservableCollection<MeasurementPoint> value);
    }
}
