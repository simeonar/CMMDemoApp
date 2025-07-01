using System.Numerics;
using CMMDemoApp.Models;

namespace CMMDemoApp.Services;

/// <summary>
/// Service zur Simulation einer Koordinatenmessmaschine
/// </summary>
public interface ICMMService
{
    /// <summary>
    /// Einen Punkt an den angegebenen Koordinaten messen
    /// </summary>
    Task<MeasurementResult> MeasurePointAsync(string pointName, Vector3 targetPosition);
    
    /// <summary>
    /// Tastkopf zu Position bewegen
    /// </summary>
    Task<bool> MoveProbeAsync(Vector3 position);
    
    /// <summary>
    /// Aktuelle Tastkopfposition abrufen
    /// </summary>
    Vector3 GetCurrentProbePosition();
    
    /// <summary>
    /// Automatisches Messprogramm starten
    /// </summary>
    Task<List<MeasurementResult>> RunMeasurementProgramAsync(IEnumerable<Vector3> measurementPoints);
    
    /// <summary>
    /// Tastkopf kalibrieren
    /// </summary>
    Task<bool> CalibrateProbeAsync();
}

public class CMMService : ICMMService
{
    private Vector3 _currentProbePosition = Vector3.Zero;
    private readonly Random _random = new();
    private bool _isCalibrated = false;

    public Vector3 GetCurrentProbePosition() => _currentProbePosition;

    public async Task<bool> MoveProbeAsync(Vector3 position)
    {
        // Simulation der Tastkopfbewegung
        await Task.Delay(100); // Simulation der Bewegungszeit
        
        _currentProbePosition = position;
        return true;
    }

    public async Task<MeasurementResult> MeasurePointAsync(string pointName, Vector3 targetPosition)
    {
        if (!_isCalibrated)
        {
            throw new InvalidOperationException("Tastkopf ist nicht kalibriert!");
        }

        // Tastkopf zum Messpunkt bewegen
        await MoveProbeAsync(targetPosition);
        
        // Simulation der Messzeit
        await Task.Delay(50);
        
        // Simulation einer realen Messung mit kleinem Fehler
        var measurementError = (_random.NextDouble() - 0.5) * 0.02; // ±10 Mikrometer
        var nominalValue = targetPosition.Length();
        var actualValue = nominalValue + measurementError;
        var deviation = actualValue - nominalValue;
        
        return new MeasurementResult
        {
            Name = pointName,
            PointId = pointName,  // Using pointName as PointId for now
            Nominal = nominalValue.ToString("F3"),
            Actual = actualValue.ToString("F3"),
            Deviation = deviation.ToString("F3"),
            Status = Math.Abs(deviation) <= 0.01 ? "OK" : "NOK"  // Using 0.01 as default tolerance
        };
    }

    public async Task<List<MeasurementResult>> RunMeasurementProgramAsync(IEnumerable<Vector3> measurementPoints)
    {
        var results = new List<MeasurementResult>();
        int pointIndex = 1;
        
        foreach (var point in measurementPoints)
        {
            var result = await MeasurePointAsync($"Punkt {pointIndex}", point);
            results.Add(result);
            pointIndex++;
        }
        
        return results;
    }

    public async Task<bool> CalibrateProbeAsync()
    {
        // Simulation der Tastkopfkalibrierung
        await Task.Delay(2000);
        _isCalibrated = true;
        return true;
    }
}
