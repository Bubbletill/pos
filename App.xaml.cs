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
using BT_POS.Views.Return;
using Square;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using BT_POS.Integrations.Square;
using BT_COMMONS.Integrations.Square;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using BT_COMMONS.Hotshot;
using BT_POS.Views.Hotshot;

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

    public static List<AdminCashMngmtButton> AdminCashManagementButtons;
    public static List<AdminRegMngmtButton> AdminRegManagementButtons;
    public static List<AdminTrxnMngmtButton> AdminTrxnManagementButtons;

    public static List<HotshotCategory> HotshotCategories;

    public static IConnection RabbitConnection;

    public static SquareIntegrationData? squareIntegrationData;
  

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
                services.AddSingleton<IHotshotRepository, HotshotRepository>();

                services.AddSingleton<MainWindow>();
                services.AddViewFactory<LoginView>();
                services.AddViewFactory<RegClosedView>();
                services.AddViewFactory<BasketOnlyView>();

                services.AddViewFactory<HomeView>();
                services.AddViewFactory<TenderHomeView>();
                services.AddViewFactory<HotshotView>();
                services.AddViewFactory<ResumeView>();

                services.AddViewFactory<EnterReturnView>();

                services.AddViewFactory<ItemModMenuView>();
                services.AddViewFactory<TransModMenuView>();

                services.AddViewFactory<AdminMenuView>();
                services.AddViewFactory<AdminCashManagementMenuView>();
                services.AddViewFactory<AdminTrxnManagementMenuView>();
                services.AddViewFactory<AdminRegManagementMenuView>();

                services.AddSingleton<POSController>();
            }).Build();

        var rabbitConnectionFactory = new ConnectionFactory() { HostName = "localhost" };
        RabbitConnection = rabbitConnectionFactory.CreateConnection();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            POSSplashScreen splash = new POSSplashScreen();
            splash.Show();

            splash.StatusText.Text = "Starting AppHost";
            await AppHost!.StartAsync();

            var controller = AppHost.Services.GetRequiredService<POSController>();

            splash.StatusText.Text = "Loading register data";
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

            splash.StatusText.Text = "Loading hard totals";
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

            splash.StatusText.Text = "Setting up operator groups";
            var operRepo = AppHost.Services.GetRequiredService<IOperatorRepository>();
            var operGroups = await operRepo.GetOperatorGroups();
            foreach (var group in operGroups)
            {
                group.Parse();
                controller.OperatorGroups.Add(group.Id, group);
            }

            splash.StatusText.Text = "Setting up configured buttons";
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
            AdminTrxnManagementButtons = await btnRepo.GetAdminTrxnManagementButtons();
            AdminRegManagementButtons = await btnRepo.GetAdminRegManagementButtons();

            splash.StatusText.Text = "Loading Hotshots";
            var hotshotRepo = AppHost.Services.GetRequiredService<IHotshotRepository>();
            HotshotCategories = await hotshotRepo.GetHotshotCategories();

            // TODO: load pos peripherals

            // Load integrations
            splash.StatusText.Text = "Loading POS intergrations";

            // World pay
            if (TenderTypes!.Contains(TransactionTender.WORLDPAY_CARD))
            {
                TenderTypes.Remove(TransactionTender.WORLDPAY_CARD);
/*              ProcessStartInfo processInfo;
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
                }*/
            }

            // squareup
            if (TenderTypes!.Contains(TransactionTender.SQUARE_CARD))
            {
                squareIntegrationData = new SquareIntegrationData();
                bool success = false;

                // Load data
                try
                {
                    using (StreamReader r = new StreamReader("C:\\bubbletill\\sqaure-integration.json"))
                    {
                        string json = r.ReadToEnd();
                        SquareDeviceData data = JsonConvert.DeserializeObject<SquareDeviceData>(json);

                        if (data == null || data.api_key == null || data.terminal_device_code == null || data.terminal_device_id == null)
                        {
                            throw new Exception("Invalid config file");
                        }

                        squareIntegrationData.APIKey = data.api_key;
                        squareIntegrationData.TerminalDeviceCode = data.terminal_device_code;
                        squareIntegrationData.TerminalDeviceId = data.terminal_device_id;
                    }

                    // start integration process
                    ProcessStartInfo processInfo;
                    processInfo = new ProcessStartInfo("C:\\bubbletill\\integrations\\square\\BT-INTEGRATIONS.SQUARE.exe");
                    processInfo.WorkingDirectory = "C:\\bubbletill\\integrations\\square";
                    processInfo.CreateNoWindow = true;
                    squareIntegrationData.IntegrationProcess = Process.Start(processInfo);

                    // Create api client
                    squareIntegrationData.Client = new SquareClient.Builder()
                        .Environment(Square.Environment.Production)
                        .BearerAuthCredentials(
                            new Square.Authentication.BearerAuthModel.Builder(squareIntegrationData.APIKey).Build())
                        .Build();

                    // Setup rabbitmq channel
                    var rabbitChannel = RabbitConnection.CreateModel();
                    rabbitChannel.ExchangeDeclare(exchange: "square.terminal.checkout", ExchangeType.Fanout);
                    var qName = rabbitChannel.QueueDeclare().QueueName;
                    var consumer = new EventingBasicConsumer(rabbitChannel);
                    rabbitChannel.QueueBind(queue: qName, exchange: "square.terminal.checkout", routingKey: string.Empty);
                    consumer.Received += (model, ea) =>
                    {
                        byte[] body = ea.Body.ToArray();
                        var data = Encoding.UTF8.GetString(body);
                        squareIntegrationData.RecieveCheckoutWebhook(data);
                    };

                    rabbitChannel.BasicConsume(queue: qName, autoAck: true, consumer: consumer);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Tender Square Card failed to initalise: " + ex.Message + ". This option has been removed.", "Bubbletill POS", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                    TenderTypes.Remove(TransactionTender.SQUARE_CARD);
                }
            }

            splash.StatusText.Text = "Starting POS...";
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

        RabbitConnection.Dispose();

        // Stop the squareup integration service
        if (squareIntegrationData != null && squareIntegrationData.IntegrationProcess != null)
        {
            squareIntegrationData.IntegrationProcess.Kill();
        }

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
