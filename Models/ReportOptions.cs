namespace CMMDemoApp.Models
{
    public class ReportOptions
    {
        public bool IncludeStatistics { get; set; } = true;
        public bool IncludeGraphs { get; set; } = true;
        public bool IncludeIndividualPoints { get; set; } = true;
        public bool Include3DVisualization { get; set; } = false;
    }
}
