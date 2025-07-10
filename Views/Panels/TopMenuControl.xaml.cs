using iText.Layout.Element;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CMMDemoApp.Views.Panels
{
    /// <summary>
    /// Interaktionslogik für TopMenuControl.xaml
    /// </summary>
    public partial class TopMenuControl : UserControl
    {
        public event EventHandler DesignLoadRequested;

        public TopMenuControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty IsMenuCollapsedProperty =
    DependencyProperty.Register("IsMenuCollapsed", typeof(bool), typeof(TopMenuControl),
        new PropertyMetadata(false));

        public bool IsMenuCollapsed
        {
            get => (bool)GetValue(IsMenuCollapsedProperty);
            set => SetValue(IsMenuCollapsedProperty, value);
        }

        private void LoadDesignButton_Click(object sender, RoutedEventArgs e)
        {
            DesignLoadRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
