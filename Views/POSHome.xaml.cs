using BT_COMMONS.DataRepositories;
using BT_COMMONS.Transactions;
using BT_POS.Buttons.Menu;
using BT_POS.RepositoryImpl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

namespace BT_POS.Views;

public partial class POSHome : UserControl
{
    private readonly MainWindow _mainWindow;
    private readonly POSController _posController;
    private readonly IStockRepository _stockRepository;
    private readonly IButtonRepository _buttonRepository;

    private readonly Style _buttonStyle;

    public POSHome(MainWindow mainWindow, POSController posController, IStockRepository stockRepository, IButtonRepository buttonRepository)
    {
        _mainWindow = mainWindow;
        _posController = posController;
        _stockRepository = stockRepository;
        _buttonRepository = buttonRepository;

        InitializeComponent();

        if (_posController.CurrentTransaction != null )
        {
            BasketGrid.ItemsSource = _posController.CurrentTransaction.Basket;
        }

        _buttonStyle = FindResource("BTVerticleButton") as Style;
        LoadButtons(App.HomeButtons);

        Keypad.SelectedBox = ManualCodeEntryBox;
    }


    public void LoadButtons(List<POSMenuButton> buttons)
    {
        ButtonStackPanel.Children.Clear();
        buttons.ForEach(type =>
        {
            Button button = new Button();
            button.Style = _buttonStyle;
            button.Content = POSMenuButtonGetter.Get(type).Name;
            button.Click += (s, e) =>
            {
                POSMenuButtonGetter.Get(type).OnClick(_mainWindow);
            };

            ButtonStackPanel.Children.Add(button);
        });
        ButtonStackPanel.InvalidateVisual();
        ButtonStackPanel.UpdateLayout();
    }

    private async void ManualCodeEntryBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        int code;
        try
        {
            code = Int32.Parse(ManualCodeEntryBox.Text);
        } 
        catch (Exception ex)
        {
            ManualCodeEntryBox.Clear();
            _mainWindow.HeaderError("Invalid item code.");
            return;
        }
        
        BasketItem? item = await _stockRepository.GetItem(code);
        if (item == null)
        {
            ManualCodeEntryBox.Clear();
            _mainWindow.HeaderError("Invalid item code.");
            return;
        }

        _mainWindow.HeaderError();
        if (_posController.CurrentTransaction == null)
        {
            LoadButtons(App.HomeTransButtons);
        }
        _posController.AddItemToBasket(item);
        TotalTextBlock.Text = "£" + _posController.CurrentTransaction!.GetTotal();
        BasketGrid.ItemsSource = _posController.CurrentTransaction!.Basket;
        BasketGrid.Items.Refresh();
        ManualCodeEntryBox.Clear();
    }

    private void ManualCodeEntryBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }
}
