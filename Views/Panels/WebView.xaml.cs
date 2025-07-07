using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Windows.Controls;
using System.Windows;

namespace CMMDemoApp.Views.Panels
{
    /// <summary>
    /// Logik für WebView.xaml
    /// </summary>
    public partial class WebView : UserControl
    {
        private string _source;
        private bool _isInitialized = false;

        public WebView() : this(null)
        {
        }

        public string ModelPath
        {
            get => _source;
            set
            {
                _source = value;

                // ⚠️ Das Laden der Szene ist nur nach der Initialisierung möglich
                if (_isInitialized)
                    LoadModel();
            }
        }

        public WebView(string modelUrl = null)
        {
            InitializeComponent();
            _source = modelUrl;
            InitWebView();
        }

        private async void InitWebView()
        {
            try
            {
                await webView.EnsureCoreWebView2Async();

                // DevTools öffnen (im Produktionsmodus entfernen)
                webView.CoreWebView2.OpenDevToolsWindow();

                // Virtueller Host für index.html
                var editorPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Editor");
                if (!Directory.Exists(editorPath))
                {
                    MessageBox.Show($"Der Ordner '{editorPath}' wurde nicht gefunden.");
                    return;
                }

                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "app",
                    editorPath,
                    CoreWebView2HostResourceAccessKind.Allow);

                // Virtueller Host für Modelle
                var modelsPath = Path.Combine(editorPath, "models");
                if (!Directory.Exists(modelsPath))
                {
                    MessageBox.Show($"Der Modellordner '{modelsPath}' wurde nicht gefunden.");
                    return;
                }

                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "models",
                    modelsPath,
                    CoreWebView2HostResourceAccessKind.Allow);

                // 💡 Markieren, dass WebView initialisiert wurde
                _isInitialized = true;

                // 🚀 Szene nach vollständiger Bereitschaft laden
                LoadModel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"WebView2-Initialisierungsfehler: {ex.Message}");
            }
        }

        private void LoadModel()
        {
            if (webView?.CoreWebView2 == null)
                return;

            if (!string.IsNullOrEmpty(_source))
            {
                string encodedUrl = Uri.EscapeDataString(_source);
                string fullUrl = $"https://app/index.html?modelPath={encodedUrl}";
                webView.Source = new Uri(fullUrl);
            }
            else
            {
                webView.Source = new Uri("https://app/index.html");
            }
        }
    }
}