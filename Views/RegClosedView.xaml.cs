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

namespace BT_POS.Views;

public partial class RegClosedView : UserControl
{
    private readonly LoginView _loginView;

    public RegClosedView(LoginView loginView)
    {
        InitializeComponent();
        _loginView = loginView;
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        POSController pc = App.AppHost.Services.GetRequiredService<POSController>();
        pc.HeaderError(null);
        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
        mw.POSViewContainer.Content = _loginView;
    }

    private void BackOffice_Click(object sender, RoutedEventArgs e)
    {

    }
}
