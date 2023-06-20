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
        switch (button)
        {
            case POSMenuButton.ADMIN:
                {
                    return new POSMenuButtonData
                    {
                        Name = "Admin",
                        OnClick = w =>
                        {
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
                            POSController controller = App.AppHost.Services.GetRequiredService<POSController>();
                            if (controller.CurrentTransaction == null)
                            {
                                controller.CurrentTransId++;
                                controller.CurrentTransaction = new Transaction();
                                controller.CurrentTransaction.Init(controller.StoreNumber, controller.RegisterNumber, DateOnly.FromDateTime(DateTime.Now), TimeOnly.FromDateTime(DateTime.Now), controller.CurrentTransId, TransactionType.SALE);
                                controller.CurrentTransaction.AddToBasket(new BasketItem
                                {
                                    Code = 9999,
                                    Description = "Example Item",
                                    Quantity = 1,
                                    FilePrice = 2.99f,
                                    SalePrice = 2.99f
                                });
                                POSHome home = App.AppHost.Services.GetRequiredService<POSHome>();
                                home.BasketGrid.ItemsSource = controller.CurrentTransaction.Basket;
                                home.BasketGrid.UpdateLayout();
                                Trace.WriteLine("Return running");
                                Trace.WriteLine(controller.CurrentTransaction.Basket.ToString());
                            }
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
