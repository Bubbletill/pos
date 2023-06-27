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

public partial class EnterFloatView : UserControl
{
    public EnterFloatView()
    {
        InitializeComponent();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
        HomeView hv = App.AppHost.Services.GetRequiredService<HomeView>();

        mw.POSViewContainer.Content = hv;
    }

    private void Declare_Click(object sender, RoutedEventArgs e)
    {

    }
}
