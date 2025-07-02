using System.Collections.Generic;
using System.Threading.Tasks;
using CMMDemoApp.Models;

namespace CMMDemoApp.Services
{
    public interface IReportingService
    {
        /// <summary>
        /// Экспортирует отчет об измерениях в выбранном формате
        /// </summary>
        Task<bool> ExportReportAsync(
            IEnumerable<PartMeasurement> measurements,
            string filePath,
            ReportFormat format,
            ReportOptions? options = null);

        /// <summary>
        /// Генерирует быстрый отчет по одной детали
        /// </summary>
        Task<bool> ExportQuickReportAsync(
            PartMeasurement measurement,
            string filePath,
            ReportFormat format);

        /// <summary>
        /// Создает отчет по выбранным точкам
        /// </summary>
        Task<bool> ExportSelectedPointsReportAsync(
            IEnumerable<MeasurementPoint> points,
            string filePath,
            ReportFormat format);

        /// <summary>
        /// Создает статистический отчет по серии измерений
        /// </summary>
        Task<bool> ExportStatisticalReportAsync(
            IEnumerable<PartMeasurement> measurements,
            string filePath,
            ReportFormat format);
    }
}
