using BT_COMMONS.Transactions;
using BT_COMMONS.Transactions.TenderAttributes;
using BT_POS.Buttons;
using BT_POS.Buttons.Menu;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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

namespace BT_POS.Views.Tender;

public partial class POSTenderSpecified : UserControl
{
    private readonly TransactionTender _tender;
    private readonly POSController _controller;
    private readonly MainWindow _mainWindow;
    private readonly Style _buttonStyle;

    public POSTenderSpecified(TransactionTender tender, POSController controller, MainWindow mainWindow)
    {
        _tender = tender;
        _controller = controller;
        _mainWindow = mainWindow;

        InitializeComponent();

        BasketComponent.BasketGrid.ItemsSource = _controller.CurrentTransaction!.Basket;

        _buttonStyle = FindResource("BTVerticleButton") as Style;
        UpdateTotals();
        Keypad.SelectedBox = ManualAmountEntryBox;
        ViewInfoComponent.Title = "Tender: " + _tender.GetTenderExternalName();

        AddAmountButton(controller.CurrentTransaction!.GetRemainingTender());

        // Back Button
        Button button = new Button();
        button.Style = _buttonStyle;
        button.Content = "Back";
        button.Click += (s, e) =>
        {
            POSTenderHome tenderHome = App.AppHost.Services.GetRequiredService<POSTenderHome>();
            _mainWindow.POSViewContainer.Content = tenderHome;
        };
        ButtonStackPanel.Children.Add(button);
    }

    private void AddAmountButton(float amount)
    {
        Button button = new Button();
        button.Style = _buttonStyle;
        button.Content = "£" + amount;
        button.Click += (s, e) =>
        {
            AddTender(amount);
        };

        ButtonStackPanel.Children.Add(button);
    }

    public void UpdateTotals()
    {
        TotalTextBlock.Text = "£" + _controller.CurrentTransaction!.GetTotal();
        LeftToTenderTextBlock.Text = "£" + _controller.CurrentTransaction!.GetRemainingTender();
        TenderedTextBlock.Text = "£" + _controller.CurrentTransaction!.GetAmountTendered();
    }

    private void AddTender(float amount)
    {
        _controller.AddTender(_tender, amount);
    }

    private async void ManualAmountEntryBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        float amount;
        try
        {
            amount = float.Parse(ManualAmountEntryBox.Text);
        }
        catch (Exception ex)
        {
            ManualAmountEntryBox.Clear();
            _mainWindow.HeaderError("Invalid price.");
            return;
        }

        if (!_tender.AllowHigherTender() && amount > _controller.CurrentTransaction!.GetRemainingTender())
        {
            ManualAmountEntryBox.Clear();
            _mainWindow.HeaderError("This tender does not accept higher than the outstanding transaction balance.");
            return;
        }

        AddTender(amount);
    }

    private void ManualAmountEntryBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9].+");
        e.Handled = regex.IsMatch(e.Text);
    }
}
