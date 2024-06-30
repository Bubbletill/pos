using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using BT_COMMONS;
using BT_COMMONS.App;
using HardTotals = BT_COMMONS.App.HardTotals;
using BT_COMMONS.Database;
using BT_COMMONS.DataRepositories;
using BT_COMMONS.Helpers;
using BT_COMMONS.Operators;
using BT_COMMONS.Transactions;
using BT_POS.Buttons;
using BT_POS.Buttons.Admin;
using BT_POS.Buttons.Admin.CashMngt;
using BT_POS.Buttons.ItemMod;
using BT_POS.Buttons.Menu;
using BT_POS.Buttons.TransMod;
using BT_POS.RepositoryImpl;
using BT_POS.Splash;
using BT_POS.Views;
using BT_POS.Views.Admin;
using BT_POS.Views.Dialogues;
using BT_POS.Views.Menus;
using BT_POS.Views.Tender;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Windows.Devices.PointOfService;

namespace BT_POS;

public partial class App : Application
{
    public static IHost? AppHost { get; private set; }

    public static List<TransactionTender> TenderTypes;

    public static List<HomeButton> HomeButtons;
    public static List<HomeButton> HomeTransButtons;
    public static List<ItemModButton> ItemModButtons;
    public static List<TransModButton> TransModButtons;
    public static List<AdminButton> AdminButtons;
    public static List<CashManagementButton> AdminCashManagementButtons;

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                IConfigurationBuilder builder = new ConfigurationBuilder();
                builder.AddJsonFile("AppSettings.json");
                IConfiguration config = builder.Build();

                services.AddSingleton<IConfiguration>(provider => config);

                services.AddSingleton<DatabaseAccess>(x => new DatabaseAccess(config["LocalConnectionString"], config["ControllerConnectionString"]));
                services.AddSingleton<APIAccess>(x => new APIAccess(config["ControllerApiUrl"]));
                services.AddSingleton<IOperatorRepository, OperatorRepository>();
                services.AddSingleton<ITransactionRepository, TransactionRepository>();
                services.AddSingleton<IStockRepository, StockRepository>();
                services.AddSingleton<IButtonRepository, ButtonRepository>();
                services.AddSingleton<ISuspendRepository, SuspendRepository>();

                services.AddSingleton<MainWindow>();
                services.AddViewFactory<LoginView>();
                services.AddViewFactory<RegClosedView>();
                services.AddViewFactory<BasketOnlyView>();

                services.AddViewFactory<HomeView>();
                services.AddViewFactory<TenderHomeView>();
                services.AddViewFactory<ResumeView>();

                services.AddViewFactory<ItemModMenuView>();
                services.AddViewFactory<TransModMenuView>();

                services.AddViewFactory<AdminMenuView>();
                services.AddViewFactory<AdminCashManagementMenuView>();

                services.AddSingleton<POSController>();
            }).Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            POSSplashScreen splash = new POSSplashScreen();
            splash.Show();

            await AppHost!.StartAsync();

            var controller = AppHost.Services.GetRequiredService<POSController>();

            // Load data.json
            try
            {
                using (StreamReader r = new StreamReader("C:\\bubbletill\\data.json"))
                {
                    string json = r.ReadToEnd();
                    AppConfig config = JsonConvert.DeserializeObject<AppConfig>(json);

                    if (config == null || config.Register == null || config.Store == null || config.RegisterOpen == null)
                    {
                        throw new Exception();
                    }

                    controller.StoreNumber = (int)config.Store;
                    controller.RegisterNumber = (int)config.Register;
                    controller.RegisterOpen = (bool)config.RegisterOpen;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bubbletill failed to launch:\nFailed to load data.json", "Bubbletill POS", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None);
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

            var operRepo = AppHost.Services.GetRequiredService<IOperatorRepository>();
            var operGroups = await operRepo.GetOperatorGroups();
            foreach (var group in operGroups)
            {
                group.Parse();
                controller.OperatorGroups.Add(group.Id, group);
            }

            var btnRepo = AppHost.Services.GetRequiredService<IButtonRepository>();

            TenderTypes = await btnRepo.GetTenderTypes();
            if (TenderTypes == null || TenderTypes.Count == 0)
            {
                MessageBox.Show("Bubbletill failed to launch:\nThere are no tenders configured.", "Bubbletill POS", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None);
                Shutdown();
                return;
            }

            HomeButtons = await btnRepo.GetHomeButtons();
            HomeTransButtons = await btnRepo.GetHomeTransButtons();
            ItemModButtons = await btnRepo.GetItemModButtons();
            TransModButtons = await btnRepo.GetTransModButtons();
            AdminButtons = await btnRepo.GetAdminButtons();
            AdminCashManagementButtons = await btnRepo.GetAdminCashManagementButtons();

            // TODO: load pos peripherals


            // Load required services
            if (TenderTypes!.Contains(TransactionTender.WORLDPAY_CARD))
            {
                ProcessStartInfo processInfo;
                Process process;

                processInfo = new ProcessStartInfo("cmd.exe", "/c " + "C:\\YESEFT\\StartPOSServer.bat");
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                process = Process.Start(processInfo);
                process.WaitForExit();

                int exitCode = process.ExitCode;
                if (exitCode != 0)
                {
                    MessageBox.Show("Tender Worldpay Card failed to initalise (code " + exitCode + "). This option has been removed.", "Bubbletill POS", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                    TenderTypes.Remove(TransactionTender.WORLDPAY_CARD);
                }
            }

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
            splash.Close();

            base.OnStartup(e);
        } catch (Exception ex)
        {
            MessageBox.Show("Bubbletill failed to launch:\n" + ex, "Bubbletill POS", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None);
            Shutdown();
            return;
        }
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
        var home = AppHost.Services.GetRequiredService<HomeView>();
        mainWindow.LoginComplete(home);
    }

    public static Button CreateButton(IButtonData buttonData, Style buttonStyle, UserControl permissionCancelView)
    {
        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        var posController = AppHost.Services.GetRequiredService<POSController>();
        Button button = new Button();
        button.Style = buttonStyle;
        button.Content = buttonData.Name;
        button.Click += (s, e) =>
        {
            if (posController.CurrentOperator.HasBoolPermission(buttonData.Permission))
            {
                buttonData.OnClick(mainWindow);
            }
            else
            {
                mainWindow.POSViewContainer.Content = new BoolAuthDialogue((OperatorBoolPermission)buttonData.Permission, () => { buttonData.OnClick(mainWindow); }, () => { mainWindow.POSViewContainer.Content = permissionCancelView; });
            }
        };

        return button;
    }
}
