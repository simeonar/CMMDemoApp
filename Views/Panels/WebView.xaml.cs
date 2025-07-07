using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Windows.Controls;
using System.Windows;

namespace CMMDemoApp.Views.Panels
{
    /// <summary>
    /// Логика для WebView.xaml
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

                // ⚠️ Загружать сцену можно только после инициализации
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

                // Открываем DevTools (удалить при продакшне)
                webView.CoreWebView2.OpenDevToolsWindow();

                // Виртуальный хост для index.html
                var editorPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Editor");
                if (!Directory.Exists(editorPath))
                {
                    MessageBox.Show($"Папка '{editorPath}' не найдена.");
                    return;
                }

                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "app",
                    editorPath,
                    CoreWebView2HostResourceAccessKind.Allow);

                // Виртуальный хост для моделей
                var modelsPath = Path.Combine(editorPath, "models");
                if (!Directory.Exists(modelsPath))
                {
                    MessageBox.Show($"Папка моделей '{modelsPath}' не найдена.");
                    return;
                }

                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "models",
                    modelsPath,
                    CoreWebView2HostResourceAccessKind.Allow);

                // 💡 Отметим, что WebView инициализирован
                _isInitialized = true;

                // 🚀 Загружаем сцену после полной готовности
                LoadModel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации WebView2: {ex.Message}");
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
