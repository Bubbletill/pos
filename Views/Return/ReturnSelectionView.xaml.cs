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
    private ReturnEntry _returnEntry;

    private readonly Style _buttonStyle;

    public ReturnSelectionView(MainWindow mainWindow, POSController posController, ITransactionRepository transactionRepository, ReturnEntry entry)
    {
        _mainWindow = mainWindow;
        _controller = posController;
        _returnEntry = entry;
        _transactionRepository = transactionRepository;

        InitializeComponent();

        BasketGrid.ItemsSource = _returnEntry.ParsedBasket;

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
        _controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Store: " + _returnEntry.Store));
        _controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Register: " + _returnEntry.Register));
        _controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Transaction ID: " + _returnEntry.TransactionId));
        _controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Date: " + _returnEntry.Date));
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
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
        
/*        BasketItem? item = await _stockRepository.GetItem(code);
        if (item == null)
        {
            ManualCodeEntryBox.Clear();
            _mainWindow.HeaderError("Invalid item code.");
            return;
        }*/

/*        _mainWindow.HeaderError();
        if (_controller.CurrentTransaction == null)
        {
            LoadButtons(App.HomeTransButtons);
        }
        _controller.AddItemToBasket(item);
        TotalTextBlock.Text = "£" + _controller.CurrentTransaction!.GetTotal();
        BasketComponent.BasketGrid.ItemsSource = _controller.CurrentTransaction!.Basket;
        BasketComponent.BasketGrid.Items.Refresh();
        ManualCodeEntryBox.Clear();*/
    }

    private void ManualCodeEntryBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }
}
