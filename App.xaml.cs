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
        
        // Инициализация RenderOptions
        RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;
        
        // Инициализация IoC-контейнера
        ConfigureServices();
    }
    
    private void ConfigureServices()
    {
        var services = new ServiceCollection();
        
        // Регистрация ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<DemoModelViewModel>();
        
        // Настройка IoC-контейнера
        Ioc.Default.ConfigureServices(services.BuildServiceProvider());
    }
}
