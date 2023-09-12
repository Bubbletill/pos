using BT_COMMONS.Operators;
using BT_COMMONS.Transactions;
using BT_COMMONS.Transactions.TenderAttributes;
using BT_POS.Buttons;
using BT_POS.Buttons.Menu;
using BT_POS.Buttons.TransMod;
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

namespace BT_POS.Views.Menus;

public partial class TransModMenuView : UserControl
{
    private readonly POSController _controller;
    private readonly MainWindow _mainWindow;
    private readonly Style _buttonStyle;

    public TransModMenuView(POSController controller, MainWindow mainWindow)
    {
        _controller = controller;
        _mainWindow = mainWindow;
        _buttonStyle = FindResource("BTVerticleButton") as Style;

        InitializeComponent();

        BasketComponent.BasketGrid.ItemsSource = _controller.CurrentTransaction!.Basket;

        LoadButtons(new List<TransModButton>
        { TransModButton.DISCOUNT, TransModButton.VOID });

        // Back 
        Button button = new Button();
        button.Style = _buttonStyle;
        button.Content = "Back";
        button.Click += (s, e) =>
        {
            HomeView home = App.AppHost.Services.GetRequiredService<HomeView>();
            _mainWindow.POSViewContainer.Content = home;
        };
        ButtonStackPanel.Children.Add(button);
    }

    private void LoadButtons(List<TransModButton> buttons)
    {
        ButtonStackPanel.Children.Clear();
        buttons.ForEach(type =>
        {
            IButtonData data = TransModButtonGetter.Get(type);
            Button button = new Button();
            button.Style = _buttonStyle;
            button.Content = data.Name;
            button.Click += (s, e) =>
            {
                if (_controller.CurrentOperator.HasBoolPermission(data.Permission))
                {
                    data.OnClick(_mainWindow);
                }
                else
                {
                    _mainWindow.POSViewContainer.Content = new BoolAuthDialogue((OperatorBoolPermission)data.Permission, () => { data.OnClick(_mainWindow); }, () => { _mainWindow.POSViewContainer.Content = this; });
                }
            };

            ButtonStackPanel.Children.Add(button);
        });
        ButtonStackPanel.InvalidateVisual();
        ButtonStackPanel.UpdateLayout();
    }
}
