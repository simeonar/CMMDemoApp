using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Windows.Controls;

namespace CMMDemoApp.Views.Panels
{
    /// <summary>
    /// Interaktionslogik für WebView.xaml
    /// </summary>
    public partial class WebView : UserControl
    {
        public WebView()
        {
            InitializeComponent();
            InitWebView();
        }

        private async void InitWebView()
        {
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.OpenDevToolsWindow();

            var editorPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Editor");
            if (!Directory.Exists(editorPath))
            {
                System.Windows.MessageBox.Show($"Der Ordner '{editorPath}' wurde nicht gefunden.");
                return;
            }

            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "app",
                editorPath,
                CoreWebView2HostResourceAccessKind.Allow);

            // Lädt index.html aus dem lokalen Ordner editor über den virtuellen Host
            webView.Source = new Uri("https://app/index.html");

        }
    }
}
