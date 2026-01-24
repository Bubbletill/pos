using BT_COMMONS.App;
using BT_COMMONS.DataRepositories;
using BT_COMMONS.Operators;
using BT_COMMONS.Transactions;
using BT_COMMONS.Transactions.TenderAttributes;
using BT_COMMONS.Transactions.TypeAttributes;
using BT_POS.Components;
using BT_POS.Integrations.Hardware;
using BT_POS.RepositoryImpl;
using BT_POS.Views;
using BT_POS.Views.Admin;
using BT_POS.Views.Dialogues;
using BT_POS.Views.Tender;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.ApplicationModel.VoiceCommands;

namespace BT_POS;

public class POSController
{

    private readonly IOperatorRepository _operatorRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ISuspendRepository _suspendRepository;
    private readonly IStockRepository _stockRepository;
    public string? ControllerAuthenticationToken { get; set; }
    public bool GotInitialControllerData { get; set; } = false;

    public bool OnlineToController { get; set; }

    public int StoreNumber { get; set; }
    public int RegisterNumber { get; set; }
    public bool RegisterOpen { get; set; }

    public Dictionary<TransactionTender, decimal> TenderHardTotals { get; set; }
    public Dictionary<TransactionType, decimal> TypeHardTotals { get; set; }

    public Operator? CurrentOperator { get; set; }
    public Dictionary<int, OperatorGroup>? OperatorGroups { get; set; }
    public Transaction? CurrentTransaction { get; set; }
    public List<TransactionLog> TransactionLogQueue { get; set; }
    public int CurrentTransId = 0;
    public bool TrainingMode = false;

    public bool LoanPrompted = false;

    private SerialLineDisplay _serialLineDisplay;

    public POSController(IOperatorRepository operatorRepository, ITransactionRepository transactionRepository, ISuspendRepository suspendRepository, IStockRepository stockRepository)
    {
        _operatorRepository = operatorRepository;
        _transactionRepository = transactionRepository;
        _suspendRepository = suspendRepository;
        _stockRepository = stockRepository;

        TenderHardTotals = new Dictionary<TransactionTender, decimal>();
        TypeHardTotals = new Dictionary<TransactionType, decimal>();
        OperatorGroups = new Dictionary<int, OperatorGroup>();
        TransactionLogQueue = new List<TransactionLog>();
    }

    public void HeaderError(string? error = null)
    {
        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
        mw.HeaderError(error);
    }

    public async Task<bool> CompleteLogin(int id)
    {
        var oper = await _operatorRepository.GetOperator(id);
        if (oper == null)
        {
            return false;
        }

        if (!oper.HasBoolPermission(OperatorBoolPermission.POS_Access))
        {
            return false;
        }

        if (!GotInitialControllerData)
        {
            //int? prevTrans = await _transactionRepository.GetPreviousTransactionId(StoreNumber, RegisterNumber);
            int? prevTrans = GetLocalTransactionNumber();

            if (prevTrans == null)
            {
                prevTrans = await _transactionRepository.GetPreviousTransactionId(StoreNumber, RegisterNumber);
                if (prevTrans == null)
                {
                    return false;
                }
            }

            CurrentTransId = prevTrans.Value;
            UpdateLocalTransactionNumber();
        }

        CurrentOperator = oper;
        LineDisplayWrite("Welcome to", "Bubbletill");

        return true;
    }

    public void StartTransaction(TransactionType type)
    {
        if (CurrentTransaction != null)
            return;

        if (CurrentTransId != 9999)
            CurrentTransId++;
        else
            CurrentTransId = 1;
        CurrentTransaction = new Transaction();
        CurrentTransaction.Init(StoreNumber, RegisterNumber, CurrentOperator!, DateTime.Now, CurrentTransId, type);
        TransactionLogQueue.ForEach(log => CurrentTransaction.Logs.Add(log));
        TransactionLogQueue.Clear();

        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
        mw.POSParentHeader_Trans.Text = "Transaction# " + CurrentTransId;
    }

    public void CheckTransactionType()
    {
        Debug.WriteLine("running!!");
        if (CurrentTransaction == null)
            return;

        Debug.WriteLine("past curr trans!!");
        Debug.WriteLine(CurrentTransaction.Type);

        if (CurrentTransaction.Type != TransactionType.SALE &&
            CurrentTransaction.Type != TransactionType.EXCHANGE &&
            CurrentTransaction.Type != TransactionType.RETURN)
            return;

        Debug.WriteLine("past trans type!!");

        int saleItems = 0;
        int returnItems = 0;
        CurrentTransaction.Basket.ForEach(b =>
        {
            if (b.Refund)
                returnItems++;
            else
                saleItems++;
        });

        Debug.WriteLine("Return: " + returnItems + ", Sale: " + saleItems);

        if ((saleItems == 0 && returnItems == 0) || (saleItems > 0 && returnItems == 0))
        {
            CurrentTransaction.UpdateTransactionType(TransactionType.SALE);
        } 
        else if (saleItems == 0 && returnItems > 0)
        {
            CurrentTransaction.UpdateTransactionType(TransactionType.RETURN);
        } 
        else if (saleItems > 0 && returnItems > 0)
        {
            CurrentTransaction.UpdateTransactionType(TransactionType.EXCHANGE);
        }
    }

