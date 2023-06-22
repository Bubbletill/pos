using BT_POS.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace BT_POS.Buttons;

public class POSButtonData : IPOSButtonData
{
    public string Name { get; set; }
    public Func<MainWindow, string> OnClick { get; set; }
}
