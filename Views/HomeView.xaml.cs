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

public partial class HomeView : UserControl
{
    private readonly MainWindow _mainWindow;
    private readonly POSController _controller;
    private readonly IStockRepository _stockRepository;
    private readonly IButtonRepository _buttonRepository;

    private readonly Style _buttonStyle;

    public HomeView(MainWindow mainWindow, POSController posController, IStockRepository stockRepository, IButtonRepository buttonRepository)
    {
        _mainWindow = mainWindow;
        _controller = posController;
        _stockRepository = stockRepository;
        _buttonRepository = buttonRepository;

        InitializeComponent();
        _buttonStyle = FindResource("BTVerticleButton") as Style;

        if (_controller.CurrentTransaction != null )
        {
            BasketComponent.BasketGrid.ItemsSource = _controller.CurrentTransaction.Basket;
            LoadButtons(App.HomeTransButtons);
            TotalTextBlock.Text = "£" + _controller.CurrentTransaction!.GetTotal();
        } 
        else
        {
            LoadButtons(App.HomeButtons);
        }

        Keypad.SelectedBox = ManualCodeEntryBox;
        ManualCodeEntryBox.Focus();
    }


    public void LoadButtons(List<HomeButton> buttons)
    {
        ButtonStackPanel.Children.Clear();
        buttons.ForEach(type =>
        {
            ButtonStackPanel.Children.Add(App.CreateButton(GetButtonFunction(type), _buttonStyle, this));
        });
        ButtonStackPanel.InvalidateVisual();
        ButtonStackPanel.UpdateLayout();
    }

    public IButtonData GetButtonFunction(HomeButton button)
    {
        switch (button)
        {
            case HomeButton.ADMIN:
                {
                    return new ButtonData
                    {
                        Name = "Admin",
                        Permission = null,
                        OnClick = w =>
                        {
                            if (_controller.CurrentTransaction != null)
                            {
                                w.HeaderError("Action not allowed. Please suspend the current transaction to access this menu.");
                                return;
                            }

                            w.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<AdminMenuView>();
                            return;
                        }
                    };
                }

            case HomeButton.TENDER:
                {
                    return new ButtonData
                    {
                        Name = "Tender",
                        Permission = null,
                        OnClick = w =>
                        {
                            if (_controller.CurrentTransaction == null)
                            {
                                w.HeaderError("Action not allowed. There is no active transaction.");
                                return;
                            }

                            if (_controller.CurrentTransaction.Basket.Count == 0)
                            {
                                w.HeaderError("Action not allowed. The basket is empty.");
                                return;
                            }

                            w.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<TenderHomeView>();
                            return;
                        }
                    };
                }

            case HomeButton.RETURN:
                {
                    return new ButtonData
                    {
                        Name = "Return",
                        Permission = OperatorBoolPermission.POS_Return_Access,
                        OnClick = w =>
                        {
                            return;
                        }
                    };
                }

            case HomeButton.HOTSHOT:
                {
                    return new ButtonData
                    {
                        Name = "Hotshot",
                        Permission = OperatorBoolPermission.POS_Hotshot_Access,
                        OnClick = w =>
                        {

                            return;
                        }
                    };
                }

            case HomeButton.ITEM_MOD:
                {
                    return new ButtonData
                    {
                        Name = "Item Mod",
                        Permission = null,
                        OnClick = w =>
                        {
                            if (_controller.CurrentTransaction == null)
                            {
                                w.HeaderError("Action not allowed. There is no active transaction.");
                                return;
                            }

                            w.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<ItemModMenuView>();
                            return;
                        }
                    };
                }

            case HomeButton.TRANS_MOD:
                {
                    return new ButtonData
                    {
                        Name = "Trans Mod",
                        Permission = null,
                        OnClick = w =>
                        {
                            if (_controller.CurrentTransaction == null)
                            {
                                w.HeaderError("Action not allowed. There is no active transaction.");
                                return;
                            }

                            w.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<TransModMenuView>();
                            return;
                        }
                    };
                }

            case HomeButton.LOGOUT:
                {
                    return new ButtonData
                    {
                        Name = "Sign-out",
                        Permission = null,
                        OnClick = w =>
                        {
                            if (_controller.CurrentTransaction != null)
                            {
                                w.HeaderError("Action not allowed. Please suspend the current transaction to sign-out.");
                                return;
                            }

                            w.Logout();
                            return;
                        }
                    };
                }

            case HomeButton.SUSPEND:
                {
                    return new ButtonData
                    {
                        Name = "Suspend",
                        Permission = null,
                        OnClick = w =>
                        {
                            if (_controller.CurrentTransaction == null)
                            {
                                w.HeaderError("Action not allowed. There is no transaction to suspend.");
                                return;
                            }

                            return;
                        }
                    };
                }

            case HomeButton.RESUME:
                {
                    return new ButtonData
                    {
                        Name = "Resume",
                        Permission = null,
                        OnClick = w =>
                        {
                            if (_controller.CurrentTransaction != null)
                            {
                                w.HeaderError("Action not allowed. Please suspend the current transaction to resume another.");
                                return;
                            }

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

    private async void ManualCodeEntryBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        int code;
        try
        {
            code = Int32.Parse(ManualCodeEntryBox.Text);
        } 
        catch (Exception ex)
        {
            ManualCodeEntryBox.Clear();
            _mainWindow.HeaderError("Invalid item code.");
            return;
        }
        
        BasketItem? item = await _stockRepository.GetItem(code);
        if (item == null)
        {
            ManualCodeEntryBox.Clear();
            _mainWindow.HeaderError("Invalid item code.");
            return;
        }

        _mainWindow.HeaderError();
        if (_controller.CurrentTransaction == null)
        {
            LoadButtons(App.HomeTransButtons);
        }
        _controller.AddItemToBasket(item);
        TotalTextBlock.Text = "£" + _controller.CurrentTransaction!.GetTotal();
        BasketComponent.BasketGrid.ItemsSource = _controller.CurrentTransaction!.Basket;
        BasketComponent.BasketGrid.Items.Refresh();
        ManualCodeEntryBox.Clear();
    }

    private void ManualCodeEntryBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }
}
