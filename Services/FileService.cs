using System.Text.Json;
using CMMDemoApp.Models;

namespace CMMDemoApp.Services;

/// <summary>
/// Service für Dateiverwaltung (Speichern/Laden von Programmen und Einstellungen)
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Messprogramm speichern
    /// </summary>
    Task<bool> SaveMeasurementProgramAsync(MeasurementProgram program, string filePath);
    
    /// <summary>
    /// Messprogramm laden
    /// </summary>
    Task<MeasurementProgram?> LoadMeasurementProgramAsync(string filePath);
    
    /// <summary>
    /// Einstellungen speichern
    /// </summary>
    Task<bool> SaveSettingsAsync(CMMSettings settings);
    
    /// <summary>
    /// Einstellungen laden
    /// </summary>
    Task<CMMSettings> LoadSettingsAsync();
    
    /// <summary>
    /// Messbericht als CSV exportieren
    /// </summary>
    Task<bool> ExportMeasurementResultsAsync(IEnumerable<MeasurementResult> results, string filePath);
}

public class FileService : IFileService
{
    private readonly string _settingsPath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CMMDemoApp",
        "settings.json");

    public async Task<bool> SaveMeasurementProgramAsync(MeasurementProgram program, string filePath)
    {
        try
        {
            var json = JsonSerializer.Serialize(program, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            await System.IO.File.WriteAllTextAsync(filePath, json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<MeasurementProgram?> LoadMeasurementProgramAsync(string filePath)
    {
        try
        {
            if (!System.IO.File.Exists(filePath))
                return null;

            var json = await System.IO.File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<MeasurementProgram>(json);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> SaveSettingsAsync(CMMSettings settings)
    {
        try
        {
            var directory = System.IO.Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            await System.IO.File.WriteAllTextAsync(_settingsPath, json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<CMMSettings> LoadSettingsAsync()
    {
        try
        {
            if (!System.IO.File.Exists(_settingsPath))
                return new CMMSettings();

            var json = await System.IO.File.ReadAllTextAsync(_settingsPath);
            return JsonSerializer.Deserialize<CMMSettings>(json) ?? new CMMSettings();
        }
        catch
        {
            return new CMMSettings();
        }
    }

    public async Task<bool> ExportMeasurementResultsAsync(IEnumerable<MeasurementResult> results, string filePath)
    {
        try
        {
            var lines = new List<string>
            {
                // Header
                "Point ID,Name,Nominal,Actual,Deviation,Status"
            };

            // Add measurement results
            foreach (var result in results)
            {
                lines.Add($"{result.PointId},{result.Name},{result.Nominal},{result.Actual},{result.Deviation},{result.Status}");
            }

            await System.IO.File.WriteAllLinesAsync(filePath, lines);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
