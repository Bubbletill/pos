using BT_COMMONS.Transactions;
using BT_POS.Buttons.Menu;
using BT_POS.Views.Admin;
using BT_POS.Views.Tender;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT_POS.Buttons.Tender;

public class POSTenderButtonGertter
{
    public static IPOSButtonData Get(TransactionTender button)
    {
        POSController controller = App.AppHost.Services.GetRequiredService<POSController>();

        switch (button)
        {
            case TransactionTender.CASH:
                {
                    return new POSButtonData
                    {
                        Name = "Cash",
                        OnClick = w =>
                        {
                            w.POSViewContainer.Content = new POSTenderSpecified(TransactionTender.CASH, controller, w);
                            return "";
                        }
                    };
                }

            case TransactionTender.EXTERNAL_CARD:
                {
                    return new POSButtonData
                    {
                        Name = "Card",
                        OnClick = w =>
                        {
                            w.POSViewContainer.Content = new POSTenderSpecified(TransactionTender.EXTERNAL_CARD, controller, w);
                            return "";
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
