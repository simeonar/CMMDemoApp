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
        private string _id = string.Empty;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private double _expectedX;

        [ObservableProperty]
        private double _expectedY;

        [ObservableProperty]
        private double _expectedZ;

        [ObservableProperty]
        private double? _measuredX;

        [ObservableProperty]
        private double? _measuredY;

        [ObservableProperty]
        private double? _measuredZ;

        [ObservableProperty]
        private double _tolerance = 0.01;

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
