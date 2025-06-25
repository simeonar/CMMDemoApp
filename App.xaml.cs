using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace CMMDemoApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;
    }
}

