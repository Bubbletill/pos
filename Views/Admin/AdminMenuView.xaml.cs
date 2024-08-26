using BT_COMMONS.Operators;
using BT_COMMONS.Transactions;
using BT_COMMONS.Transactions.TypeAttributes;
using BT_POS.Buttons;
using BT_POS.Buttons.Admin;
using BT_POS.Buttons.Menu;
using BT_POS.Components;
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

    private readonly Style _buttonStyle;

    public AdminMenuView(MainWindow mainWindow, POSController controller)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        _controller = controller;

        InfoComponent.SetAdminColour();

        _buttonStyle = FindResource("BTVerticleButton") as Style;

        LoadButtons(App.AdminButtons);
        Button button = new Button();
        button.Style = _buttonStyle;
        button.Content = "Back";
        button.Click += (s, e) =>
        {
            _mainWindow.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<HomeView>();
        };
        ButtonStackPanel.Children.Add(button);
    }

    public void LoadButtons(List<AdminButton> buttons)
    {
        ButtonStackPanel.Children.Clear();
        buttons.ForEach(type =>
        {
            ButtonStackPanel.Children.Add(App.CreateButton(GetButtonFunction(type), _buttonStyle, this));
        });
        ButtonStackPanel.InvalidateVisual();
        ButtonStackPanel.UpdateLayout();
    }

    public IButtonData GetButtonFunction(AdminButton button)
    {

        switch (button)
        {
            case AdminButton.CASH_MANAGEMENT:
                {
                    return new ButtonData
                    {
                        Name = "Cash",
                        Permission = null,
                        OnClick = w =>
                        {
                            if (_controller.CurrentTransaction != null)
                            {
                                w.HeaderError("Action not allowed. Please suspend the current transaction.");
                                return;
                            }

                            w.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<AdminCashManagementMenuView>();
                            return;
                        }
                    };
                }

            case AdminButton.TRXN_MANAGEMENT:
                {
                    return new ButtonData
                    {
                        Name = "Transaction",
                        Permission = null,
                        OnClick = w =>
                        {
                            if (_controller.CurrentTransaction != null)
                            {
                                w.HeaderError("Action not allowed. Please suspend the current transaction.");
                                return;
                            }

                            w.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<AdminTrxnManagementMenuView>();
                            return;
                        }
                    };
                }

            case AdminButton.REG_MANAGEMENT:
                {
                    return new ButtonData
                    {
                        Name = "Register",
                        Permission = null,
                        OnClick = w =>
                        {
                            if (_controller.CurrentTransaction != null)
                            {
                                w.HeaderError("Action not allowed. Please suspend the current transaction.");
                                return;
                            }

                            w.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<AdminRegManagementMenuView>();
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
