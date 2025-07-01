namespace CMMDemoApp.Models;

/// <summary>
/// Einstellungen der Koordinatenmessmaschine
/// </summary>
public class CMMSettings
{
    /// <summary>
    /// Messgeschwindigkeit in mm/min
    /// </summary>
    public double MeasuringSpeed { get; set; } = 100.0;
    
    /// <summary>
    /// Antastkraft in Newton
    /// </summary>
    public double ProbingForce { get; set; } = 0.1;
    
    /// <summary>
    /// Toleranz für Messungen in mm
    /// </summary>
    public double MeasurementTolerance { get; set; } = 0.01;
    
    /// <summary>
    /// Automatische Kalibrierung aktiviert
    /// </summary>
    public bool AutoCalibrationEnabled { get; set; } = true;
    
    /// <summary>
    /// Anzahl der Messpunkte für Durchschnitt
    /// </summary>
    public int AveragingPoints { get; set; } = 3;
    
    /// <summary>
    /// Sicherheitsabstand in mm
    /// </summary>
    public double SafetyDistance { get; set; } = 5.0;
}

/// <summary>
/// Messprogramm-Definition
/// </summary>
public class MeasurementProgram
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Models.MeasurementPoint> Points { get; set; } = new();
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public string Author { get; set; } = string.Empty;
}

/// <summary>
/// Art der Messung
/// </summary>
public enum MeasurementType
{
    Point,      // Punktmessung
    Diameter,   // Durchmessermessung
    Plane,      // Ebenenmessung
    Circle,     // Kreismessung
    Cylinder,   // Zylindermessung
    Sphere      // Kugelmessung
}
