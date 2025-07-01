using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using CMMDemoApp.ViewModels;

namespace CMMDemoApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Initialisierung von RenderOptions
        RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;
        
        // Initialisierung des IoC-Containers
        ConfigureServices();
    }
    
    private void ConfigureServices()
    {
        var services = new ServiceCollection();
        
        // Registrierung der ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<DemoModelViewModel>();
        
        // Konfiguration des IoC-Containers
        Ioc.Default.ConfigureServices(services.BuildServiceProvider());
    }
}
