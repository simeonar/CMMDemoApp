using CMMDemoApp.Views.Panels;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace CMMDemoApp.Services
{
    public class TabManager
    {
        public static TabControl CenterTabControl { get; set; }
        public static TabItem SplashTab { get; set; }

        public static void OpenModelTab(string localFilePath)
        {
            if (CenterTabControl == null)
            {
                MessageBox.Show("CenterTabControl ist nicht initialisiert. Setzen Sie ihn, bevor Sie OpenModelTab aufrufen.");
                return;
            }

            if (string.IsNullOrWhiteSpace(localFilePath) || !File.Exists(localFilePath))
                return;

            string fileName = Path.GetFileName(localFilePath);
            string modelUrl = $"https://models/{Uri.EscapeDataString(fileName)}";

            var viewer = new WebView(modelUrl);

            // Erstellen Sie ein neues TabItem
            var tab = new TabItem
            {
                Content = viewer
            };

            // Tab-Header: Text + Schließen-Schaltfläche
            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var headerText = new TextBlock
            {
                Text = fileName,
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var closeButton = new Button
            {
                Content = "×",
                Width = 16,
                Height = 16,
                Padding = new Thickness(0),
                Margin = new Thickness(5, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Background = null,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            closeButton.Click += (s, e) =>
            {
                CenterTabControl.Items.Remove(tab);

                if (CenterTabControl.Items.Count == 0 && SplashTab != null && !CenterTabControl.Items.Contains(SplashTab))
                    CenterTabControl.Items.Add(SplashTab);
            };

            headerPanel.Children.Add(headerText);
            headerPanel.Children.Add(closeButton);

            tab.Header = headerPanel;

            CenterTabControl.Items.Add(tab);
            CenterTabControl.SelectedItem = tab;

            if (CenterTabControl.Items.Contains(SplashTab))
                CenterTabControl.Items.Remove(SplashTab);
        }
    }
}
