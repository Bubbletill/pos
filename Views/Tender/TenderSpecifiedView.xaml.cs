using BT_COMMONS.Transactions;
using BT_COMMONS.Transactions.TenderAttributes;
using BT_POS.Buttons;
using BT_POS.Buttons.Menu;
using BT_POS.Components;
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

public partial class TenderSpecifiedView : UserControl
{
    private readonly TransactionTender _tender;
    private readonly POSController _controller;
    private readonly MainWindow _mainWindow;
    private readonly Style _buttonStyle;

    public TenderSpecifiedView(TransactionTender tender, POSController controller, MainWindow mainWindow)
    {
        _tender = tender;
        _controller = controller;
        _mainWindow = mainWindow;

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
        }

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
            TenderHomeView tenderHome = App.AppHost.Services.GetRequiredService<TenderHomeView>();
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
        switch (_tender)
        {
            case TransactionTender.WORLDPAY_CARD:
                {
                    // temp until logic
                    _controller.AddTender(_tender, amount);
                    return;

                    ToggleReadOnly(true);
                    ViewInfoComponent.Information = "Please follow the instructions in the pop-up window.";
                    return;
                }

            default:
                {
                    _controller.AddTender(_tender, amount);
                    return;
                }
        }
    }

    private void ToggleButtons(bool status)
    {
        foreach (Button item in ButtonStackPanel.Children)
        {
            item.IsEnabled = status;
        }
    }

    private void ToggleReadOnly(bool status)
    {
        ToggleButtons(!status);
        Keypad.Visibility = !status ? Visibility.Visible : Visibility.Hidden;
        ManualAmountEntryBox.Visibility = !status ? Visibility.Visible : Visibility.Hidden;
        ManualAmountLabel.Visibility = !status ? Visibility.Visible : Visibility.Hidden;
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
            _mainWindow.HeaderError("Invalid amount.");
            return;
        }

        if (_controller.CurrentTransaction!.GetTotal() < 0 && amount > _controller.CurrentTransaction!.GetRemainingTender())
        {
            ManualAmountEntryBox.Clear();
            _mainWindow.HeaderError("You cannot give back more money than the amount being returned.");
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
