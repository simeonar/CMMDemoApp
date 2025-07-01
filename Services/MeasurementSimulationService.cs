using System;
using System.Numerics;
using System.Threading.Tasks;
using CMMDemoApp.Models;

namespace CMMDemoApp.Services;

/// <summary>
/// Service for simulating CMM measurements in demo mode
/// </summary>
public interface IMeasurementSimulationService
{
    /// <summary>
    /// Simulates a measurement for a given point
    /// </summary>
    /// <param name="point">The point to measure</param>
    /// <returns>Simulated measurement result</returns>
    Task<MeasurementResult> SimulateMeasurementAsync(MeasurementPoint point);
}

public class MeasurementSimulationService : IMeasurementSimulationService
{
    private readonly Random _random = new();

    public async Task<MeasurementResult> SimulateMeasurementAsync(MeasurementPoint point)
    {
        // Simulate measurement delay
        await Task.Delay(_random.Next(500, 2000));

        // Create nominal vector
        var nominal = new Vector3((float)point.NominalX, (float)point.NominalY, (float)point.NominalZ);

        // Generate random deviations within tolerance range
        var deviationX = (float)((point.ToleranceMax - point.ToleranceMin) * (_random.NextDouble() - 0.5));
        var deviationY = (float)((point.ToleranceMax - point.ToleranceMin) * (_random.NextDouble() - 0.5));
        var deviationZ = (float)((point.ToleranceMax - point.ToleranceMin) * (_random.NextDouble() - 0.5));
        
        // Create deviation vector and actual measurement vector
        var deviationVector = new Vector3(deviationX, deviationY, deviationZ);
        var actual = nominal + deviationVector;

        // Calculate maximum absolute deviation
        var maxDeviation = Math.Max(Math.Abs(deviationX), Math.Max(Math.Abs(deviationY), Math.Abs(deviationZ)));

        var result = new MeasurementResult
        {
            PointId = point.PointId,
            Name = point.Name,
            Nominal = nominal,
            Actual = actual,
            Deviation = maxDeviation,
            ToleranceMin = point.ToleranceMin,
            ToleranceMax = point.ToleranceMax,
            Status = maxDeviation <= point.ToleranceMax && maxDeviation >= point.ToleranceMin ? 
                MeasurementStatus.Completed : MeasurementStatus.Failed
        };

        return result;
    }
}
