﻿using BT_COMMONS.Operators;
using BT_COMMONS.Transactions;
using BT_POS.Views;
using BT_POS.Views.Admin;
using BT_POS.Views.Dialogues;
using BT_POS.Views.Menus;
using BT_POS.Views.Tender;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace BT_POS.Buttons.TransMod;

public class TransModButtonGetter
{
    public static IButtonData Get(TransModButton button)
    {
        POSController controller = App.AppHost.Services.GetRequiredService<POSController>();

        switch (button)
        {
            case TransModButton.DISCOUNT:
                {
                    return new ButtonData
                    {
                        Name = "Discount Transaction",
                        Permission = null,
                        OnClick = w =>
                        {
                            return;
                        }
                    };
                }

            case TransModButton.VOID:
                {
                    return new ButtonData
                    {
                        Name = "Void Transaction",
                        Permission = OperatorBoolPermission.POS_TransMod_TransVoid,
                        OnClick = w =>
                        {
                            w.POSViewContainer.Content = new YesNoDialogue("Are you sure you want to void this transaction?", () =>
                            {
                                // Yes
                                controller.VoidTransaction();
                            }, () =>
                            {
                                // No
                                w.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<TransModMenuView>();
                            });
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
