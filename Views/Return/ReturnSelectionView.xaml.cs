using BT_COMMONS.DataRepositories;
using BT_COMMONS.Operators;
using BT_COMMONS.Transactions;
using BT_POS.Buttons;
using BT_POS.Buttons.Menu;
using BT_POS.Components;
using BT_POS.RepositoryImpl;
using BT_POS.Views.Admin;
using BT_POS.Views.Dialogues;
using BT_POS.Views.Menus;
using BT_POS.Views.Return;
using BT_POS.Views.Tender;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BT_POS.Views.Return;

public partial class ReturnSelectionView : UserControl
{
    private readonly MainWindow _mainWindow;
    private readonly POSController _controller;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IStockRepository _stockRepository;
    private ReturnEntry _returnEntry;

    private readonly Style _buttonStyle;

    public ReturnSelectionView(MainWindow mainWindow, POSController posController, ITransactionRepository transactionRepository, IStockRepository stockRepository, ReturnEntry entry)
    {
        _mainWindow = mainWindow;
        _controller = posController;
        _returnEntry = entry;
        _transactionRepository = transactionRepository;
        _stockRepository = stockRepository;

        InitializeComponent();

        List<BasketItem> returnAllowed = new List<BasketItem>();
        List<BasketItem> returnDisallowed = new List<BasketItem>();
        _returnEntry.ParsedBasket.ForEach(b =>
        {
            if (!b.Returned)
                returnAllowed.Add(b);
            else
                returnDisallowed.Add(b);
        });
        BasketGrid.ItemsSource = returnAllowed;
        ExpiredGrid.ItemsSource = returnDisallowed;

        Keypad.SelectedBox = ManualCodeEntryBox;
        ManualCodeEntryBox.Focus();

        if (_controller.CurrentTransaction == null)
        {
            _controller.StartTransaction(TransactionType.RETURN);
        } else
        {
            _controller.CurrentTransaction.UpdateTransactionType(TransactionType.EXCHANGE);
        }

        _controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Return Details:"));
        if (!_returnEntry.IsNoInfo)
        {
            _controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Store: " + _returnEntry.Store));
            _controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Register: " + _returnEntry.Register));
            _controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Transaction ID: " + _returnEntry.TransactionId));
            _controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Date: " + _returnEntry.Date));
        } else
        {
            _controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Performing return with no receipt."));
            ViewInfo.Title = "Return without Receipt";
            ViewInfo.Information = "You are returning without a receipt. Please carefully check all items against their description. Scan or enter the item code, then once all items are selected press Finish.";
        }
    }

    private void Finish_Click(object sender, RoutedEventArgs e)
    {
        int returnedItems = 0;
        foreach (BasketItem item in _returnEntry.ParsedBasket)
        {
            if (item.Refund && !item.Returned)
            {
                item.Returned = true;
                item.PartOfReturnId = _returnEntry.Urid;
                _controller.AddItemToBasket(item);
                returnedItems++;
            }
        }

        if (returnedItems != 0 && !_returnEntry.IsNoInfo)
        {
            if (!_controller.CurrentTransaction!.ReturnBasket.ContainsKey(_returnEntry.Urid))
                _controller.CurrentTransaction!.ReturnBasket.Add(_returnEntry.Urid, _returnEntry);
        }

        ReturnHome();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        ReturnHome();
    }

    private void ReturnHome()
    {
        if (!_controller.CurrentTransaction!.ReturnBasket.ContainsKey(_returnEntry.Urid) && !_returnEntry.IsNoInfo)
            _transactionRepository.ToggleReturnLock(_returnEntry.Urid, false);

        if (_controller.CurrentTransaction!.Basket.Count == 0)
        {
            _controller.CancelTransaction();
        }

        _controller.CheckTransactionType();

        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
        HomeView hv = App.AppHost.Services.GetRequiredService<HomeView>();

        mw.POSViewContainer.Content = hv;
    }


    private async void ManualCodeEntryBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        int code;
        try
        {
            code = Int32.Parse(ManualCodeEntryBox.Text);
        } 
        catch (Exception ex)
        {
            ManualCodeEntryBox.Clear();
            _mainWindow.HeaderError("Invalid item code.");
            return;
        }

        if (!_returnEntry.IsNoInfo)
        {
            List<BasketItem> potentialItems = _returnEntry.ParsedBasket.FindAll(b => code == b.Code && !b.Returned && !b.Refund);
            if (potentialItems.Count == 0)
            {
                _controller.HeaderError("Item not found on this receipt. Do not return this item.");
            }
            else if (potentialItems.Count > 1)
            {
                _controller.HeaderError("Multiple items found. Please select item manually.");
            }
            else if (potentialItems.Count == 1)
            {
                potentialItems[0].Refund = true;
                _controller.HeaderError();
            }
        } else 
        {
            BasketItem? item = await _stockRepository.GetItem(code);
            if (item == null)
            {
                ManualCodeEntryBox.Clear();
                _mainWindow.HeaderError("Invalid item code.");
                return;
            }

            _mainWindow.HeaderError();

            _returnEntry.ParsedBasket.Add(item);
            BasketGrid.ItemsSource = _returnEntry.ParsedBasket;
            item.Refund = true;
        }

        // Has to run twice, can't tell you why
        BasketGrid.CommitEdit();
        BasketGrid.CommitEdit();

        BasketGrid.Items.Refresh();
        ManualCodeEntryBox.Clear();
    }

    private void ManualCodeEntryBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }
}
