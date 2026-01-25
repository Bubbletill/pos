using BT_COMMONS.DataRepositories;
using BT_COMMONS.Hotshot;
using BT_COMMONS.Operators;
using BT_COMMONS.Transactions;
using BT_POS.Buttons;
using BT_POS.Buttons.Menu;
using BT_POS.RepositoryImpl;
using BT_POS.Views.Admin;
using BT_POS.Views.Dialogues;
using BT_POS.Views.Menus;
using BT_POS.Views.Tender;
using Microsoft.Extensions.DependencyInjection;
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

namespace BT_POS.Views.Hotshot;

public partial class HotshotView : UserControl
{
    private readonly MainWindow _mainWindow;
    private readonly POSController _controller;
    private readonly List<HotshotCategory> hotshotCategories;

    private readonly Style _buttonStyle;

    private bool insideCategory;

    public HotshotView(MainWindow mainWindow, POSController posController)
    {
        _mainWindow = mainWindow;
        _controller = posController;
        hotshotCategories = App.HotshotCategories;
        _buttonStyle = FindResource("BTHorizontalButton") as Style;
        insideCategory = false;

        InitializeComponent();

        LoadCategories();
    }

    private void LoadCategories()
    {
        insideCategory = false;
        ButtonPanel.Children.Clear();

        InfoBox.Title = "Hotshot";
        InfoBox.Information = "Please select an item category.";

        foreach (HotshotCategory cat in hotshotCategories)
        {
            Button button = new Button();
            button.Style = _buttonStyle;
            button.Content = cat.title;
            button.Click += (s, e) =>
            {
                LoadInsideCategory(cat);
                return;
            };

            ButtonPanel.Children.Add(button);
        }
    }

    private void LoadInsideCategory(HotshotCategory cat)
    {
        insideCategory = true;
        ButtonPanel.Children.Clear();

        InfoBox.Title = cat.title;
        InfoBox.Information = "Please select an item.";

        foreach (HotshotItem item in cat.items)
        {
            Button button = new Button();
            button.Style = _buttonStyle;
            button.Content = item.title;
            button.Click += async (s, e) =>
            {
                BasketItem? bi = await _controller.AddItemToBasket(item.product);
                if (bi == null)
                    return; // as header error will show

                // if not age restricted (or customer is already confirmed of age), return home
                // if this check fails then we do not go home and allow for the age restricted prompt to show
                if (bi.AgeRestricted == 0 || 
                (_controller.CurrentTransaction?.CustomerAge >= bi.AgeRestricted))
                {
                    _mainWindow.POSViewContainer.Content =
                        App.AppHost.Services.GetRequiredService<HomeView>();
                }


                return;
            };

            ButtonPanel.Children.Add(button);
        }
    }


    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (insideCategory)
            LoadCategories();
        else
            _mainWindow.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<HomeView>();
    }
}
