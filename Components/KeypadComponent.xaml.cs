using Org.BouncyCastle.Asn1.X509;
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

namespace BT_POS.Components;

public partial class KeypadComponent : UserControl
{

    public TextBox? SelectedBox { get; set; }


    public KeypadComponent()
    {
        EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotFocusEvent, new RoutedEventHandler(TextBoxSelect));
        SelectedBox = null;
        InitializeComponent();
    }

    private void TextBoxSelect(object sender, RoutedEventArgs e)
    {
        SelectedBox = (TextBox)sender;
    }

    private void AddToBox(string toAdd)
    {
        if (SelectedBox == null)
            return;

        SelectedBox.Text += toAdd;
    }

    private void SevenButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox("7");
    }

    private void EightButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox("8");
    }

    private void NineButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox("9");
    }

    private void FourButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox("4");
    }

    private void FiveButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox("5");
    }

    private void SixButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox("6");
    }

    private void OneButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox("1");
    }

    private void TwoButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox("2");
    }

    private void ThreeButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox("3");
    }

    private void ZeroButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox("0");
    }

    private void PeriodButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox(".");
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedBox == null)
            return;

        string s = SelectedBox.Text;

        if (s.Length > 1)
        {
            s = s.Substring(0, s.Length - 1);
        }
        else
        {
            s = "";
        }

        SelectedBox.Text = s;
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedBox == null)
            return;

        SelectedBox.Clear();
    }

    private void EnterButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedBox == null)
            return;

        var routedEvent = Keyboard.KeyDownEvent;
        var keyEvent = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(this), 0, Key.Enter)
        {
            RoutedEvent = routedEvent
        };

        SelectedBox.RaiseEvent(keyEvent);
    }
}
