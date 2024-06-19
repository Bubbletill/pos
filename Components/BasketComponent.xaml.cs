using BT_COMMONS.Transactions;
using Microsoft.Extensions.DependencyInjection;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BT_POS.Components
{
    /// <summary>
    /// Interaction logic for BasketComponent.xaml
    /// </summary>
    public partial class BasketComponent : UserControl
    {

        public BasketItem SelectedItem { get; set; }

        public BasketComponent()
        {
            InitializeComponent();
        }

        private void BasketGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BasketGrid.SelectedItem == null)
                return;

            App.AppHost.Services.GetRequiredService<POSController>().CurrentTransaction.SelectedItem = BasketGrid.SelectedItem as BasketItem;
        }
    }
}
