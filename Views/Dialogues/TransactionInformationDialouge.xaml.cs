using BT_COMMONS.DataRepositories;
using BT_COMMONS.Transactions;
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
using static System.Net.Mime.MediaTypeNames;

namespace BT_POS.Views.Dialogues;

/// <summary>
/// Interaction logic for TransactionInformationDialouge.xaml
/// </summary>
public partial class TransactionInformationDialouge : UserControl
{
    private readonly POSController _controller;
    private readonly ITransactionRepository _transactionRepository;
    private readonly Action<Transaction?> _onAccept;
    private readonly Action _onCancel;
    private readonly int _store;

    public TransactionInformationDialouge(string title, string text, int store, Action<Transaction?> onYes, Action onNo, bool admin = false)
    {
        InitializeComponent();
        _controller = App.AppHost.Services.GetRequiredService<POSController>();
        _transactionRepository = App.AppHost.Services.GetRequiredService<ITransactionRepository>();
        _onAccept = onYes;
        _onCancel = onNo;
        _store = store;

        if (store == -1)
        {
            StoreNumber.IsEnabled = false;
            StoreNumber.Text = _controller.StoreNumber + "";
        }

        if (admin)
            ViewInfoComponent.SetAdminColour();

        ViewInfoComponent.Title = title;
        ViewInfoComponent.Information = text;
    }


    private async void Accept_Click(object sender, RoutedEventArgs e)
    {
        if (StoreNumber.Text.Length == 0 || RegisterNumber.Text.Length == 0 || TransactionNumber.Text.Length == 0)
        {
            _controller.HeaderError("Please complete all fields.");
            return;
        }
        try
        {
            DateOnly date = DateOnly.Parse(Date.Text);
        } catch (Exception ex)
        {
            _controller.HeaderError("Please complete all fields.");
            return;
        }

        Transaction? transaction = await _transactionRepository.GetTransaction(int.Parse(StoreNumber.Text), int.Parse(RegisterNumber.Text), int.Parse(TransactionNumber.Text), DateOnly.Parse(Date.Text));
        _onAccept(transaction);
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        _onCancel();
    }

    private void PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }
}
