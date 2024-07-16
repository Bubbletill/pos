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
                        Name = "Cash Management",
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
            case AdminButton.RECEIPT_REPRINT:
                return new ButtonData
                {
                    Name = "Receipt Reprint",
                    Permission = OperatorBoolPermission.POS_Admin_ReceiptReprint,
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

            case AdminButton.POST_VOID:
                return new ButtonData
                {
                    Name = "Post Void",
                    Permission = OperatorBoolPermission.POS_Admin_PostVoid,
                    OnClick = w =>
                    {
                        if (_controller.CurrentTransaction != null)
                        {
                            w.HeaderError("Action not allowed. Please suspend the current transaction.");
                            return;
                        }

                        _controller.StartTransaction(TransactionType.POST_VOID);
                        w.POSViewContainer.Content = new TransactionInformationDialouge(
                            "Post Void",
                            "Please enter the transaction details for the transaction you would like to post void.",
                            -1, // Store number -1 to force it to current store

                            // Accept
                            (transaction) =>
                            {
                                if (transaction == null)
                                {
                                    _controller.HeaderError("Transaction not found. Please check transation details.");
                                    return;
                                }

                                if (!transaction.PostTransType.CanPostVoid())
                                {
                                    _controller.HeaderError("This transaction can not be post voided.");
                                    return;
                                }

                                w.POSViewContainer.Content = new PostVoidView(transaction);
                            },

                            // Cancel
                            () =>
                            {
                                _controller.CancelTransaction();
                                AdminMenuView av = App.AppHost.Services.GetRequiredService<AdminMenuView>();
                                w.POSViewContainer.Content = av;
                            },
                            true // Admin colours
                            );
                        return;
                    }
                };

            case AdminButton.NO_SALE:
                return new ButtonData
                {
                    Name = "No Sale",
                    Permission = OperatorBoolPermission.POS_Admin_NoSale,
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

            case AdminButton.TRAINING:
                return new ButtonData
                {
                    Name = "Training Mode",
                    Permission = OperatorBoolPermission.POS_Admin_Training,
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

            case AdminButton.X_READ:
                return new ButtonData
                {
                    Name = "X-Read",
                    Permission = OperatorBoolPermission.POS_Admin_XRead,
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

            case AdminButton.CLOSE_REGISTER:
                return new ButtonData
                {
                    Name = "Close Register",
                    Permission = OperatorBoolPermission.POS_Admin_CloseRegister,
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
