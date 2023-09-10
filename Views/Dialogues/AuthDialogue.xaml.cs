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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BT_POS.Views.Dialogues;

/// <summary>
/// Interaction logic for AuthDialogue.xaml
/// </summary>
public partial class AuthDialogue : UserControl
{
    private readonly Action _onYes;
    private readonly Action _onNo;

    public AuthDialogue(string text, Action onYes, Action onNo)
    {
        InitializeComponent();
        _onYes = onYes;
        _onNo = onNo;

        ViewInfo.VICRoot.Information = text;
    }


    private void Yes_Click(object sender, RoutedEventArgs e)
    {
        _onYes();
    }

    private void No_Click(object sender, RoutedEventArgs e)
    {
        _onNo();
    }
}
