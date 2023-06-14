using BT_POS.Views;
using Microsoft.Extensions.DependencyInjection;

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
                        Control = App.AppHost.Services.GetRequiredService<POSLogin>()
                    };
                }
            default:
                {
                    return null;
                }
        }
    }
}
