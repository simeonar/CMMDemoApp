using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CMMDemoApp.Models;

namespace CMMDemoApp.ViewModels
{
    public partial class ReportPreviewViewModel : ObservableObject
    {
        public event Action<bool>? RequestClose;

        private readonly IEnumerable<PartMeasurement> _measurements;
        private readonly ReportFormat _format;

        [ObservableProperty]
        private string _reportTitle = string.Empty;

        [ObservableProperty]
        private ReportOptions _options = new();

        [ObservableProperty]
        private object _previewContent = new TextBlock 
        { 
            Text = "Loading preview...",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        public IRelayCommand ExportCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public ReportPreviewViewModel(IEnumerable<PartMeasurement> measurements, ReportFormat format, ReportOptions options)
        {
            _measurements = measurements;
            _format = format;
            Options = options;

            ReportTitle = $"{format} Report Preview";
            ExportCommand = new RelayCommand(Export);
            CancelCommand = new RelayCommand(Cancel);

            UpdatePreview();
        }

        private void UpdatePreview()
        {
            // TODO: Generate preview based on format and options
            PreviewContent = new TextBlock 
            { 
                Text = "Report preview will be shown here",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private void Export()
        {
            RequestClose?.Invoke(true);
        }

        private void Cancel()
        {
            RequestClose?.Invoke(false);
        }
    }
}
