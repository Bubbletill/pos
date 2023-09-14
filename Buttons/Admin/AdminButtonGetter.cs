using BT_COMMONS.Operators;
using BT_POS.Views;
using BT_POS.Views.Admin;
using BT_POS.Views.Dialogues;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT_POS.Buttons.Admin;

public class AdminButtonGetter
{
    public static IButtonData Get(AdminButton button)
    {
        POSController controller = App.AppHost.Services.GetRequiredService<POSController>();
        
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
                            if (controller.CurrentTransaction != null)
                            {
                                w.HeaderError("Action not allowed. Please suspend the current transaction.");
                                return;
                            }

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
                        if (controller.CurrentTransaction != null)
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
                    Name = "Postvoid",
                    Permission = OperatorBoolPermission.POS_Admin_PostVoid,
                    OnClick = w =>
                    {
                        if (controller.CurrentTransaction != null)
                        {
                            w.HeaderError("Action not allowed. Please suspend the current transaction.");
                            return;
                        }

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
                        if (controller.CurrentTransaction != null)
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
                        if (controller.CurrentTransaction != null)
                        {
                            w.HeaderError("Action not allowed. Please suspend the current transaction.");
                            return;
                        }

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
                        if (controller.CurrentTransaction != null)
                        {
                            w.HeaderError("Action not allowed. Please suspend the current transaction.");
                            return;
                        }

                        return;
                    }
                };

            case AdminButton.CLOSE_REGISTER:
                return new ButtonData
                {
                    Name = "Close Register",
                    Permission = OperatorBoolPermission.POS_Admin_CloseRegister,
                    OnClick = w =>
                    {
                        if (controller.CurrentTransaction != null)
                        {
                            w.HeaderError("Action not allowed. Please suspend the current transaction.");
                            return;
                        }

                        w.POSViewContainer.Content = new YesNoDialogue("Are you sure you want to close this register?", () =>
                        {
                            controller.CloseRegister();
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
