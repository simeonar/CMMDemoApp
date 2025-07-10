using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMMDemoApp.Services
{
    public static class MenuStateManager
    {
        private static bool _isMenuCollapsed;
        public static bool IsMenuCollapsed
        {
            get => _isMenuCollapsed;
            set
            {
                if (_isMenuCollapsed != value)
                {
                    _isMenuCollapsed = value;
                    PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsMenuCollapsed)));
                }
            }
        }

        public static event PropertyChangedEventHandler PropertyChanged;
    }
}
