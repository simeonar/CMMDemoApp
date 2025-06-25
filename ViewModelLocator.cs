using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace CMMDemoApp;

public class ViewModelLocator
{
    public ViewModelLocator()
    {
        var services = new ServiceCollection();
        services.AddSingleton<MainWindowViewModel>();
        
        Ioc.Default.ConfigureServices(services.BuildServiceProvider());
    }

    public MainWindowViewModel Main => Ioc.Default.GetService<MainWindowViewModel>()!;
}
