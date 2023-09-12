using BT_COMMONS.DataRepositories;
using BT_COMMONS.Operators;
using BT_COMMONS.Transactions;
using BT_POS.Buttons;
using BT_POS.Buttons.Menu;
using BT_POS.RepositoryImpl;
using BT_POS.Views.Dialogues;
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

public partial class HomeView : UserControl
{
    private readonly MainWindow _mainWindow;
    private readonly POSController _posController;
    private readonly IStockRepository _stockRepository;
    private readonly IButtonRepository _buttonRepository;

    private readonly Style _buttonStyle;

    public HomeView(MainWindow mainWindow, POSController posController, IStockRepository stockRepository, IButtonRepository buttonRepository)
    {
        _mainWindow = mainWindow;
        _posController = posController;
        _stockRepository = stockRepository;
        _buttonRepository = buttonRepository;

        InitializeComponent();
        _buttonStyle = FindResource("BTVerticleButton") as Style;

        if (_posController.CurrentTransaction != null )
        {
            BasketComponent.BasketGrid.ItemsSource = _posController.CurrentTransaction.Basket;
            LoadButtons(App.HomeTransButtons);
            TotalTextBlock.Text = "£" + _posController.CurrentTransaction!.GetTotal();
        } 
        else
        {
            LoadButtons(App.HomeButtons);
        }

        Keypad.SelectedBox = ManualCodeEntryBox;
        ManualCodeEntryBox.Focus();
    }


    public void LoadButtons(List<HomeButton> buttons)
    {
        ButtonStackPanel.Children.Clear();
        buttons.ForEach(type =>
        {
            IButtonData data = HomeButtonGetter.Get(type);
            Button button = new Button();
            button.Style = _buttonStyle;
            button.Content = data.Name;
            button.Click += (s, e) =>
            {
                if (_posController.CurrentOperator.HasBoolPermission(data.Permission))
                {
                    data.OnClick(_mainWindow);
                } 
                else
                {
                    _mainWindow.POSViewContainer.Content = new BoolAuthDialogue((OperatorBoolPermission)data.Permission, () => { data.OnClick(_mainWindow); }, () => { _mainWindow.POSViewContainer.Content = this; });
                }
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
        BasketComponent.BasketGrid.ItemsSource = _posController.CurrentTransaction!.Basket;
        BasketComponent.BasketGrid.Items.Refresh();
        ManualCodeEntryBox.Clear();
    }

    private void ManualCodeEntryBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }
}
