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
using Xceed.Wpf.Toolkit;

namespace BT_POS.Components;

public partial class KeypadComponent : UserControl
{

    public TextBox? SelectedBox { get; set; }
    public MaskedTextBox? SelectedMaskedBox { get; set; }
    public PasswordBox? SelectedPasswordBox { get; set; }


    public KeypadComponent()
    {
        EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotFocusEvent, new RoutedEventHandler(TextBoxSelect));
        EventManager.RegisterClassHandler(typeof(MaskedTextBox), MaskedTextBox.GotFocusEvent, new RoutedEventHandler(MaskedTextBoxSelect));
        EventManager.RegisterClassHandler(typeof(PasswordBox), PasswordBox.GotFocusEvent, new RoutedEventHandler(PasswordBoxSelect));
        SelectedBox = null;
        SelectedMaskedBox = null;
        SelectedPasswordBox = null;
        InitializeComponent();
    }

    public void DisableButton(Button button)
    {
        button.IsEnabled = false;
        button.Style = FindResource("BTDisabledKeypadButton") as Style;
    }

    private void TextBoxSelect(object sender, RoutedEventArgs e)
    {
        SelectedBox = (TextBox)sender;
        SelectedMaskedBox = null;
        SelectedPasswordBox = null;
    }

    private void MaskedTextBoxSelect(object sender, RoutedEventArgs e)
    {
        SelectedMaskedBox = (MaskedTextBox)sender;
        SelectedBox = null;
        SelectedPasswordBox = null;
    }

    private void PasswordBoxSelect(object sender, RoutedEventArgs e)
    {
        SelectedPasswordBox = (PasswordBox)sender;
        SelectedBox = null;
        SelectedMaskedBox = null;
    }

    private void AddToBox(char toAdd)
    {
        if (SelectedMaskedBox != null)
        {
            SelectedMaskedBox.PromptChar += toAdd;
        }
        else if (SelectedBox != null)
        {
             SelectedBox.Text += toAdd;
        }
        else if (SelectedPasswordBox != null)
        {
            SelectedPasswordBox.Password += toAdd;
        }
    }

    private void SevenButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox('7');
    }

    private void EightButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox('8');
    }

    private void NineButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox('9');
    }

    private void FourButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox('4');
    }

    private void FiveButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox('5');
    }

    private void SixButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox('6');
    }

    private void OneButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox('1');
    }

    private void TwoButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox('2');
    }

    private void ThreeButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox('3');
    }

    private void ZeroButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox('0');
    }

    private void PeriodButton_Click(object sender, RoutedEventArgs e)
    {
        AddToBox('.');
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedBox != null)
            SelectedBox.Text = BackText(SelectedBox.Text);
        else if (SelectedMaskedBox != null)
            SelectedMaskedBox.Text = BackText(SelectedMaskedBox.Text);
        else if (SelectedPasswordBox != null)
            SelectedPasswordBox.Password = BackText(SelectedPasswordBox.Password);
    }

    private string BackText(string s)
    {
        if (s.Length > 1)
        {
            s = s.Substring(0, s.Length - 1);
        }
        else
        {
            s = "";
        }

        return s;
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedBox != null)
            SelectedBox.Clear();
        else if (SelectedMaskedBox != null)
            SelectedMaskedBox.Clear();
        else if (SelectedPasswordBox != null)
            SelectedPasswordBox.Clear();
    }

    private void EnterButton_Click(object sender, RoutedEventArgs e)
    {
        var routedEvent = Keyboard.KeyDownEvent;
        var keyEvent = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(this), 0, Key.Enter)
        {
            RoutedEvent = routedEvent
        };

        if (SelectedBox != null)
            SelectedBox.RaiseEvent(keyEvent);
        else if (SelectedMaskedBox != null)
            SelectedMaskedBox.RaiseEvent(keyEvent);
        else if (SelectedPasswordBox != null)
            SelectedPasswordBox.RaiseEvent(keyEvent);
    }
}
