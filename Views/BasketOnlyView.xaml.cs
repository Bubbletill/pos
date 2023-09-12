using BT_COMMONS.Transactions;
using BT_COMMONS.Transactions.TenderAttributes;
using BT_POS.Buttons.Menu;
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

public partial class BasketOnlyView : UserControl
{
    private readonly POSController _controller;
    private readonly MainWindow _mainWindow;
    private readonly Style _buttonStyle;

    public BasketOnlyView(POSController controller, MainWindow mainWindow)
    {
        _controller = controller;
        _mainWindow = mainWindow;

        InitializeComponent();

        List<BasketItem> localBasket = new List<BasketItem>(_controller.CurrentTransaction!.Basket);
        BasketComponent.BasketGrid.ItemsSource = localBasket;
        if (_controller.CurrentTransaction.Tenders.Count != 0)
        {
            localBasket.Add(new BasketItem(0, " ", 0, false));
            foreach (KeyValuePair<TransactionTender, float> entry in _controller.CurrentTransaction.Tenders)
            {
                localBasket.Add(new BasketItem(0, entry.Key.GetTenderExternalName(), entry.Value, false));
            }
            if (_controller.CurrentTransaction.Change != 0)
            {
                localBasket.Add(new BasketItem(0, "Change", _controller.CurrentTransaction.Change, false));
            }
        }
    }
}
