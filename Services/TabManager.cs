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
                MessageBox.Show("CenterTabControl ist nicht initialisiert. Bitte setzen Sie ihn vor dem Aufruf von OpenModelTab.");
                return;
            }

            if (string.IsNullOrWhiteSpace(localFilePath) || !File.Exists(localFilePath))
                return;

            string fileName = Path.GetFileName(localFilePath);
            string modelUrl = $"https://models/{Uri.EscapeDataString(fileName)}";

            var viewer = new WebView(modelUrl);

            var tab = new TabItem
            {
                Header = fileName,
                Content = viewer
            };


            CenterTabControl.Items.Add(tab);
            CenterTabControl.SelectedItem = tab;

            if (CenterTabControl.Items.Contains(SplashTab))
                CenterTabControl.Items.Remove(SplashTab);
        }
    }
}
