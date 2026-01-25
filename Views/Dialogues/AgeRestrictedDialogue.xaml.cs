using BT_COMMONS.Transactions;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace BT_POS.Views.Dialogues;

public partial class AgeRestrictedDialogue : UserControl
{
    private readonly POSController _controller;
    private readonly BasketItem _item;

    public AgeRestrictedDialogue(POSController posController, BasketItem item)
    {
        _controller = posController;
        _item = item;

        InitializeComponent();

        if (_controller.CurrentTransaction != null )
        {
            BasketComponent.BasketGrid.ItemsSource = _controller.CurrentTransaction.Basket;
        }

        ViewInformation.Information =
            "Is the customer over " + item.AgeRestricted
            + "? Please confirm with a valid, physical photo ID: passport, driving license, PASS card. Please either approve or refuse the item."
            + "\nRestricted Item: " + item.Description;
    }

    private void Approve_Click(object sender, RoutedEventArgs e)
    {
        _controller.CurrentTransaction!.CustomerAge = _item.AgeRestricted;
        _controller.AddItemToBasket( _item );
        _controller.CurrentTransaction!.Logs.Add(
            new TransactionLog(TransactionLogType.Hidden, "Customer age verified."));

        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
        mw.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<HomeView>();
    }

    private void Refuse_Click(object sender, RoutedEventArgs e)
    {
        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
        mw.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<HomeView>();
        _controller.CurrentTransaction!.Logs.Add(
            new TransactionLog(TransactionLogType.Hidden, "Item refused: " + _item.Code + " _ " + _item.Description + ". Age check refused."));
    }
}
