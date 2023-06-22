using BT_COMMONS.Transactions;
using BT_POS.Buttons.Menu;
using BT_POS.Buttons.Tender;
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

public partial class POSTenderHome : UserControl
{
    private readonly POSController _controller;
    private readonly MainWindow _mainWindow;
    private readonly Style _buttonStyle;

    public POSTenderHome(POSController controller, MainWindow mainWindow)
    {
        _controller = controller;
        _mainWindow = mainWindow;
        _buttonStyle = FindResource("BTVerticleButton") as Style;

        InitializeComponent();

        BasketComponent.BasketGrid.ItemsSource = _controller.CurrentTransaction!.Basket;

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
                POSHome home = App.AppHost.Services.GetRequiredService<POSHome>();
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
                POSHome home = App.AppHost.Services.GetRequiredService<POSHome>();
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
            button.Content = POSTenderButtonGertter.Get(type).Name;
            button.Click += (s, e) =>
            {
                POSTenderButtonGertter.Get(type).OnClick(_mainWindow);
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
