using CommunityToolkit.Mvvm.ComponentModel;

namespace CMMDemoApp.Models;

public partial class MeasurementResult : ObservableObject
{
    [ObservableProperty]
    private string _pointId = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _nominal = string.Empty;

    [ObservableProperty]
    private string _actual = string.Empty;

    [ObservableProperty]
    private string _deviation = string.Empty;

    [ObservableProperty]
    private string _status = string.Empty;

    public bool IsWithinTolerance => Status == "OK";
}
