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

public partial class EnterLoanView : UserControl
{
    public EnterLoanView(string title, string infoType)
    {
        InitializeComponent();
        Keypad.DisableButton(Keypad.PeriodButton);

        ViewInfoComponent.Title = title;
        ViewInfoComponent.Information += infoType + ".";
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
        HomeView hv = App.AppHost.Services.GetRequiredService<HomeView>();

        mw.POSViewContainer.Content = hv;
    }

    private void Declare_Click(object sender, RoutedEventArgs e)
    {
        var amount = MoneyCounterComponent.CalculateAmount();
        POSController c = App.AppHost.Services.GetRequiredService<POSController>();

        if (amount == 0)
        {
            c.HeaderError("Please enter a value to declare.");
            return;
        }
        c.LoanTransaction(amount);
    }
}
