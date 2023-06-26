using BT_COMMONS.App;
using BT_COMMONS.DataRepositories;
using BT_COMMONS.Operators;
using BT_COMMONS.Transactions;
using BT_COMMONS.Transactions.TenderAttributes;
using BT_POS.Views;
using BT_POS.Views.Tender;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BT_POS;

public class POSController
{

    private readonly IOperatorRepository _operatorRepository;
    private readonly ITransactionRepository _transactionRepository;
    public string? ControllerAuthenticationToken { get; set; }
    public bool GotInitialControllerData { get; set; } = false;

    public bool OnlineToController { get; set; }

    public int StoreNumber { get; set; }
    public int RegisterNumber { get; set; }
    public bool RegisterOpen { get; set; }

    public Dictionary<TransactionTender, float> TenderHardTotals { get; set; }
    public Dictionary<TransactionType, float> TypeHardTotals { get; set; }

    public Operator? CurrentOperator { get; set; }
    public Transaction? CurrentTransaction { get; set; }
    public int CurrentTransId = 0;

    public POSController(IOperatorRepository operatorRepository, ITransactionRepository transactionRepository)
    {
        _operatorRepository = operatorRepository;
        _transactionRepository = transactionRepository;

        TenderHardTotals = new Dictionary<TransactionTender, float>();
        TypeHardTotals = new Dictionary<TransactionType, float>();
    }

    public void HeaderError(string? error = null)
    {
        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
        mw.HeaderError(error);
    }

    public async Task<bool> CompleteLogin()
    {
        App.SetAPIToken(ControllerAuthenticationToken);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(ControllerAuthenticationToken);
        var id = token.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

        var oper = await _operatorRepository.GetOperator(Int32.Parse(id));
        if (oper == null)
        {
            return false;
        }

        if (!GotInitialControllerData)
        {
            int? prevTrans = await _transactionRepository.GetPreviousTransactionId(StoreNumber, RegisterNumber);

            if (prevTrans == null)
                return false;

            CurrentTransId = prevTrans.Value;
        }

        CurrentOperator = oper;

        return true;
    }

    public void StartTransaction(TransactionType type)
    {
        if (CurrentTransaction != null)
            return;

        CurrentTransId++;
        CurrentTransaction = new Transaction();
        CurrentTransaction.Init(StoreNumber, RegisterNumber, CurrentOperator!.OperatorId, DateTime.Now, CurrentTransId, type);

        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
        mw.POSParentHeader_Trans.Text = "Transaction# " + CurrentTransId;
    }

    public void AddItemToBasket(BasketItem item)
    {
        if (CurrentTransaction == null)
        {
            StartTransaction(TransactionType.SALE);
        }

        CurrentTransaction!.AddToBasket(item);
    }

    public void AddTender(TransactionTender tender, float amount)
    {
        CurrentTransaction!.AddTender(tender, amount);

        if (CurrentTransaction.IsTenderComplete())
        {
            Submit();
            return;
        }

        MainWindow mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
        TenderHomeView tenderHome = App.AppHost.Services.GetRequiredService<TenderHomeView>();
        mainWindow.POSViewContainer.Content = tenderHome;
    }

    private void IncreaseTenderHardTotal(TransactionTender tender, float amount)
    {
        var current = TenderHardTotals.GetValueOrDefault(tender, 0);
        current += amount;
        TenderHardTotals[tender] = current;

        string json = JsonConvert.SerializeObject(new HardTotals
        {
            Tender = TenderHardTotals,
            Type = TypeHardTotals
        });

        File.WriteAllText("C:\\bubbletill\\hardtotals.json", json);
    }

    private void IncreaseTypeHardTotal(TransactionType type, float amount)
    {
        var current = TypeHardTotals.GetValueOrDefault(type, 0);
        current += amount;
        TypeHardTotals[type] = current;

        string json = JsonConvert.SerializeObject(new HardTotals
        {
            Tender = TenderHardTotals,
            Type = TypeHardTotals
        });

        File.WriteAllText("C:\\bubbletill\\hardtotals.json", json);
    }

    // Custom transactions
    public void OpenRegister()
    {
        StartTransaction(TransactionType.REGISTER_OPEN);

        RegisterOpen = true;
        string json = JsonConvert.SerializeObject(new AppConfig
        {
            Store = StoreNumber,
            Register = RegisterNumber,
            RegisterOpen = RegisterOpen
        });

        File.WriteAllText("C:\\bubbletill\\data.json", json);

        Submit();
    }

    public void CloseRegister()
    {
        StartTransaction(TransactionType.REGISTER_CLOSE);

        RegisterOpen = false;
        string dataJson = JsonConvert.SerializeObject(new AppConfig
        {
            Store = StoreNumber,
            Register = RegisterNumber,
            RegisterOpen = RegisterOpen
        });

        File.WriteAllText("C:\\bubbletill\\data.json", dataJson);

        // Clear hard totals
        CurrentTransaction!.Logs.Add("Regiser Close Reading");
        foreach (KeyValuePair<TransactionTender, float> entry in TenderHardTotals)
        {
            CurrentTransaction.Logs.Add(entry.Key.GetTenderInternalName() + ": " + entry.Value);
        }
        CurrentTransaction!.Logs.Add(" ");
        foreach (KeyValuePair<TransactionType, float> entry in TypeHardTotals)
        {
            CurrentTransaction.Logs.Add(entry.Key.ToString() + ": " + entry.Value);
        }

        TenderHardTotals = new Dictionary<TransactionTender, float>();
        TypeHardTotals = new Dictionary<TransactionType, float>();
        string json = JsonConvert.SerializeObject(new HardTotals
        {
            Tender = TenderHardTotals,
            Type = TypeHardTotals
        });

        File.WriteAllText("C:\\bubbletill\\hardtotals.json", json);

        Submit();
        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
        mw.Logout();
    }

    public void VoidTransaction()
    {
        CurrentTransaction!.UpdateTransactionType(TransactionType.VOID);
        CurrentTransaction.VoidTender();
        Submit();
    }

    // Submits current transaction to the database.
    private async void Submit()
    {
        if (CurrentTransaction == null)
            return;


        // Update hard totals
        foreach (KeyValuePair<TransactionTender, float> entry in CurrentTransaction.Tenders)
        {
            if (CurrentTransaction.GetChangeTender() == entry.Key)
            {
                IncreaseTenderHardTotal(entry.Key, entry.Value - CurrentTransaction.Change);
                continue;
            }

            IncreaseTenderHardTotal(entry.Key, entry.Value);
        }

        IncreaseTypeHardTotal(CurrentTransaction.Type, CurrentTransaction.GetTotal());

        CurrentTransaction.Logs.Add("Transaction " + CurrentTransaction.TransactionId + " ended at " + DateTime.Now.ToString());

        var success = await _transactionRepository.SubmitTransaction(CurrentTransaction);
        if (!success)
        {
            CurrentTransaction.Logs.Add("Transaction failed to submit to controller.");
            // Store on disk?
            return;
        }

        CurrentTransaction = null;

        MainWindow mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
        HomeView home = App.AppHost.Services.GetRequiredService<HomeView>();
        mainWindow.POSViewContainer.Content = home;
    }
}
