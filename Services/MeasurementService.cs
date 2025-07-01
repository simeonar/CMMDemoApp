using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using CMMDemoApp.Models;
using System.Xml;

namespace CMMDemoApp.Services
{
    public interface IMeasurementService
    {
        Task<PartMeasurement> LoadExpectedDataAsync(string filePath);
        Task<bool> SaveMeasurementResultsAsync(PartMeasurement measurement, string filePath);
        Task SimulateMeasurementAsync(MeasurementPoint point);
    }

    public class MeasurementService : IMeasurementService
    {
        private readonly Random _random = new Random();

        public async Task<PartMeasurement> LoadExpectedDataAsync(string filePath)
        {
            try
            {
                var xmlContent = await File.ReadAllTextAsync(filePath);
                var doc = XDocument.Parse(xmlContent);
                var root = doc.Root;

                if (root == null)
                {
                    throw new ApplicationException("Invalid XML structure: Missing root element.");
                }

                var measurement = new PartMeasurement
                {
                    PartId = root.Element("PartId")?.Value ?? string.Empty,
                    Name = root.Element("Name")?.Value ?? string.Empty
                };

                var pointsElement = root.Element("Points");
                if (pointsElement != null)
                {
                    foreach (var pointElement in pointsElement.Elements("Point"))
                    {
                        var tolerance = double.TryParse(pointElement.Element("Tolerance")?.Value, out var t) ? t : 0.01;
                        var point = new MeasurementPoint
                        {
                            PointId = pointElement.Attribute("id")?.Value ?? string.Empty,
                            Name = pointElement.Attribute("name")?.Value ?? string.Empty,
                            NominalX = double.TryParse(pointElement.Element("X")?.Value, out var x) ? x : 0,
                            NominalY = double.TryParse(pointElement.Element("Y")?.Value, out var y) ? y : 0,
                            NominalZ = double.TryParse(pointElement.Element("Z")?.Value, out var z) ? z : 0,
                            ToleranceMin = -tolerance,
                            ToleranceMax = tolerance
                        };
                        measurement.Points.Add(point);
                    }
                }

                return measurement;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to load measurement data", ex);
            }
        }

        public async Task<bool> SaveMeasurementResultsAsync(PartMeasurement measurement, string filePath)
        {
            try
            {
                var doc = new XDocument(
                    new XElement("MeasurementResults",
                        new XElement("PartId", measurement.PartId),
                        new XElement("Name", measurement.Name),
                        new XElement("Points",
                            measurement.Points.Select(point =>
                                new XElement("Point",
                                    new XAttribute("id", point.PointId),
                                    new XAttribute("name", point.Name),
                                    new XElement("X", point.NominalX),
                                    new XElement("Y", point.NominalY),
                                    new XElement("Z", point.NominalZ),
                                    new XElement("Tolerance", point.ToleranceMax) // Save the positive tolerance value
                                )
                            )
                        )
                    )
                );

                using (var writer = XmlWriter.Create(filePath, new XmlWriterSettings { Async = true, Indent = true }))
                {
                    doc.Save(writer);
                    await writer.FlushAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to save measurement results", ex);
            }
        }

        public async Task SimulateMeasurementAsync(MeasurementPoint point)
        {
            // Simulate measurement process
            point.Status = MeasurementStatus.InProgress;
            
            await Task.Delay(1000); // Simulate measurement time
            
            // Generate measured values with some random deviation
            double maxDeviation = point.ToleranceMax;
            double deviation = maxDeviation * (_random.NextDouble() * 2 - 1); // Random deviation within tolerance range
            
            point.MeasuredX = point.NominalX + deviation;
            point.MeasuredY = point.NominalY + deviation;
            point.MeasuredZ = point.NominalZ + deviation;
            
            point.Status = point.IsWithinTolerance ? MeasurementStatus.Completed : MeasurementStatus.Failed;
        }
    }
}
