using CommunityToolkit.Mvvm.ComponentModel;
using System.Numerics;

namespace CMMDemoApp.Models;

public partial class MeasurementResult : ObservableObject
{
    [ObservableProperty]
    private string _pointId = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private Vector3 _nominal;

    [ObservableProperty]
    private Vector3 _actual;

    [ObservableProperty]
    private double _deviation;

    [ObservableProperty]
    private double _toleranceMin = -0.1;

    [ObservableProperty]
    private double _toleranceMax = 0.1;

    [ObservableProperty]
    private MeasurementStatus _status = MeasurementStatus.NotStarted;

    public string FormattedNominal => $"({Nominal.X:F3}, {Nominal.Y:F3}, {Nominal.Z:F3})";
    public string FormattedActual => $"({Actual.X:F3}, {Actual.Y:F3}, {Actual.Z:F3})";
    public string FormattedDeviation => $"{Deviation:F3}";

    public bool IsWithinTolerance => Deviation >= ToleranceMin && Deviation <= ToleranceMax;
}
