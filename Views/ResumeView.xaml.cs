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

    private readonly Style _buttonStyle;

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
        List<Transaction> trans = await _suspendRepository.List(_controller.StoreNumber);
        this.Dispatcher.Invoke(() =>
        {
            ResumeGrid.ItemsSource = trans;
            InfoBox.Information = "Please select the transaction you would like to resume then press Resume.";
        });
    }

    private void Resume_Click(object sender, RoutedEventArgs e)
    {

    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        _mainWindow.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<HomeView>();
    }
}
