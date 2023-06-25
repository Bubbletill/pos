using BT_COMMONS.Transactions;
using BT_POS.Views;
using BT_POS.Views.Admin;
using BT_POS.Views.Menus;
using BT_POS.Views.Tender;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace BT_POS.Buttons.Menu;

public class HomeButtonGetter
{
    public static IButtonData Get(HomeButton button)
    {
        POSController controller = App.AppHost.Services.GetRequiredService<POSController>();

        switch (button)
        {
            case HomeButton.ADMIN:
                {
                    return new ButtonData
                    {
                        Name = "Admin",
                        OnClick = w =>
                        {
                            if (controller.CurrentTransaction != null)
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
                        OnClick = w =>
                        {
                            if (controller.CurrentTransaction == null)
                            {
                                w.HeaderError("Action not allowed. There is no active transaction.");
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
                        OnClick = w =>
                        {
                            if (controller.CurrentTransaction == null)
                            {
                                w.HeaderError("Action not allowed. There is no active transaction.");
                                return;
                            }

                            return;
                        }
                    };
                }

            case HomeButton.TRANS_MOD:
                {
                    return new ButtonData
                    {
                        Name = "Trans Mod",
                        OnClick = w =>
                        {
                            if (controller.CurrentTransaction == null)
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
                        OnClick = w =>
                        {
                            if (controller.CurrentTransaction != null)
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
                        OnClick = w =>
                        {
                            if (controller.CurrentTransaction == null)
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
                        OnClick = w =>
                        {
                            if (controller.CurrentTransaction != null)
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
}
