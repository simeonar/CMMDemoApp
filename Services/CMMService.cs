using System;
using System.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    private const double DefaultTolerance = 0.01; // ±10 micrometers

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

        // Move probe to measurement point
        await MoveProbeAsync(targetPosition);
        
        // Simulate measurement time
        await Task.Delay(50);
        
        // Simulate real measurement with small error
        var deviationX = (_random.NextDouble() - 0.5) * DefaultTolerance;
        var deviationY = (_random.NextDouble() - 0.5) * DefaultTolerance;
        var deviationZ = (_random.NextDouble() - 0.5) * DefaultTolerance;
        
        var actualPosition = targetPosition + new Vector3((float)deviationX, (float)deviationY, (float)deviationZ);
        var maxDeviation = Math.Max(Math.Abs((float)deviationX), Math.Max(Math.Abs((float)deviationY), Math.Abs((float)deviationZ)));
        
        return new MeasurementResult
        {
            Name = pointName,
            PointId = pointName,  // Using pointName as PointId for now
            Nominal = targetPosition,
            Actual = actualPosition,
            Deviation = maxDeviation,
            ToleranceMin = -DefaultTolerance,
            ToleranceMax = DefaultTolerance,
            Status = maxDeviation <= DefaultTolerance ? MeasurementStatus.Completed : MeasurementStatus.Failed
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
        if (_isCalibrated)
            return true;

        // Simulation der Tastkopfkalibrierung
        await Task.Delay(2000); // Simulation der Kalibrierungszeit
        _isCalibrated = true;
        _currentProbePosition = Vector3.Zero; // Nach der Kalibrierung zur Home-Position zurücksetzen
        
        return true;
    }
}
