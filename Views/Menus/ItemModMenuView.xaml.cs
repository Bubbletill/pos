using BT_COMMONS.Operators;
using BT_COMMONS.Transactions;
using BT_COMMONS.Transactions.TenderAttributes;
using BT_POS.Buttons;
using BT_POS.Buttons.ItemMod;
using BT_POS.Buttons.Menu;
using BT_POS.Buttons.TransMod;
using BT_POS.Views.Dialogues;
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

namespace BT_POS.Views.Menus;

public partial class ItemModMenuView : UserControl
{
    private readonly POSController _controller;
    private readonly MainWindow _mainWindow;
    private readonly Style _buttonStyle;

    public ItemModMenuView(POSController controller, MainWindow mainWindow)
    {
        _controller = controller;
        _mainWindow = mainWindow;
        _buttonStyle = FindResource("BTVerticleButton") as Style;

        InitializeComponent();

        BasketComponent.BasketGrid.ItemsSource = _controller.CurrentTransaction!.Basket;

        LoadButtons(App.ItemModButtons);

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

    private void LoadButtons(List<ItemModButton> buttons)
    {
        ButtonStackPanel.Children.Clear();
        buttons.ForEach(type =>
        {
            ButtonStackPanel.Children.Add(App.CreateButton(GetButtonFunction(type), _buttonStyle, this));
        });
        ButtonStackPanel.InvalidateVisual();
        ButtonStackPanel.UpdateLayout();
    }

    public IButtonData GetButtonFunction(ItemModButton button)
    {
        switch (button)
        {
            case ItemModButton.DISCOUNT:
                {
                    return new ButtonData
                    {
                        Name = "Discount Item",
                        Permission = null,
                        OnClick = w =>
                        {
                            return;
                        }
                    };
                }

            case ItemModButton.VOID:
                {
                    return new ButtonData
                    {
                        Name = "Void Item",
                        Permission = OperatorBoolPermission.POS_ItemMod_ItemVoid,
                        OnClick = w =>
                        {
                            BasketItem bi = _controller.CurrentTransaction!.SelectedItem;
                            if (!_controller.CurrentTransaction.VoidBasketItem(bi))
                            {
                                _controller.HeaderError("Invalid item to void.");
                            }

                            w.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<HomeView>();

                            return;
                        }
                    };
                }
            default:
                {
                    return null;
                }
        }
    }
}
