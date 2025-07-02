using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CMMDemoApp.Models
{
    public enum MeasurementStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Failed
    }

    public partial class MeasurementPoint : ObservableObject
    {
        [ObservableProperty]
        private string _pointId = string.Empty;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private double _nominalX;

        [ObservableProperty]
        private double _nominalY;

        [ObservableProperty]
        private double _nominalZ;

        [ObservableProperty]
        private double? _measuredX;

        [ObservableProperty]
        private double? _measuredY;

        [ObservableProperty]
        private double? _measuredZ;

        [ObservableProperty]
        private double _toleranceMin = -0.1;

        [ObservableProperty]
        private double _toleranceMax = 0.1;

        [ObservableProperty]
        private MeasurementStatus _status = MeasurementStatus.NotStarted;

        [ObservableProperty]
        private double _measurementProgress;

        [ObservableProperty]
        private bool _isExpanded;

        [ObservableProperty]
        private bool _isSelected;

        partial void OnMeasurementProgressChanged(double value)
        {
            // Update status based on progress
            Status = value switch
            {
                0 => MeasurementStatus.NotStarted,
                100 => MeasurementStatus.Completed,
                > 0 and < 100 => MeasurementStatus.InProgress,
                _ => Status
            };
        }

        [ObservableProperty]
        private bool _isInProgress;

        public double? Deviation
        {
            get
            {
                if (!MeasuredX.HasValue || !MeasuredY.HasValue || !MeasuredZ.HasValue)
                    return null;

                var dx = MeasuredX.Value - NominalX;
                var dy = MeasuredY.Value - NominalY;
                var dz = MeasuredZ.Value - NominalZ;

                return Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }
        }

        public string ToleranceRange => $"±{ToleranceMax:F3}";

        public bool IsWithinTolerance
        {
            get
            {
                var dev = Deviation;
                if (!dev.HasValue) return false;
                return dev.Value >= ToleranceMin && dev.Value <= ToleranceMax;
            }
        }
    }
}
