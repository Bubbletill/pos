using BT_COMMONS.DataRepositories;
using BT_COMMONS.Operators;
using BT_COMMONS.Transactions;
using BT_POS.Buttons;
using BT_POS.Buttons.Menu;
using BT_POS.RepositoryImpl;
using BT_POS.Views.Admin;
using BT_POS.Views.Dialogues;
using BT_POS.Views.Menus;
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
