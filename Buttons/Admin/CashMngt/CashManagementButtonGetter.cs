using BT_COMMONS.Operators;
using BT_POS.Buttons.Admin.CashMngt;
using BT_POS.Views;
using BT_POS.Views.Admin;
using BT_POS.Views.Dialogues;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT_POS.Buttons.Admin.CashMngt;

public class CashManagementButtonGetter
{
    public static IButtonData Get(CashManagementButton button)
    {
        POSController controller = App.AppHost.Services.GetRequiredService<POSController>();
        
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
                            if (controller.CurrentTransaction != null)
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
                            if (controller.CurrentTransaction != null)
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
                            if (controller.CurrentTransaction != null)
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
