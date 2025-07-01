using System;
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

        // Generate random deviation within tolerance range
        var deviation = (point.ToleranceMax - point.ToleranceMin) * (_random.NextDouble() - 0.5);
        
        // Create actual coordinates with deviation
        var actualX = point.NominalX + _random.NextDouble() * deviation;
        var actualY = point.NominalY + _random.NextDouble() * deviation;
        var actualZ = point.NominalZ + _random.NextDouble() * deviation;

        return new MeasurementResult
        {
            PointId = point.PointId,
            Name = point.Name,
            Nominal = $"({point.NominalX:F3}, {point.NominalY:F3}, {point.NominalZ:F3})",
            Actual = $"({actualX:F3}, {actualY:F3}, {actualZ:F3})",
            Deviation = $"{deviation:F3}",
            Status = Math.Abs(deviation) <= point.ToleranceMax ? "OK" : "Out of Tolerance"
        };
    }
}
