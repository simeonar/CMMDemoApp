using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using CMMDemoApp.ViewModels;

namespace CMMDemoApp;

public class ViewModelLocator
{
    public ViewModelLocator()
    {
        var services = new ServiceCollection();
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<DemoModelViewModel>();
        
        Ioc.Default.ConfigureServices(services.BuildServiceProvider());
    }

    public MainWindowViewModel Main => Ioc.Default.GetService<MainWindowViewModel>()!;
    public DemoModelViewModel DemoModel => Ioc.Default.GetService<DemoModelViewModel>()!;
}
