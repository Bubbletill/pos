using BT_COMMONS.Operators;
using BT_COMMONS.Transactions.TypeAttributes;
using BT_COMMONS.Transactions;
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

public partial class AdminTrxnManagementMenuView : UserControl
{
    private readonly MainWindow _mainWindow;
    private readonly POSController _controller;

    private readonly Style _buttonStyle;

    public AdminTrxnManagementMenuView(MainWindow mainWindow, POSController controller)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        _controller = controller;

        InfoComponent.SetAdminColour();

        _buttonStyle = FindResource("BTVerticleButton") as Style;

        LoadButtons(App.AdminTrxnManagementButtons);
        Button button = new Button();
        button.Style = _buttonStyle;
        button.Content = "Back";
        button.Click += (s, e) =>
        {
            _mainWindow.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<AdminMenuView>(); ;
        };
        ButtonStackPanel.Children.Add(button);
    }

    public void LoadButtons(List<AdminTrxnMngmtButton> buttons)
    {
        ButtonStackPanel.Children.Clear();
        buttons.ForEach(type =>
        {
            ButtonStackPanel.Children.Add(App.CreateButton(GetButtonFunction(type), _buttonStyle, this));
        });
        ButtonStackPanel.InvalidateVisual();
        ButtonStackPanel.UpdateLayout();
    }

    public IButtonData GetButtonFunction(AdminTrxnMngmtButton button)
    {
        switch (button)
        {
            case AdminTrxnMngmtButton.RECEIPT_REPRINT:
                return new ButtonData
                {
                    Name = "Receipt Reprint",
                    Permission = OperatorBoolPermission.POS_Admin_TrxnManagement_ReceiptReprint,
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

            case AdminTrxnMngmtButton.POST_VOID:
                return new ButtonData
                {
                    Name = "Post Void",
                    Permission = OperatorBoolPermission.POS_Admin_TrxnManagement_PostVoid,
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
                                    _controller.HeaderError("This transaction cannot be post voided.");
                                    return;
                                }

                                bool canPv = true;
                                foreach (TransactionTender tender in transaction.Tenders.Keys)
                                {
                                    if (!tender.CanPostVoid())
                                        canPv = false;
                                }

                                if (!canPv)
                                {
                                    _controller.HeaderError("This transaction contains a tender that cannot be post voided.");
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

            default:
                {
                    return null;
                }
        }
    }
}
