using System.Windows;
using BT_COMMONS.Database;
using BT_COMMONS.Helpers;
using BT_POS.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BT_POS;

public partial class App : Application
{

    public static IHost? AppHost { get; private set; }

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<APIAccess>(x => new APIAccess("https://127.0.0.1:5001/api/"));

                services.AddSingleton<MainWindow>();
                services.AddViewFactory<POSLogin>();
                services.AddViewFactory<POSHome>();

                services.AddSingleton<POSController>();
            }).Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost!.StartAsync();
        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost!.StopAsync();

        base.OnExit(e);
    }

    public static void LoginComplete()
    {
        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        var home = AppHost.Services.GetRequiredService<POSHome>();
        mainWindow.LoginComplete(home);
    }
}
