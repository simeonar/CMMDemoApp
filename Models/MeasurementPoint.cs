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
        private bool _isExpanded;

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private double _measurementProgress;

        [ObservableProperty]
        private bool _isInProgress;

        public bool IsWithinTolerance
        {
            get
            {
                if (!MeasuredX.HasValue || !MeasuredY.HasValue || !MeasuredZ.HasValue)
                    return false;

                var deltaX = Math.Abs(ExpectedX - MeasuredX.Value);
                var deltaY = Math.Abs(ExpectedY - MeasuredY.Value);
                var deltaZ = Math.Abs(ExpectedZ - MeasuredZ.Value);

                return deltaX <= Tolerance && deltaY <= Tolerance && deltaZ <= Tolerance;
            }
        }
    }
}
