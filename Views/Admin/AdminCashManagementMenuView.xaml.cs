using BT_COMMONS.Operators;
using BT_POS.Buttons;
using BT_POS.Buttons.Admin;
using BT_POS.Buttons.Admin.CashMngt;
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

public partial class AdminCashManagementMenuView : UserControl
{
    private readonly MainWindow _mainWindow;
    private readonly POSController _controller;

    private readonly Style _buttonStyle;

    public AdminCashManagementMenuView(MainWindow mainWindow, POSController controller)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        _controller = controller;

        InfoComponent.MainBox.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xe0, 0xe0));

        _buttonStyle = FindResource("BTVerticleButton") as Style;

        LoadButtons(App.AdminCashManagementButtons);
        Button button = new Button();
        button.Style = _buttonStyle;
        button.Content = "Back";
        button.Click += (s, e) =>
        {
            _mainWindow.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<AdminMenuView>(); ;
        };
        ButtonStackPanel.Children.Add(button);
    }

    public void LoadButtons(List<CashManagementButton> buttons)
    {
        ButtonStackPanel.Children.Clear();
        buttons.ForEach(type =>
        {
            ButtonStackPanel.Children.Add(App.CreateButton(GetButtonFunction(type), _buttonStyle, this));
        });
        ButtonStackPanel.InvalidateVisual();
        ButtonStackPanel.UpdateLayout();
    }

    public IButtonData GetButtonFunction(CashManagementButton button)
    {
        switch (button)
        {
            case CashManagementButton.LOAN:
                {
                    return new ButtonData
                    {
                        Name = "Loan",
                        Permission = OperatorBoolPermission.POS_Admin_CashManagement_Loan,
                        OnClick = w =>
                        {
                            if (_controller.CurrentTransaction != null)
                            {
                                w.HeaderError("Action not allowed. Please suspend the current transaction.");
                                return;
                            }

                            w.POSViewContainer.Content = new EnterLoanView("Loan", "the loan");
                        }
                    };
                }
            case CashManagementButton.SPOT_CHECK:
                {
                    return new ButtonData
                    {
                        Name = "Spot Check",
                        Permission = OperatorBoolPermission.POS_Admin_CashManagement_Spotcheck,
                        OnClick = w =>
                        {
                            if (_controller.CurrentTransaction != null)
                            {
                                w.HeaderError("Action not allowed. Please suspend the current transaction.");
                                return;
                            }

                            return;
                        }
                    };
                }
            case CashManagementButton.PICKUP:
                {
                    return new ButtonData
                    {
                        Name = "Pickup",
                        Permission = OperatorBoolPermission.POS_Admin_CashManagement_Pickup,
                        OnClick = w =>
                        {
                            if (_controller.CurrentTransaction != null)
                            {
                                w.HeaderError("Action not allowed. Please suspend the current transaction.");
                                return;
                            }

                            return;
                        }
                    };
                };

            default:
                {
                    return null;
                }
        }
    }
}
