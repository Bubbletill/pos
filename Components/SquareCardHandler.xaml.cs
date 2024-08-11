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

    private string checkoutId;

    public SquareCardHandler(string checkoutId)
    {
        InitializeComponent();
        CancelButton.IsEnabled = false;
        this.checkoutId = checkoutId;
    }

    private async void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        CancelButton.IsEnabled = false;
        InfoText.Text = "Cancelling transaction...";
        await App.squareIntegrationData!.Client.TerminalApi.CancelTerminalCheckoutAsync(checkoutId);
    }
}
