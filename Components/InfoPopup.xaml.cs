using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BT_POS.Components;

/// <summary>
/// Interaction logic for InfoPopup.xaml
/// </summary>
public partial class InfoPopup : Window
{
    public InfoPopup(string text)
    {
        InitializeComponent();
        InfoText.Text = text;
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
