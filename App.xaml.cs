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
    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);
            
            // Initialize RenderOptions
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;
            
            // Initialize default theme
            ThemeManager.ApplyTheme("Modern Fluent");
            
            // Initialize IoC container and services
            ConfigureServices();
            
            // Create and show main window
            var mainWindow = new MainWindow();
            var viewModel = Ioc.Default.GetService<MainWindowViewModel>();
            if (viewModel == null)
            {
                MessageBox.Show("Failed to initialize application services.", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
                return;
            }
            mainWindow.DataContext = viewModel;
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Application initialization error: {ex.Message}\n\n{ex.StackTrace}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }
    }
    
    private void ConfigureServices()
    {
        var services = new ServiceCollection();
        
        // Register Services
        services.AddSingleton<IMeasurementService, MeasurementService>();
        services.AddSingleton<IMeasurementSimulationService, MeasurementSimulationService>();
        services.AddSingleton<IReportingService, ReportingService>();
        
        // Register ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<DemoModelViewModel>();
        
        // Build service provider and configure IoC container
        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions 
        { 
            ValidateOnBuild = true,
            ValidateScopes = true 
        });
        
        Ioc.Default.ConfigureServices(serviceProvider);
    }
}
