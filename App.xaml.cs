using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using CMMDemoApp.ViewModels;
using CMMDemoApp.Services;
using CMMDemoApp.Helpers;
using System.Diagnostics;

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
        try
        {
            base.OnStartup(e);

            // Initialize RenderOptions for better performance
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;

            Debug.WriteLine("[App] Starting application...");

            // Show the main window
            var mainWindow = Ioc.Default.GetRequiredService<MainWindow>();
            mainWindow.Show();

            Debug.WriteLine("[App] Main window created and shown");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[App] Error during startup: {ex.Message}");
            Debug.WriteLine($"[App] Stack trace: {ex.StackTrace}");
            MessageBox.Show($"Application startup error: {ex.Message}", "Startup Error", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
            Current.Shutdown();
        }
    }

    private void ConfigureServices()
    {
        try
        {
            Debug.WriteLine("[App] Configuring services...");
            // Setup dependency injection
            var services = new ServiceCollection()
                .AddSingleton<IMeasurementService, MeasurementService>()
                .AddSingleton<IMeasurementSimulationService, MeasurementSimulationService>()
                .AddSingleton<IReportingService, ReportingService>()
                .AddSingleton<MainWindowViewModel>()
                .AddSingleton<ThemeSelectorViewModel>()
                .AddSingleton<MainWindow>();

            var serviceProvider = services.BuildServiceProvider();
            Ioc.Default.ConfigureServices(serviceProvider);
            Debug.WriteLine("[App] Services configured successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[App] Error configuring services: {ex.Message}");
            Debug.WriteLine($"[App] Stack trace: {ex.StackTrace}");
            throw; // Re-throw to be caught in OnStartup
        }
    }
}
