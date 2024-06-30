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

namespace BT_POS.Views;

public partial class ResumeView : UserControl
{
    private readonly MainWindow _mainWindow;
    private readonly POSController _controller;
    private readonly ISuspendRepository _suspendRepository;

    private SuspendEntry _selectedTransaction;

    public ResumeView(MainWindow mainWindow, POSController posController, ISuspendRepository suspendRepository)
    {
        _mainWindow = mainWindow;
        _controller = posController;
        _suspendRepository = suspendRepository;

        InitializeComponent();

        Task.Run(() => LoadResumes()).Wait();
    }

    public async void LoadResumes()
    {
        List<SuspendEntry> trans = await _suspendRepository.List(_controller.StoreNumber);
        this.Dispatcher.Invoke(() =>
        {
            ResumeGrid.ItemsSource = trans;
            InfoBox.Information = "Please select the transaction you would like to resume then press Resume.";
            if (trans == null || trans.Count == 0)
            {
                _controller.HeaderError("No suspended transactions found.");
                ResumeButton.IsEnabled = false;
            }
        });
    }

    private void Resume_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTransaction == null)
        {
            _controller.HeaderError("Please select a transaction to resume.");
            return;
        }

        _controller.Resume(_selectedTransaction.Transaction, _selectedTransaction.Sid);
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        _controller.HeaderError();
        _mainWindow.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<HomeView>();
    }

    private void Grid_Change(object sender, SelectionChangedEventArgs e)
    {
        if (ResumeGrid.SelectedItem == null) 
            return;

        _selectedTransaction = ResumeGrid.SelectedItem as SuspendEntry;
    }
}
