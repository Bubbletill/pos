﻿using System.Diagnostics;
using System.Windows;
using BT_COMMONS;
using BT_COMMONS.Database;
using BT_COMMONS.Helpers;
using BT_POS.Data;
using BT_POS.Views;
using Microsoft.Extensions.Configuration;
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
                IConfigurationBuilder builder = new ConfigurationBuilder();
                builder.AddJsonFile("AppSettings.json");
                IConfiguration config = builder.Build();

                services.AddSingleton<IConfiguration>(provider => config);

                services.AddSingleton<DatabaseAccess>(x => new DatabaseAccess(config["LocalConnectionString"]));
                services.AddSingleton<IAPIAccess, PAPIAccess>();

                services.AddSingleton<MainWindow>();
                services.AddViewFactory<POSLogin>();
                services.AddViewFactory<POSHome>();

                services.AddSingleton<POSController>();
            }).Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost!.StartAsync();

        var apiAccess =AppHost.Services.GetRequiredService<IAPIAccess>();
        var config = AppHost.Services.GetRequiredService<IConfiguration>();
        Trace.WriteLine(config["ControllerApiUrl"]);
        apiAccess.UpdateWithUrl(config["ControllerApiUrl"]);

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
