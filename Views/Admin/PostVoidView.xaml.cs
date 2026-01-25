using BT_COMMONS.DataRepositories;
using BT_COMMONS.Transactions;
using BT_COMMONS.Transactions.TenderAttributes;
using BT_POS.Views.Dialogues;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace BT_POS.Views.Admin;

/// <summary>
/// Interaction logic for PostVoidView.xaml
/// </summary>
public partial class PostVoidView : UserControl
{
    private readonly POSController _controller;
    private readonly ITransactionRepository _transactionRepository;
    private readonly MainWindow _mainWindow;
    private readonly Transaction _transaction;

    public PostVoidView(Transaction transaction)
    {
        InitializeComponent();
        _controller = App.AppHost.Services.GetRequiredService<POSController>();
        _transactionRepository = App.AppHost.Services.GetRequiredService<ITransactionRepository>();
        _mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
        _transaction = transaction;

        ViewInformation.SetAdminColour();

        List<BasketItem> localBasket = new List<BasketItem>(_transaction.Basket);
        BasketComponent.BasketGrid.ItemsSource = localBasket;
        if (_transaction.Tenders.Count != 0)
        {
            localBasket.Add(new BasketItem(0, " ", 0, 0));
            foreach (KeyValuePair<TransactionTender, decimal> entry in _transaction.Tenders)
            {
                localBasket.Add(new BasketItem(0, entry.Key.GetTenderExternalName(), entry.Value, 0));
            }
            if (_transaction.Change != 0)
            {
                localBasket.Add(new BasketItem(0, "Change", _controller.CurrentTransaction.Change, 0));
            }
        }


        TotalTextBlock.Text = "£" + _transaction.GetTotal();

        _controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.NSGeneral, "Post Void details:"));
        _controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.NSGeneral, "Register: " + _transaction.Register));
        _controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.NSGeneral, "Transaction Number: " + _transaction.TransactionId));
        _controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.NSGeneral, "Date: " + _transaction.DateTime));
        _controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.NSGeneral, "Amount: £" + _transaction.GetTotal()));

    }

    private async void Confirm_Click(object sender, RoutedEventArgs e)
    {
        if (_transaction.Register != _controller.RegisterNumber)
        {
            _mainWindow.POSViewContainer.Content = new YesNoDialogue("WARNING! This transaction was not performed on the current till. If confirmed, the amount of the transaction will be (un)creddited to this till. Are you sure you want to complete the post void?",
                // Yes
                () =>
                {
                    PostVoid();
                },
                // No
                () =>
                {
                    Cancel();
                },
                true // Admin colours
                );

            return;
        }

        PostVoid();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Cancel();
    }

    private async void PostVoid()
    {
        await _controller.ActionPostVoid(_transaction);
    }       

    private void Cancel()
    {
        _controller.CancelTransaction();
        AdminTrxnManagementMenuView av = App.AppHost.Services.GetRequiredService<AdminTrxnManagementMenuView>();
        _mainWindow.POSViewContainer.Content = av;
    }
}
