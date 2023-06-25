using BT_POS.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace BT_POS.Buttons;

public class ButtonData : IButtonData
{
    public string Name { get; set; }
    public Action<MainWindow> OnClick { get; set; }
}
