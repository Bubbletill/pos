using System;
using System.Windows.Controls;

namespace BT_POS.Buttons;

public interface IPOSButtonData
{
    Func<MainWindow, string> OnClick { get; set; }
    string Name { get; set; }
}