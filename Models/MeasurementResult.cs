namespace CMMDemoApp.Models;

public class MeasurementResult
{
    public string Name { get; set; } = string.Empty;
    public double Nominal { get; set; }
    public double Actual { get; set; }
    public double Deviation => Actual - Nominal;
}
