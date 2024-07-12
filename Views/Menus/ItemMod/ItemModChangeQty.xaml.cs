using BT_COMMONS.Operators;
using BT_COMMONS.Transactions;
using BT_COMMONS.Transactions.TenderAttributes;
using BT_POS.Buttons;
using BT_POS.Buttons.ItemMod;
using BT_POS.Buttons.Menu;
using BT_POS.Buttons.TransMod;
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

namespace BT_POS.Views.Menus.ItemMod;

public partial class ItemModChangeQty : UserControl
{
    private readonly POSController _controller;
    private readonly MainWindow _mainWindow;
    private readonly Style _buttonStyle;

    public ItemModChangeQty(POSController controller, MainWindow mainWindow)
    {
        _controller = controller;
        _mainWindow = mainWindow;
        _buttonStyle = FindResource("BTVerticleButton") as Style;

        InitializeComponent();

        BasketComponent.BasketGrid.ItemsSource = _controller.CurrentTransaction!.Basket;
        QuantityEntryBox.Focus();
        Keypad.DisableButton(Keypad.PeriodButton);
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        Confirm();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        _mainWindow.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<ItemModMenuView>();
    }

    private void Confirm()
    {
        int qty;
        try
        {
            qty = int.Parse(QuantityEntryBox.Text);
            if (qty < 1)
            {
                throw new Exception();
            }
        }
        catch (Exception ex)
        {
            QuantityEntryBox.Clear();
            _mainWindow.HeaderError("Invalid quantity.");
            return;
        }

        _mainWindow.HeaderError(null);
        BasketItem selected = _controller.CurrentTransaction!.SelectedItem;
        selected.Quantity = qty;
        _controller.CurrentTransaction.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Quantity for item " + selected.Code + " - " + selected.Description + " updated to " + qty));
        _mainWindow.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<HomeView>();
    }

    private async void QuantityEntryBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        Confirm();
    }

    private void QuantityEntryBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }
}
