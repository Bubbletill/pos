﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using BT_COMMONS;
using BT_COMMONS.App;
using BT_COMMONS.Database;
using BT_COMMONS.DataRepositories;
using BT_COMMONS.Helpers;
using BT_COMMONS.Transactions;
using BT_POS.Buttons.Menu;
using BT_POS.RepositoryImpl;
using BT_POS.Views;
using BT_POS.Views.Admin;
using BT_POS.Views.Tender;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

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
                services.AddViewFactory<POSTenderHome>();

                services.AddViewFactory<POSAdmin>();

                services.AddSingleton<POSController>();
            }).Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost!.StartAsync();

        var controller = AppHost.Services.GetRequiredService<POSController>();

        // Load data.json
        try
        {
            using (StreamReader r = new StreamReader("C:\\bubbletill\\data.json"))
            {
                string json = r.ReadToEnd();
                AppConfig config = JsonConvert.DeserializeObject<AppConfig>(json);

                if (config == null || config.Register == null || config.Store == null)
                {
                    throw new Exception();
                }

                controller.StoreNumber = (int)config!.Store!;
                controller.RegisterNumber = (int)config!.Store!;
            }
        } 
        catch (Exception ex)
        {
            MessageBox.Show("Failed to load data.json", "Bubbletill POS", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None);
            Shutdown();
            return;
        }

        // Load hard totals
        try
        {
            using (StreamReader r = new StreamReader("C:\\bubbletill\\hardtotals.json"))
            {
                string json = r.ReadToEnd();
                HardTotals totals = JsonConvert.DeserializeObject<HardTotals>(json);

                if (totals == null || totals.Tender == null || totals.Type == null)
                {
                    throw new Exception();
                }

                controller.TenderHardTotals = totals.Tender;
                controller.TypeHardTotals = totals.Type;
            }
        }
        catch (Exception ex)
        {
            HardTotals ht = new HardTotals();
            ht.Tender = new Dictionary<TransactionTender, float>();
            ht.Type = new Dictionary<TransactionType, float>();
            foreach (TransactionTender tender in Enum.GetValues(typeof(TransactionTender)))
            {
                ht.Tender.Add(tender, 0);
            }

            foreach (TransactionType type in Enum.GetValues(typeof(TransactionType)))
            {
                ht.Type.Add(type, 0);
            }

            controller.TenderHardTotals = ht.Tender;
            controller.TypeHardTotals = ht.Type;

            string json = JsonConvert.SerializeObject(ht);
            File.WriteAllText("C:\\bubbletill\\hardtotals.json", json);
        }

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
