using BT_POS.Views.Dialogues;
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

namespace BT_POS.Views.Admin;

public partial class AdminMenuView : UserControl
{
    private readonly MainWindow _mainWindow;
    private readonly POSController _controller;

    public AdminMenuView(MainWindow mainWindow, POSController controller)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        _controller = controller;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        _mainWindow.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<HomeView>();
    }

    private void CloseRegister_Click(object sender, RoutedEventArgs e)
    {
        _mainWindow.POSViewContainer.Content = new YesNoDialogue("Are you sure you want to close this register?", () =>
        {
            _controller.CloseRegister();
        }, () =>
        {
            HomeView hv = App.AppHost.Services.GetRequiredService<HomeView>();
            _mainWindow.POSViewContainer.Content = hv;
        });
    }
}
