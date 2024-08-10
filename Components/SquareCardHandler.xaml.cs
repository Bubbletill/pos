using BT_COMMONS.Transactions;
using BT_POS.Views.Tender;
using Microsoft.Extensions.DependencyInjection;
using Square.Apis;
using Square.Exceptions;
using Square.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace BT_POS.Components;

/// <summary>
/// Interaction logic for InfoPopup.xaml
/// </summary>
public partial class SquareCardHandler : Window
{

    private string _referenceId;
    private ITerminalApi _terminalApi;
    private readonly MainWindow _mainWindow;
    private readonly POSController _controller;
    private string _error = "NA";

    public SquareCardHandler()
    {
        InitializeComponent();
        RefreshButton.IsEnabled = false;
        CancelButton.IsEnabled = false;
        _mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
        _controller = App.AppHost.Services.GetRequiredService<POSController>();
    }

    public async Task Start(long amount)
    {
        this.Show();
        try
        {
            if (App.squareIntegrationData == null || App.squareIntegrationData.Client == null)
            {
                Error("Client failed to initalise.");
                return;
            }
            _terminalApi = App.squareIntegrationData.Client.TerminalApi;

            CreateTerminalCheckoutRequest body = new CreateTerminalCheckoutRequest.Builder(
                Guid.NewGuid().ToString(),
                new TerminalCheckout.Builder(
                    new Money.Builder()
                        .Amount(amount)
                        .Currency("GBP")
                    .Build(),
                    new DeviceCheckoutOptions.Builder(App.squareIntegrationData.TerminalDeviceId).Build()
                    )
                .Build()
                ).Build();

            CreateTerminalCheckoutResponse result = await _terminalApi.CreateTerminalCheckoutAsync(body);
            _referenceId = result.Checkout.Id;
            InfoText.Text = "Sent. Please follow the instructions on the terminal.\nLast Status: " + result.Checkout.Status;
            RefreshButton.IsEnabled = true;
            CancelButton.IsEnabled = true;
            return;
        }
        catch (ApiException ex)
        {
            Error(ex.Message);
            Trace.WriteLine(ex);
        }
    }

    private void Error(string message) 
    {
        _error = "SQAPI Error: " + message;
        this.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0x00));
        this.AboveText.FontSize = 30;
        this.AboveText.Text = "Transaction Failed";
        InfoText.Text = message;
        CancelButton.IsEnabled = true;
        CancelButton.Content = "Back";
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            GetTerminalCheckoutResponse result = await _terminalApi.GetTerminalCheckoutAsync(_referenceId);

            if (result.Checkout.Status == "COMPLETED")
            {
                //_controller.AddTender(_tender, amount);
            }

        } catch (ApiException ex)
        {
            _controller.HeaderError("SQAPI Error: " + ex.Message);
            return;
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        if (_error != "NA")
        {
            _mainWindow.POSViewContainer.Content = new TenderSpecifiedView(TransactionTender.SQUARE_CARD, _controller, _mainWindow);
            _controller.HeaderError(_error);
            this.Close();
        }
    }
}
