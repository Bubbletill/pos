using BT_COMMONS.DataRepositories;
using BT_COMMONS.Transactions;
using BT_COMMONS.Transactions.TypeAttributes;
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

namespace BT_POS.Views.Return;

public partial class EnterReturnView : UserControl
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly POSController _controller;

    public EnterReturnView(ITransactionRepository transactionRepository, POSController controller)
    {
        InitializeComponent();
        Keypad.DisableButton(Keypad.PeriodButton);
        _transactionRepository = transactionRepository;
        _controller = controller;
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
                _controller.HeaderError("This transaction's type does not permit returns. (" + transaction.Type + ")");
                return;
            }

            returnEntry = new ReturnEntry(transaction);
            await _transactionRepository.SubmitReturnEntry(returnEntry);

            MainWindow main = App.AppHost.Services.GetRequiredService<MainWindow>();
            main.POSViewContainer.Content = new ReturnSelectionView(main, _controller, _transactionRepository, returnEntry);
            return;
        }

        if (returnEntry.Locked)
        {
            _controller.HeaderError("This transaction is in-use and currently locked.");
            return;
        }

        await _transactionRepository.ToggleReturnLock(returnEntry.Urid, true);
        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
        mw.POSViewContainer.Content = new ReturnSelectionView(mw, _controller, _transactionRepository, returnEntry);
    }

    private void PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }
}
