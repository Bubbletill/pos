using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

public partial class MoneyCounterComponent : UserControl
{
    public MoneyCounterComponent()
    {
        EventManager.RegisterClassHandler(typeof(TextBox), TextBox.TextChangedEvent, new RoutedEventHandler(FocusEvent));
        InitializeComponent();
    }

    private void PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }

    private void FocusEvent(object sender, RoutedEventArgs e)
    {
        TotalTextBlock.Text = "£" + CalculateAmount();
    }

    private float GetAmount(TextBox box)
    {
        if (box.Text == null || box.Text == string.Empty)
        {
            return 0;
        }

        try
        {
            return float.Parse(box.Text);
        } catch
        {
            return 0;
        }
    }

    public float CalculateAmount()
    {
        float amount = 0;
        
        // Notes
        amount += GetAmount(FiftyPoundNotes) * 50;
        amount += GetAmount(FiftyPoundBands) * 2500;

        amount += GetAmount(TwentyPoundNotes) * 20;
        amount += GetAmount(TwentyPoundBands) * 1000;

        amount += GetAmount(TenPoundNotes) * 10;
        amount += GetAmount(TenPoundBands) * 1000;

        amount += GetAmount(FivePoundNotes) * 5;
        amount += GetAmount(FivePoundBands) * 500;

        // Coins
        amount += GetAmount(TwoPoundCoins) * 2;
        amount += GetAmount(TwoPoundRolls) * 20;

        amount += GetAmount(OnePoundCoins) * 1;
        amount += GetAmount(OnePoundRolls) * 20;

        amount += GetAmount(FiftyPenceCoins) * 0.5f;
        amount += GetAmount(FiftyPenceRolls) * 10;

        amount += GetAmount(TwentyPenceCoins) * 0.2f;
        amount += GetAmount(TwentyPenceRolls) * 10;

        amount += GetAmount(TenPenceCoins) * 0.1f;
        amount += GetAmount(TenPenceRolls) * 5;

        amount += GetAmount(FivePenceCoins) * 0.05f;
        amount += GetAmount(FivePenceRolls) * 5;

        amount += GetAmount(TwoPenceCoins) * 0.02f;
        amount += GetAmount(TwoPenceRolls) * 1;

        amount += GetAmount(OnePenceCoins) * 0.01f;
        amount += GetAmount(OnePenceRolls) * 1;

        return amount;
    }
}
