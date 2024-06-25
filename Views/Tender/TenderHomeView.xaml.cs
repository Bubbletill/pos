using BT_COMMONS.Transactions;
using BT_COMMONS.Transactions.TenderAttributes;
using BT_POS.Buttons.Menu;
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

namespace BT_POS.Views.Tender;

public partial class TenderHomeView : UserControl
{
    private readonly POSController _controller;
    private readonly MainWindow _mainWindow;
    private readonly Style _buttonStyle;

    public TenderHomeView(POSController controller, MainWindow mainWindow)
    {
        _controller = controller;
        _mainWindow = mainWindow;
        _buttonStyle = FindResource("BTVerticleButton") as Style;

        InitializeComponent();

        List<BasketItem> localBasket = new List<BasketItem>(_controller.CurrentTransaction!.Basket);
        BasketComponent.BasketGrid.ItemsSource = localBasket;
        if (_controller.CurrentTransaction.Tenders.Count != 0)
        {
            localBasket.Add(new BasketItem(0, " ", 0, 0));
            foreach (KeyValuePair<TransactionTender, float> entry in _controller.CurrentTransaction.Tenders)
            {
                localBasket.Add(new BasketItem(0, entry.Key.GetTenderExternalName(), entry.Value, 0));
            }
            if (_controller.CurrentTransaction.Change != 0)
            {
                localBasket.Add(new BasketItem(0, "Change", _controller.CurrentTransaction.Change, 0));
            }
        }

        UpdateTotals();
        LoadButtons(new List<TransactionTender>
        { TransactionTender.CASH, TransactionTender.EXTERNAL_CARD });

        // Back 
        if (_controller.CurrentTransaction!.Tenders.Count == 0)
        {
            Button button = new Button();
            button.Style = _buttonStyle;
            button.Content = "Back";
            button.Click += (s, e) =>
            {
                HomeView home = App.AppHost.Services.GetRequiredService<HomeView>();
                _mainWindow.POSViewContainer.Content = home;
            };
            ButtonStackPanel.Children.Add(button);
        } 
        else
        {
            Button button = new Button();
            button.Style = _buttonStyle;
            button.Content = "Void Tender";
            button.Click += (s, e) =>
            {
                _controller.CurrentTransaction.VoidTender();
                HomeView home = App.AppHost.Services.GetRequiredService<HomeView>();
                _mainWindow.POSViewContainer.Content = home;
            };
            ButtonStackPanel.Children.Add(button);
        }
    }

    private void LoadButtons(List<TransactionTender> buttons)
    {
        ButtonStackPanel.Children.Clear();
        buttons.ForEach(type =>
        {
            Button button = new Button();
            button.Style = _buttonStyle;
            button.Content = type.GetTenderExternalName();
            button.Click += (s, e) =>
            {
                _mainWindow.POSViewContainer.Content = new TenderSpecifiedView(type, _controller, _mainWindow);
            };

            ButtonStackPanel.Children.Add(button);
        });
        ButtonStackPanel.InvalidateVisual();
        ButtonStackPanel.UpdateLayout();
    }

    public void UpdateTotals()
    {
        TotalTextBlock.Text = "£" + _controller.CurrentTransaction!.GetTotal();
        LeftToTenderTextBlock.Text = "£" + _controller.CurrentTransaction!.GetRemainingTender();
        TenderedTextBlock.Text = "£" + _controller.CurrentTransaction!.GetAmountTendered();
    }
}
