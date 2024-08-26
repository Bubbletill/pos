using BT_COMMONS.Operators;
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

public partial class AdminRegManagementMenuView : UserControl
{
    private readonly MainWindow _mainWindow;
    private readonly POSController _controller;

    private readonly Style _buttonStyle;

    public AdminRegManagementMenuView(MainWindow mainWindow, POSController controller)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        _controller = controller;

        InfoComponent.SetAdminColour();

        _buttonStyle = FindResource("BTVerticleButton") as Style;

        LoadButtons(App.AdminRegManagementButtons);
        Button button = new Button();
        button.Style = _buttonStyle;
        button.Content = "Back";
        button.Click += (s, e) =>
        {
            _mainWindow.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<AdminMenuView>(); ;
        };
        ButtonStackPanel.Children.Add(button);
    }

    public void LoadButtons(List<AdminRegMngmtButton> buttons)
    {
        ButtonStackPanel.Children.Clear();
        buttons.ForEach(type =>
        {
            ButtonStackPanel.Children.Add(App.CreateButton(GetButtonFunction(type), _buttonStyle, this));
        });
        ButtonStackPanel.InvalidateVisual();
        ButtonStackPanel.UpdateLayout();
    }

    public IButtonData GetButtonFunction(AdminRegMngmtButton button)
    {
        switch (button)
        {
            case AdminRegMngmtButton.TRAINING:
                return new ButtonData
                {
                    Name = "Training Mode",
                    Permission = OperatorBoolPermission.POS_Admin_RegManagement_Training,
                    OnClick = w =>
                    {
                        if (_controller.CurrentTransaction != null)
                        {
                            w.HeaderError("Action not allowed. Please suspend the current transaction.");
                            return;
                        }

                        _controller.TrainingModeToggleTransaction();
                        return;
                    }
                };

            case AdminRegMngmtButton.X_READ:
                return new ButtonData
                {
                    Name = "X-Read",
                    Permission = OperatorBoolPermission.POS_Admin_RegManagement_XRead,
                    OnClick = w =>
                    {
                        if (_controller.CurrentTransaction != null)
                        {
                            w.HeaderError("Action not allowed. Please suspend the current transaction.");
                            return;
                        }

                        _controller.RegisterXRead();
                    }
                };

            case AdminRegMngmtButton.CLOSE_REGISTER:
                return new ButtonData
                {
                    Name = "Close Register",
                    Permission = OperatorBoolPermission.POS_Admin_RegManagement_CloseRegister,
                    OnClick = w =>
                    {
                        if (_controller.CurrentTransaction != null)
                        {
                            w.HeaderError("Action not allowed. Please suspend the current transaction.");
                            return;
                        }

                        if (_controller.TrainingMode)
                        {
                            w.HeaderError("Action not allowed. Please disable training mode.");
                            return;
                        }

                        w.POSViewContainer.Content = new YesNoDialogue("Are you sure you want to close this register?", () =>
                        {
                            _controller.CloseRegister();
                        }, () =>
                        {
                            AdminMenuView av = App.AppHost.Services.GetRequiredService<AdminMenuView>();
                            w.POSViewContainer.Content = av;
                        });

                        return;
                    }
                };

            default:
                {
                    return null;
                }
        }
    }
}