    public void AddItemToBasket(BasketItem item)
    {
        HeaderError();
        if (CurrentTransaction == null)
        {
            StartTransaction(TransactionType.SALE);
        }

        if (item.AgeRestricted > CurrentTransaction!.CustomerAge && !item.Refund)
        {
            MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
            mw.POSViewContainer.Content = new AgeRestrictedDialogue(this, item);
            return;
        }

        string[] lines = CurrentTransaction!.AddToBasket(item);
        CurrentTransaction!.SelectedItem = item;
        LineDisplayWrite(lines[0], lines[1]);
        CheckTransactionType();
    }

    public async Task AddItemToBasket(int code)
    {
        BasketItem? item = await _stockRepository.GetItem(code);
        if (item == null)
        {
            HeaderError("Invalid item code.");
            return;
        }

        AddItemToBasket(item);
    }

    public void AddTender(TransactionTender tender, decimal amount)
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

    private void IncreaseTenderHardTotal(TransactionTender tender, decimal amount, bool forcePositive = true)
    {
        if (CurrentTransaction!.GetTotal() < 0 && forcePositive)
            amount *= -1;

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

    private void IncreaseTypeHardTotal(TransactionType type, decimal amount)
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

    public async void RegisterXRead()
    {
        StartTransaction(TransactionType.X_READ);
        LineDisplayWrite("X Read", "Please wait");

        CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.NSGeneral, "Regiser X Reading"));
        foreach (KeyValuePair<TransactionTender, decimal> entry in TenderHardTotals)
        {
            CurrentTransaction.Logs.Add(new TransactionLog(TransactionLogType.NSGeneral, entry.Key.GetTenderInternalName() + ": " + entry.Value));
        }
        CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.NSGeneral, " "));
        foreach (KeyValuePair<TransactionType, decimal> entry in TypeHardTotals)
        {
            if (entry.Key.ShowOnXRead())
                CurrentTransaction.Logs.Add(new TransactionLog(TransactionLogType.NSGeneral, entry.Key.ToString() + ": " + entry.Value));
        }

        await Submit();
    }

    public async void CloseRegister()
    {
        if (TrainingMode)
            return;

        StartTransaction(TransactionType.REGISTER_CLOSE);
        LineDisplayWrite("Closing Register", "Please wait");

        RegisterOpen = false;
        LoanPrompted = false;
        string dataJson = JsonConvert.SerializeObject(new AppConfig
        {
            Store = StoreNumber,
            Register = RegisterNumber,
            RegisterOpen = RegisterOpen
        });

        File.WriteAllText("C:\\bubbletill\\data.json", dataJson);

        // Clear hard totals
        CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.NSGeneral, "Regiser Close Reading"));
        foreach (KeyValuePair<TransactionTender, decimal> entry in TenderHardTotals)
        {
            CurrentTransaction.Logs.Add(new TransactionLog(TransactionLogType.NSGeneral, entry.Key.GetTenderInternalName() + ": " + entry.Value));
        }
        CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.NSGeneral, " "));
        foreach (KeyValuePair<TransactionType, decimal> entry in TypeHardTotals)
        {
            if (entry.Key.ShowOnXRead())
                CurrentTransaction.Logs.Add(new TransactionLog(TransactionLogType.NSGeneral, entry.Key.ToString() + ": " + entry.Value));
        }

        TenderHardTotals = new Dictionary<TransactionTender, decimal>();
        TypeHardTotals = new Dictionary<TransactionType, decimal>();
        foreach (TransactionTender tender in Enum.GetValues(typeof(TransactionTender)))
        {
            TenderHardTotals.Add(tender, 0);
        }

        foreach (TransactionType type in Enum.GetValues(typeof(TransactionType)))
        {
            TypeHardTotals.Add(type, 0);
        }
        string json = JsonConvert.SerializeObject(new HardTotals
        {
            Tender = TenderHardTotals,
            Type = TypeHardTotals
        });

        File.WriteAllText("C:\\bubbletill\\hardtotals.json", json);

        await Submit();
        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
        mw.Logout();
    }

    public void LoanTransaction(decimal loan)
    {
        StartTransaction(TransactionType.LOAN);
        AddItemToBasket(new BasketItem(0, "Loan", loan, 0));
        CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.NSGeneral, "Loan: £" + loan));

        Submit();
    }

    public void TrainingModeToggleTransaction()
    {
        bool status = !TrainingMode;
        TrainingMode = status;
        StartTransaction(status ? TransactionType.TRAINING_ON : TransactionType.TRAINING_OFF);

        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
        mw.POSParentHeader_TrainingStatus.Visibility = status ? Visibility.Visible : Visibility.Hidden;
        var converter = new BrushConverter();
        mw.POSParentHeader.Style = status ? (Style)mw.FindResource("BTParentHeaderTraining") : (Style)mw.FindResource("BTParentHeader");
        Submit();
    }

    public void VoidTransaction()
    {
        LineDisplayWrite("Transaction Voided", "");
        CurrentTransaction!.UpdateTransactionType(TransactionType.VOID);
        CurrentTransaction.VoidTender();
        CurrentTransaction.Logs.Add(new TransactionLog(TransactionLogType.NSGeneral, "Transaction Voided"));
        Submit();
    }

    public async Task ActionPostVoid(Transaction transaction)
    {
        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
        bool success = await _transactionRepository.UpdatePostTransactionType(transaction.Utid, TransactionType.POST_VOID);
        if (!success)
        {
            mw.POSViewContainer.Content = new PostVoidView(transaction);
            HeaderError("Failed to post void this transaction. Please try again later.");
            return;
        }

        foreach (var entry in transaction.Tenders)
        {
            IncreaseTenderHardTotal(entry.Key, -entry.Value, false);
            CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Tender " + entry.Key.GetTenderInternalName() + " modified by " + -entry.Value + " to hard totals"));
        }

        IncreaseTypeHardTotal(transaction.Type, -transaction.GetTotal());
        CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Type " + transaction.Type.FriendlyName() + " modified by " + -transaction.GetTotal() + " to hard totals"));

        IncreaseTypeHardTotal(TransactionType.POST_VOID, transaction.GetTotal());
        CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Post Void value of " + transaction.GetTotal() + " incremented to hard total"));
        CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Transaction post voided successfully."));
        await Submit();
    }

    // If an empty transaction started, rather than voiding it, it can be cancelled.
    public void CancelTransaction()
    {
        if (CurrentTransaction == null)
            return;

        TransactionLogQueue.Clear();
        CurrentTransaction = null;

        CurrentTransId--;
        if (CurrentTransId == 0)
            CurrentTransId = 9999;

        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
        mw.POSParentHeader_Trans.Text = "Transaction# " + CurrentTransId;
    }

    // Submits current transaction to the database.
    public async Task Submit()
    {
        if (CurrentTransaction == null)
            return;

        MainWindow mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();

        if (CurrentTransaction.Type.GetReturnHome())
        {
            BasketOnlyView basketOnly = App.AppHost.Services.GetRequiredService<BasketOnlyView>();
            mainWindow.POSViewContainer.Content = basketOnly;
        }

        // Update hard totals
        foreach (KeyValuePair<TransactionTender, decimal> entry in CurrentTransaction.Tenders)
        {
            if (TrainingMode)
                break;

            if (CurrentTransaction.GetChangeTender() == entry.Key)
            {
                IncreaseTenderHardTotal(entry.Key, entry.Value - CurrentTransaction.Change);
                continue;
            }

            IncreaseTenderHardTotal(entry.Key, entry.Value);
        }

        if (!TrainingMode)
            IncreaseTypeHardTotal(CurrentTransaction.Type, CurrentTransaction.GetTotal());

        TransactionLogQueue.ForEach(log => CurrentTransaction.Logs.Add(log));
        TransactionLogQueue.Clear();
        CurrentTransaction.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Transaction " + CurrentTransaction.TransactionId + " ended at " + DateTime.Now.ToString()));

        if (CurrentTransaction.Change != 0)
        {
            LineDisplayWrite("Change: £" + CurrentTransaction.Change, "");
            InfoPopup popup = new InfoPopup("Amount Tendered: £" + CurrentTransaction.Tenders[CurrentTransaction.GetChangeTender()] + "\nChange: £" + CurrentTransaction.Change);
            popup.ShowDialog();
        }

        if (CurrentTransaction.Type == TransactionType.VOID)
        {
            foreach (var item in CurrentTransaction.ReturnBasket)
            {
                await _transactionRepository.ToggleReturnLock(item.Key, false);
            }
            CurrentTransaction.ReturnBasket.Clear();
        }

        var success = await _transactionRepository.SubmitTransaction(CurrentTransaction, TrainingMode && CurrentTransaction.Type != TransactionType.TRAINING_ON ? TransactionType.TRAINING_TRANS : null);
        if (!success)
        {
            ControllerOffline();
        } else
        {
            await ControllerOnline();
        }
        UpdateLocalTransactionNumber();

        if (!CurrentTransaction.Type.GetReturnHome())
        {
            CurrentTransaction = null;
            return;
        }

        CurrentTransaction = null;

        HomeView home = App.AppHost.Services.GetRequiredService<HomeView>();
        mainWindow.POSViewContainer.Content = home;
        LineDisplayWrite("Welcome to", "Bubbletill");
    }

    // Suspend transaction
    public async void Suspend()
    {
        if (CurrentTransaction == null)
            return;

        LineDisplayWrite("Transaction Suspend", "");

        MainWindow mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();

        BasketOnlyView basketOnly = App.AppHost.Services.GetRequiredService<BasketOnlyView>();
        mainWindow.POSViewContainer.Content = basketOnly;

        TransactionLogQueue.ForEach(log => CurrentTransaction.Logs.Add(log));
        TransactionLogQueue.Clear();
        CurrentTransaction.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Transaction " + CurrentTransaction.TransactionId + " suspended at " + DateTime.Now.ToString()));

        var suspendSuccess = await _suspendRepository.Suspend(CurrentTransaction);

        CurrentTransaction.UpdateTransactionType(TransactionType.SUSPEND);
        var submitSuccess = await _transactionRepository.SubmitTransaction(CurrentTransaction, TransactionType.SUSPEND);
        if (!suspendSuccess || !submitSuccess)
        {
            ControllerOffline();
        }
        else
        {
            await ControllerOnline();
        }
        UpdateLocalTransactionNumber();

        CurrentTransaction = null;

        HomeView home = App.AppHost.Services.GetRequiredService<HomeView>();
        mainWindow.POSViewContainer.Content = home;
        LineDisplayWrite("Welcome to", "Bubbletill");
    }

    public async void Resume(Transaction transaction, int sid)
    {
        if (CurrentTransaction != null)
            return;

        if (CurrentTransId != 9999)
            CurrentTransId++;
        else
            CurrentTransId = 1;
        int oldTid = transaction.TransactionId;
        CurrentTransaction = transaction;
        //CurrentTransaction.Init(StoreNumber, RegisterNumber, CurrentOperator!, DateTime.Now, CurrentTransId, type);
        CurrentTransaction.TransactionId = CurrentTransId;
        CurrentTransaction.Register = RegisterNumber;
        CurrentTransaction.DateTime = DateTime.Now;
        CurrentTransaction.Operator = CurrentOperator!;
        CurrentTransaction.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Transaction " + oldTid + " (now " + CurrentTransId + ") resumed at " + DateTime.Now.ToString() + " by " + CurrentOperator!.ReducedName() + ", ID " + CurrentOperator.OperatorId));
        TransactionLogQueue.ForEach(log => CurrentTransaction.Logs.Add(log));
        TransactionLogQueue.Clear();

        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
        mw.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<HomeView>();
        mw.POSParentHeader_Trans.Text = "Transaction# " + CurrentTransId;

        await _suspendRepository.Delete(sid);
    }

    public void UpdateLocalTransactionNumber()
    {
        File.WriteAllText("C:\\bubbletill\\transid.txt", CurrentTransId.ToString());
    }

    public int? GetLocalTransactionNumber()
    {
        try
        {
            using (StreamReader r = new StreamReader("C:\\bubbletill\\transid.txt"))
            {
                int id = Int32.Parse(r.ReadToEnd());
                return id;
            }
        } catch (Exception ex) {
            Trace.WriteLine(ex);
            return null;
        }
    }

    public void ControllerOffline()
    {
        MainWindow mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
        mainWindow.POSParentHeader_Status.Text = "Offline";
    }

    public async Task ControllerOnline()
    {
        string[] files = Directory.GetFiles("C:\\bubbletill\\offlinetransactions", "*.json");
        foreach (var item in files)
        {
            var success = false;
            using (StreamReader r = new StreamReader(item))
            {
                string json = r.ReadToEnd();
                Trace.WriteLine(json);
                Transaction trans = JsonConvert.DeserializeObject<Transaction>(json);

                if (trans == null)
                {
                    throw new Exception();
                }

                trans.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Reattempting transaction submission at " + DateTime.Now));
                success = await _transactionRepository.SubmitTransaction(trans);
            }
            if (success)
            {
                File.Delete(item);
            }
        }

        MainWindow mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
        mainWindow.POSParentHeader_Status.Text = "Online";
    }

    public void LineDisplayWrite(string line1, string line2)
    {
        if (_serialLineDisplay == null) return;
        _serialLineDisplay.Show(line1, line2);
    }

    public void LineDisplayClear()
    {
        if (_serialLineDisplay == null) return;
        _serialLineDisplay.Clear();
    }

    public void LineDisplayLoad(SerialLineDisplay? serialLineDisplay)
    {
        _serialLineDisplay = serialLineDisplay;
    }

    public void LineDisplayDispose()
    {
        if (_serialLineDisplay != null) _serialLineDisplay.Dispose();
    }
}
