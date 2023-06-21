using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using BT_COMMONS;
using BT_COMMONS.Database;
using BT_COMMONS.DataRepositories;
using BT_COMMONS.Helpers;
using BT_POS.Buttons.Menu;
using BT_POS.RepositoryImpl;
using BT_POS.Views;
using BT_POS.Views.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BT_POS;

public partial class App : Application
{
    public static IHost? AppHost { get; private set; }

    public static List<POSMenuButton> HomeButtons;
    public static List<POSMenuButton> HomeTransButtons;

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                IConfigurationBuilder builder = new ConfigurationBuilder();
                builder.AddJsonFile("AppSettings.json");
                IConfiguration config = builder.Build();

                services.AddSingleton<IConfiguration>(provider => config);

                services.AddSingleton<DatabaseAccess>(x => new DatabaseAccess(config["LocalConnectionString"]));
                services.AddSingleton<APIAccess>(x => new APIAccess(config["ControllerApiUrl"]));
                services.AddSingleton<IOperatorRepository, OperatorRepository>();
                services.AddSingleton<ITransactionRepository, TransactionRepository>();
                services.AddSingleton<IStockRepository, StockRepository>();
                services.AddSingleton<IButtonRepository, ButtonRepository>();

                services.AddSingleton<MainWindow>();
                services.AddViewFactory<POSLogin>();

                services.AddViewFactory<POSHome>();

                services.AddViewFactory<POSAdmin>();

                services.AddSingleton<POSController>();
            }).Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost!.StartAsync();

        var btnRepo = AppHost.Services.GetRequiredService<IButtonRepository>();
        HomeButtons = await btnRepo.GetHomeButtons();
        HomeTransButtons = await btnRepo.GetHomeTransButtons();

        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost!.StopAsync();

        base.OnExit(e);
    }

    public static void SetAPIToken(string token)
    {
        APIAccess api = AppHost.Services.GetRequiredService<APIAccess>();
        api.UpdateWithToken(token);
    }

    public static void LoginComplete()
    {
        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        var home = AppHost.Services.GetRequiredService<POSHome>();
        mainWindow.LoginComplete(home);
    }
}
