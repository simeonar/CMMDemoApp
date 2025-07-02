using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using CMMDemoApp.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Colors;
using ScottPlot;

namespace CMMDemoApp.Services
{
    public class ReportingService : IReportingService
    {
        private readonly string _reportTemplatesPath;

        public ReportingService()
        {
            _reportTemplatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReportTemplates");
            Directory.CreateDirectory(_reportTemplatesPath);
        }

        public async Task<bool> ExportReportAsync(
            IEnumerable<PartMeasurement> measurements,
            string filePath,
            ReportFormat format,
            ReportOptions? options = null)
        {
            options ??= new ReportOptions();

            try
            {
                // Validate input parameters
                if (measurements == null || !measurements.Any())
                {
                    System.Diagnostics.Debug.WriteLine("Error: No measurement data provided for export");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    System.Diagnostics.Debug.WriteLine("Error: Invalid file path for export");
                    return false;
                }

                // Ensure the directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    try
                    {
                        Directory.CreateDirectory(directory);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error creating directory: {ex.Message}");
                        return false;
                    }
                }

                // Try to export based on format
                switch (format)
                {
                    case ReportFormat.XML:
                        await ExportXmlReportAsync(measurements, filePath, options);
                        break;
                    case ReportFormat.CSV:
                        await ExportCsvReportAsync(measurements, filePath, options);
                        break;
                    case ReportFormat.PDF:
                        await ExportPdfReportAsync(measurements, filePath, options);
                        break;
                    case ReportFormat.HTML:
                        await ExportHtmlReportAsync(measurements, filePath, options);
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine($"Error: Unsupported format: {format}");
                        return false;
                }

                // Verify the file was created
                if (!File.Exists(filePath))
                {
                    System.Diagnostics.Debug.WriteLine("Error: Export completed but file was not created");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log detailed error information
                System.Diagnostics.Debug.WriteLine($"Error exporting report:");
                System.Diagnostics.Debug.WriteLine($"Format: {format}");
                System.Diagnostics.Debug.WriteLine($"File path: {filePath}");
                System.Diagnostics.Debug.WriteLine($"Error message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public async Task<bool> ExportQuickReportAsync(
            PartMeasurement measurement,
            string filePath,
            ReportFormat format)
        {
            var options = new ReportOptions
            {
                IncludeStatistics = true,
                IncludeGraphs = false,
                IncludeIndividualPoints = true,
                Include3DVisualization = false
            };

            return await ExportReportAsync(new[] { measurement }, filePath, format, options);
        }

        public async Task<bool> ExportSelectedPointsReportAsync(
            IEnumerable<MeasurementPoint> points,
            string filePath,
            ReportFormat format)
        {
            // Создаем временный PartMeasurement для выбранных точек
            var measurement = new PartMeasurement
            {
                Name = "Selected Points Report",
                PartId = "SELECTED_" + DateTime.Now.ToString("yyyyMMddHHmmss")
            };

            foreach (var point in points)
            {
                measurement.Points.Add(point);
            }

            return await ExportQuickReportAsync(measurement, filePath, format);
        }

        public async Task<bool> ExportStatisticalReportAsync(
            IEnumerable<PartMeasurement> measurements,
            string filePath,
            ReportFormat format)
        {
            var options = new ReportOptions
            {
                IncludeStatistics = true,
                IncludeGraphs = true,
                IncludeIndividualPoints = false,
                Include3DVisualization = true
            };

            return await ExportReportAsync(measurements, filePath, format, options);
        }

        private async Task ExportXmlReportAsync(
            IEnumerable<PartMeasurement> measurements,
            string filePath,
            ReportOptions options)
        {
            var report = new XDocument(
                new XElement("MeasurementReport",
                    new XElement("GeneratedDate", DateTime.Now.ToString("s")),
                    new XElement("Statistics",
                        new XElement("TotalParts", measurements.Count()),
                        new XElement("TotalPoints", measurements.Sum(m => m.Points.Count)),
                        new XElement("CompletedPoints", measurements.Sum(m => 
                            m.Points.Count(p => p.Status == MeasurementStatus.Completed))),
                        new XElement("FailedPoints", measurements.Sum(m => 
                            m.Points.Count(p => p.Status == MeasurementStatus.Failed)))
                    ),
                    new XElement("Parts",
                        measurements.Select(m =>
                            new XElement("Part",
                                new XAttribute("id", m.PartId),
                                new XAttribute("name", m.Name),
                                new XElement("Points",
                                    m.Points.Select(p =>
                                        new XElement("Point",
                                            new XAttribute("id", p.PointId),
                                            new XAttribute("name", p.Name),
                                            new XAttribute("status", p.Status),
                                            new XElement("Nominal",
                                                new XElement("X", p.NominalX),
                                                new XElement("Y", p.NominalY),
                                                new XElement("Z", p.NominalZ)
                                            ),
                                            p.MeasuredX.HasValue && p.MeasuredY.HasValue && p.MeasuredZ.HasValue ? 
                                                new XElement("Measured",
                                                    new XElement("X", p.MeasuredX.Value),
                                                    new XElement("Y", p.MeasuredY.Value),
                                                    new XElement("Z", p.MeasuredZ.Value)
                                                ) : null,
                                            new XElement("Tolerance",
                                                new XElement("Min", p.ToleranceMin),
                                                new XElement("Max", p.ToleranceMax)
                                            ),
                                            p.Deviation.HasValue ?
                                                new XElement("Deviation", p.Deviation.Value) : null
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            );

            await File.WriteAllTextAsync(filePath, report.ToString());
        }

        private async Task ExportCsvReportAsync(
            IEnumerable<PartMeasurement> measurements,
            string filePath,
            ReportOptions options)
        {
            var lines = new List<string>
            {
                // Header
                "Part ID,Part Name,Point ID,Point Name,Status,Nominal X,Nominal Y,Nominal Z,Measured X,Measured Y,Measured Z,Deviation,Within Tolerance"
            };

            foreach (var part in measurements)
            {
                foreach (var point in part.Points)
                {
                    lines.Add($"{part.PartId},{part.Name},{point.PointId},{point.Name},{point.Status}," +
                            $"{point.NominalX},{point.NominalY},{point.NominalZ}," +
                            $"{point.MeasuredX ?? 0},{point.MeasuredY ?? 0},{point.MeasuredZ ?? 0}," +
                            $"{point.Deviation ?? 0},{point.IsWithinTolerance}");
                }
            }

            await File.WriteAllLinesAsync(filePath, lines);
        }

        // This method is marked async for interface consistency, but iText7 operations are synchronous
        private async Task ExportPdfReportAsync(
            IEnumerable<PartMeasurement> measurements,
            string filePath,
            ReportOptions options)
        {
            PdfWriter? writer = null;
            PdfDocument? pdf = null;
            Document? document = null;

            try
            {
                writer = new PdfWriter(filePath);
                pdf = new PdfDocument(writer);
                document = new Document(pdf);

                // Create and cache the title font
                var titleFont = PdfFontFactory.CreateRegisteredFont(StandardFonts.HELVETICA_BOLD);
                var regularFont = PdfFontFactory.CreateRegisteredFont(StandardFonts.HELVETICA);

                // Title
                document.Add(new Paragraph("Measurement Report")
                    .SetFontSize(20)
                    .SetFont(titleFont));

                document.Add(new Paragraph($"Generated: {DateTime.Now:g}")
                    .SetFontSize(10)
                    .SetFont(regularFont));

                // Summary Statistics
                document.Add(new Paragraph("Summary Statistics")
                    .SetFontSize(16)
                    .SetFont(titleFont));

                var totalPoints = measurements.Sum(m => m.Points.Count);
                var completedPoints = measurements.Sum(m => m.Points.Count(p => p.Status == MeasurementStatus.Completed));
                var failedPoints = measurements.Sum(m => m.Points.Count(p => p.Status == MeasurementStatus.Failed));

                var statsTable = new Table(2);
                // Add table borders and styling
                statsTable.SetWidth(UnitValue.CreatePercentValue(100));
                statsTable.SetMarginBottom(20);

                void AddStatsRow(string label, string value)
                {
                    statsTable.AddCell(new Cell().Add(new Paragraph(label).SetFont(titleFont)));
                    statsTable.AddCell(new Cell().Add(new Paragraph(value).SetFont(regularFont)));
                }

                AddStatsRow("Total Parts", measurements.Count().ToString());
                AddStatsRow("Total Points", totalPoints.ToString());
                AddStatsRow("Completed Points", completedPoints.ToString());
                AddStatsRow("Failed Points", failedPoints.ToString());
                AddStatsRow("Success Rate", $"{(totalPoints > 0 ? (double)completedPoints / totalPoints : 0):P2}");

                document.Add(statsTable);

                if (options.IncludeIndividualPoints)
                {
                    document.Add(new Paragraph("Detailed Results")
                        .SetFontSize(16)
                        .SetFont(titleFont)
                        .SetMarginTop(20));

                    foreach (var part in measurements)
                    {
                        document.Add(new Paragraph($"Part: {part.Name} (ID: {part.PartId})")
                            .SetFontSize(14)
                            .SetFont(titleFont)
                            .SetMarginTop(15));

                        var pointsTable = new Table(6);
                        pointsTable.SetWidth(UnitValue.CreatePercentValue(100));

                        // Add header cells with bold text
                        var headers = new[] { "Point", "Status", "Nominal", "Actual", "Deviation", "Tolerance" };
                        foreach (var header in headers)
                        {
                            pointsTable.AddHeaderCell(new Cell()
                                .Add(new Paragraph(header).SetFont(titleFont))
                                .SetBackgroundColor(ColorConstants.LIGHT_GRAY));
                        }

                        foreach (var point in part.Points)
                        {
                            // Add cell content with appropriate colors for status
                            var statusColor = point.Status switch
                            {
                                MeasurementStatus.Completed => ColorConstants.GREEN,
                                MeasurementStatus.Failed => ColorConstants.RED,
                                _ => ColorConstants.BLACK
                            };

                            pointsTable.AddCell(new Cell().Add(new Paragraph(point.Name).SetFont(regularFont)));
                            pointsTable.AddCell(new Cell().Add(new Paragraph(point.Status.ToString()).SetFont(regularFont).SetFontColor(statusColor)));
                            pointsTable.AddCell(new Cell().Add(new Paragraph($"({point.NominalX:F3}, {point.NominalY:F3}, {point.NominalZ:F3})").SetFont(regularFont)));
                            pointsTable.AddCell(new Cell().Add(new Paragraph(point.MeasuredX.HasValue ? 
                                $"({point.MeasuredX:F3}, {point.MeasuredY:F3}, {point.MeasuredZ:F3})" : "Not Measured").SetFont(regularFont)));
                            pointsTable.AddCell(new Cell().Add(new Paragraph(point.Deviation?.ToString("F3") ?? "-").SetFont(regularFont)));
                            pointsTable.AddCell(new Cell().Add(new Paragraph($"±{point.ToleranceMax:F3}").SetFont(regularFont)));
                        }

                        document.Add(pointsTable);
                    }
                }

                if (options.IncludeGraphs)
                {
                    // Future enhancement: Add graphs using ScottPlot
                    document.Add(new Paragraph("Graphical Analysis")
                        .SetFontSize(16)
                        .SetFont(titleFont)
                        .SetMarginTop(20));
                    document.Add(new Paragraph("(Graphs will be added in a future update)")
                        .SetFont(regularFont)
                        .SetFontColor(ColorConstants.GRAY));
                }

                // Ensure all changes are written
                document.Close();
                pdf.Close();
                writer.Close();

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PDF export: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Clean up in case of error
                document?.Close();
                pdf?.Close();
                writer?.Close();

                // Try to delete partially written file
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch 
                    {
                        // Ignore cleanup errors
                    }
                }

                throw; // Re-throw to be handled by caller
            }
        }

        private async Task ExportHtmlReportAsync(
            IEnumerable<PartMeasurement> measurements,
            string filePath,
            ReportOptions options)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Measurement Report</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .stats {{ margin: 20px 0; }}
        table {{ border-collapse: collapse; width: 100%; margin: 20px 0; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background-color: #f2f2f2; }}
        .passed {{ color: green; }}
        .failed {{ color: red; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>Measurement Report</h1>
        <p>Generated: {DateTime.Now:g}</p>
    </div>

    <div class='stats'>
        <h2>Summary Statistics</h2>
        <table>
            <tr><td>Total Parts</td><td>{measurements.Count()}</td></tr>
            <tr><td>Total Points</td><td>{measurements.Sum(m => m.Points.Count)}</td></tr>
            <tr><td>Completed Points</td><td>{measurements.Sum(m => m.Points.Count(p => p.Status == MeasurementStatus.Completed))}</td></tr>
            <tr><td>Failed Points</td><td>{measurements.Sum(m => m.Points.Count(p => p.Status == MeasurementStatus.Failed))}</td></tr>
        </table>
    </div>
";

            foreach (var part in measurements)
            {
                html += $@"
    <div class='part'>
        <h2>Part: {part.Name} (ID: {part.PartId})</h2>
        <table>
            <tr>
                <th>Point</th>
                <th>Status</th>
                <th>Nominal</th>
                <th>Actual</th>
                <th>Deviation</th>
                <th>Tolerance</th>
            </tr>
";

                foreach (var point in part.Points)
                {
                    var statusClass = point.Status == MeasurementStatus.Completed ? "passed" : 
                                    point.Status == MeasurementStatus.Failed ? "failed" : "";
                    html += $@"
            <tr>
                <td>{point.Name}</td>
                <td class='{statusClass}'>{point.Status}</td>
                <td>({point.NominalX:F3}, {point.NominalY:F3}, {point.NominalZ:F3})</td>
                <td>{(point.MeasuredX.HasValue ? $"({point.MeasuredX:F3}, {point.MeasuredY:F3}, {point.MeasuredZ:F3})" : "Not Measured")}</td>
                <td>{point.Deviation?.ToString("F3") ?? "-"}</td>
                <td>±{point.ToleranceMax:F3}</td>
            </tr>";
                }

                html += @"
        </table>
    </div>";
            }

            html += @"
</body>
</html>";

            await File.WriteAllTextAsync(filePath, html);
        }
    }
}
