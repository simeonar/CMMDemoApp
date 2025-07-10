using CMMDemoApp.Services;
using CMMDemoApp.ViewModels;
using CMMDemoApp.Views.Panels;
using iText.Layout.Element;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace CMMDemoApp.Views
{
    /// <summary>
    /// Interaktionslogik für MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public bool IsLeftPanelVisible { get; set; } = true;
        public bool IsRightPanelVisible { get; set; } = true;
        public bool Is3DViewVisible { get; set; } = true;
        public bool IsWebViewVisible { get; set; } = true;

        public MainView()
        {
            InitializeComponent();
            TabManager.CenterTabControl = CenterTabControl;
            TabManager.SplashTab = SplashTab;
            PanelSelector.SelectedIndex = 0;
            this.DataContext = new MainViewModel();
            TopMenu.DesignLoadRequested += OnDesignLoadRequested;
        }

        private void OnDesignLoadRequested(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "3D-Modelle (*.glb;*.gltf)|*.glb;*.gltf",
                Title = "Wählen Sie ein 3D-Modell aus"
            };

            if (dialog.ShowDialog() == true)
            {
                string selectedPath = dialog.FileName;

                // Pfad an TabManager übergeben – dieser wandelt ihn in eine sichere URL um
                TabManager.OpenModelTab(selectedPath);
            }
        }


        private string _currentPanelTag = null;

        /// <summary>
        /// Wird aufgerufen, wenn ein Tab im seitlichen ListBox ausgewählt wird
        /// </summary>
        private void PanelSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PanelSelector.SelectedItem is ListBoxItem item)
            {
                string tag = item.Tag?.ToString();

                // Zeige die linke Spalte, wenn sie ausgeblendet war
                if (LeftContentColumn.Width.Value == 0)
                    LeftContentColumn.Width = new GridLength(250);

                // Oberes Fenster anschließen
                switch (tag)
                {
                    case "Features":
                        LeftUpperPanel.Content = new FeaturesPanel(); break;
                    case "Rules":
                        LeftUpperPanel.Content = new RulesPanel(); break;
                    case "Inspection":
                        LeftUpperPanel.Content = new InspectionPanel(); break;
                }

                // Unteres Panel – immer Property View
                if (LeftLowerPanel.Content == null)
                {
                    LeftLowerPanel.Content = new PropertiesPanel();
                }

                _currentPanelTag = tag;
            }
        }

        private void LeftPanelCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool isChecked = (sender as CheckBox)?.IsChecked ?? false;

            if (isChecked)
            {
                var item = PanelSelector.SelectedItem as ListBoxItem;
                if (item == null)
                {
                    // Aktivieren Sie die erste Registerkarte, wenn nichts ausgewählt ist
                    PanelSelector.SelectedIndex = 0;
                }
                else
                {
                    LeftContentColumn.Width = new GridLength(250);
                }
            }
            else
            {
                // Verstecke das Panel
                LeftContentColumn.Width = new GridLength(0);
                //PanelContent.Content = null;
                _currentPanelTag = null;
            }
        }

        private void MenuToggleButton_Click(object sender, RoutedEventArgs e)
        {
            MenuStateManager.IsMenuCollapsed = !MenuStateManager.IsMenuCollapsed;
            MenuToggleButton.Content = MenuStateManager.IsMenuCollapsed ? "▲" : "▼";
        }
    }
}
