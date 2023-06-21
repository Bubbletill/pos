using BT_COMMONS.Transactions;
using BT_POS.Views;
using BT_POS.Views.Admin;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace BT_POS.Buttons.Menu;

public class POSMenuButtonGetter
{
    public static IPOSButtonData Get(POSMenuButton button)
    {
        POSController controller = App.AppHost.Services.GetRequiredService<POSController>();

        switch (button)
        {
            case POSMenuButton.ADMIN:
                {
                    return new POSMenuButtonData
                    {
                        Name = "Admin",
                        OnClick = w =>
                        {
                            if (controller.CurrentTransaction != null)
                            {
                                w.HeaderError("Action not allowed. Please suspend the current transaction to access this menu.");
                                return "";
                            }
                            
                            w.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<POSAdmin>();
                            return "";
                        }
                    };
                }

            case POSMenuButton.TENDER:
                {
                    return new POSMenuButtonData
                    {
                        Name = "Tender",
                        OnClick = w =>
                        {
                            if (controller.CurrentTransaction == null)
                            {
                                w.HeaderError("Action not allowed. There is no active transaction.");
                                return "";
                            }

                            return "";
                        }
                    };
                }

            case POSMenuButton.RETURN:
                {
                    return new POSMenuButtonData
                    {
                        Name = "Return",
                        OnClick = w =>
                        {
                            return "";
                        }
                    };
                }

            case POSMenuButton.HOTSHOT:
                {
                    return new POSMenuButtonData
                    {
                        Name = "Hotshot",
                        OnClick = w =>
                        {

                            return "";
                        }
                    };
                }

            case POSMenuButton.ITEM_MOD:
                {
                    return new POSMenuButtonData
                    {
                        Name = "Item Mod",
                        OnClick = w =>
                        {
                            if (controller.CurrentTransaction == null)
                            {
                                w.HeaderError("Action not allowed. There is no active transaction.");
                                return "";
                            }

                            return "";
                        }
                    };
                }

            case POSMenuButton.TRANS_MOD:
                {
                    return new POSMenuButtonData
                    {
                        Name = "Trans Mod",
                        OnClick = w =>
                        {
                            if (controller.CurrentTransaction == null)
                            {
                                w.HeaderError("Action not allowed. There is no active transaction.");
                                return "";
                            }

                            return "";
                        }
                    };
                }

            case POSMenuButton.LOGOUT:
                {
                    return new POSMenuButtonData
                    {
                        Name = "Sign-out",
                        OnClick = w =>
                        {
                            if (controller.CurrentTransaction != null)
                            {
                                w.HeaderError("Action not allowed. Please suspend the current transaction to sign-out.");
                                return "";
                            }

                            w.Logout();
                            return "";
                        }
                    };
                }

            case POSMenuButton.SUSPEND:
                {
                    return new POSMenuButtonData
                    {
                        Name = "Suspend",
                        OnClick = w =>
                        {
                            if (controller.CurrentTransaction == null)
                            {
                                w.HeaderError("Action not allowed. There is no transaction to suspend.");
                                return "";
                            }

                            return "";
                        }
                    };
                }

            case POSMenuButton.RESUME:
                {
                    return new POSMenuButtonData
                    {
                        Name = "Resume",
                        OnClick = w =>
                        {
                            if (controller.CurrentTransaction != null)
                            {
                                w.HeaderError("Action not allowed. Please suspend the current transaction to resume another.");
                                return "";
                            }

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
