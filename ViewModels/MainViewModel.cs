using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMMDemoApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private bool _isLeftPanelVisible = true;
        public bool IsLeftPanelVisible
        {
            get => _isLeftPanelVisible;
            set { _isLeftPanelVisible = value; OnPropertyChanged(nameof(IsLeftPanelVisible)); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
