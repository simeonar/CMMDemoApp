using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using CMMDemoApp.ViewModels;
using CMMDemoApp.Services;
using CMMDemoApp.Helpers;

namespace CMMDemoApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public App()
    {
        // Initialize IoC container and services
        ConfigureServices();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize RenderOptions for better performance
        RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;

        // Apply the default theme at startup
        ThemeManager.ApplyTheme("Modern Fluent");

        // Show the main window
        var mainWindow = Ioc.Default.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices()
    {
        // Setup dependency injection
        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                .AddSingleton<IMeasurementService, MeasurementService>()
                .AddSingleton<IMeasurementSimulationService, MeasurementSimulationService>()
                .AddSingleton<IReportingService, ReportingService>()
                .AddSingleton<MainWindowViewModel>()
                .AddSingleton<ThemeSelectorViewModel>()
                .AddSingleton<MainWindow>()
                .BuildServiceProvider()
        );
    }
}
