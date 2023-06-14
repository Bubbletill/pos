using System.Windows.Controls;

namespace BT_POS.Buttons
{
    public interface IPOSButtonData
    {
        UserControl Control { get; set; }
        string Name { get; set; }
    }
}