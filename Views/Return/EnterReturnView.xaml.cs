using BT_COMMONS.DataRepositories;
using BT_COMMONS.Operators;
using BT_COMMONS.Transactions;
using BT_COMMONS.Transactions.TypeAttributes;
using BT_POS.Buttons;
using BT_POS.Views.Dialogues;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
using Windows.Devices.Pwm;

namespace BT_POS.Views.Return;

public partial class EnterReturnView : UserControl
{
    private readonly POSController _controller;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IStockRepository _stockRepository;

    public EnterReturnView(POSController controller, ITransactionRepository transactionRepository, IStockRepository stockRepository)
    {
        _controller = controller;
        _transactionRepository = transactionRepository;
        _stockRepository = stockRepository;

        InitializeComponent();
        Keypad.DisableButton(Keypad.PeriodButton);
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
        HomeView hv = App.AppHost.Services.GetRequiredService<HomeView>();
        mw.POSViewContainer.Content = hv;
    }

    private async void Accept_Click(object sender, RoutedEventArgs e)
    {
        ReturnEntry? returnEntry = await _transactionRepository.GetReturnEntry(int.Parse(StoreNumber.Text), int.Parse(RegisterNumber.Text), int.Parse(TransactionNumber.Text), DateOnly.Parse(Date.Text));
        if (returnEntry == null) {
            Transaction? transaction = await _transactionRepository.GetTransaction(int.Parse(StoreNumber.Text), int.Parse(RegisterNumber.Text), int.Parse(TransactionNumber.Text), DateOnly.Parse(Date.Text));
            if (transaction == null)
            {
                _controller.HeaderError("Transaction not found.");
                return;
            }

            if (!transaction.Type.CanReturn() || !transaction.PostTransType.CanReturn())
            {
                _controller.HeaderError("This transaction's type of " + transaction.Type.FriendlyName() + " does not permit returns.");
                return;
            }

            returnEntry = new ReturnEntry(transaction);
            int urid = await _transactionRepository.SubmitReturnEntry(returnEntry);
            if (urid == -1)
            {
                _controller.HeaderError("Failed to retrieve transaction data. Please try again.");
                return;
            }
            returnEntry.Urid = urid;

            MainWindow main = App.AppHost.Services.GetRequiredService<MainWindow>();
            main.POSViewContainer.Content = new ReturnSelectionView(main, _controller, _transactionRepository, _stockRepository, returnEntry);
            return;
        }

        if (returnEntry.Locked)
        {
            if (_controller.CurrentTransaction != null && _controller.CurrentTransaction.ReturnBasket.ContainsKey(returnEntry.Urid))
            {
                LoadTransaction(_controller.CurrentTransaction.ReturnBasket[returnEntry.Urid]);
                return;
            }

            _controller.HeaderError("This transaction is in-use and currently locked.");
            return;
        }

        LoadTransaction(returnEntry);
    }

    private void NoInfo_Click(object sender, RoutedEventArgs e)
    {
        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();

        if (_controller.CurrentOperator!.HasBoolPermission(OperatorBoolPermission.POS_Return_NoInform))
        {
            LoadTransaction(new ReturnEntry(_controller.StoreNumber, _controller.RegisterNumber, _controller.CurrentTransId, true));
        }
        else
        {
            mw.POSViewContainer.Content = new BoolAuthDialogue(OperatorBoolPermission.POS_Return_NoInform, () => 
            // Yes
            {
                LoadTransaction(new ReturnEntry(_controller.StoreNumber, _controller.RegisterNumber, _controller.CurrentTransId, true));
            }, 
            () => 
            { 
                mw.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<EnterReturnView>(); 
            });
        }
    }

    private async void LoadTransaction(ReturnEntry returnEntry)
    {
        if (!returnEntry.IsNoInfo)
            await _transactionRepository.ToggleReturnLock(returnEntry.Urid, true);
        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
        mw.POSViewContainer.Content = new ReturnSelectionView(mw, _controller, _transactionRepository, _stockRepository, returnEntry);
    }

    private void PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }
}
