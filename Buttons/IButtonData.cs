using System;
using System.Windows.Controls;

namespace BT_POS.Buttons;

public interface IButtonData
{
    Action<MainWindow> OnClick { get; set; }
    string Name { get; set; }
}