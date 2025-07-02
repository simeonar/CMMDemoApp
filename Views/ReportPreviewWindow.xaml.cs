using System.Collections.Generic;
using System.Windows;
using CMMDemoApp.Models;
using CMMDemoApp.ViewModels;

namespace CMMDemoApp.Views
{
    public partial class ReportPreviewWindow : Window
    {
        public ReportPreviewWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(System.EventArgs e)
        {
            base.OnSourceInitialized(e);
            if (DataContext is ReportPreviewViewModel viewModel)
            {
                viewModel.RequestClose += (result) =>
                {
                    DialogResult = result;
                    Close();
                };
            }
        }
    }
}
