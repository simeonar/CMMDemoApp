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
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Name,Sollwert,Istwert,Abweichung,Status");
            
            foreach (var result in results)
            {
                var status = Math.Abs(result.Deviation) <= 0.01 ? "OK" : "Außerhalb Toleranz";
                csv.AppendLine($"{result.Name},{result.Nominal:F3},{result.Actual:F3},{result.Deviation:F3},{status}");
            }
            
            await System.IO.File.WriteAllTextAsync(filePath, csv.ToString());
            return true;
        }
        catch
        {
            return false;
        }
    }
}
