using CMMDemoApp.ViewModels;
using CMMDemoApp.Views.Panels;
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

        public MainView()
        {
            InitializeComponent();
            PanelSelector.SelectedIndex = 0;
            this.DataContext = new MainViewModel();
        }

        private string _currentPanelTag = null;

        /// <summary>
        /// Вызывается при выборе вкладки в боковом ListBox
        /// </summary>
        private void PanelSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PanelSelector.SelectedItem is ListBoxItem item)
            {
                string tag = item.Tag?.ToString();

                // Показать левую колонку, если она была скрыта
                if (LeftContentColumn.Width.Value == 0)
                    LeftContentColumn.Width = new GridLength(250);

                // Подключить верхнее окно
                switch (tag)
                {
                    case "Features":
                        LeftUpperPanel.Content = new FeaturesPanel(); break;
                    case "Rules":
                        LeftUpperPanel.Content = new RulesPanel(); break;
                    case "Inspection":
                        LeftUpperPanel.Content = new InspectionPanel(); break;
                }

                // Нижняя панель — всегда Property View
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
                    // Активируем первую вкладку, если ничего не выбрано
                    PanelSelector.SelectedIndex = 0;
                }
                else
                {
                    LeftContentColumn.Width = new GridLength(250);
                }
            }
            else
            {
                // Скрываем панель
                LeftContentColumn.Width = new GridLength(0);
                //PanelContent.Content = null;
                _currentPanelTag = null;
            }
        }
    }
}
