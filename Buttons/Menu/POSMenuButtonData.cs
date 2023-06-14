using BT_POS.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace BT_POS.Buttons.Menu;

public class POSMenuButtonData : IPOSButtonData
{
    public string Name { get; set; }
    public UserControl Control { get; set; }
}
