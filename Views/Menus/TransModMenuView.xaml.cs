﻿using BT_COMMONS.Operators;
using BT_COMMONS.Transactions;
using BT_COMMONS.Transactions.TenderAttributes;
using BT_POS.Buttons;
using BT_POS.Buttons.Menu;
using BT_POS.Buttons.TransMod;
using BT_POS.Views.Dialogues;
using Microsoft.Extensions.DependencyInjection;
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

namespace BT_POS.Views.Menus;

public partial class TransModMenuView : UserControl
{
    private readonly POSController _controller;
    private readonly MainWindow _mainWindow;
    private readonly Style _buttonStyle;

    public TransModMenuView(POSController controller, MainWindow mainWindow)
    {
        _controller = controller;
        _mainWindow = mainWindow;
        _buttonStyle = FindResource("BTVerticleButton") as Style;

        InitializeComponent();

        BasketComponent.BasketGrid.ItemsSource = _controller.CurrentTransaction!.Basket;

        LoadButtons(App.TransModButtons);

        // Back 
        Button button = new Button();
        button.Style = _buttonStyle;
        button.Content = "Back";
        button.Click += (s, e) =>
        {
            HomeView home = App.AppHost.Services.GetRequiredService<HomeView>();
            _mainWindow.POSViewContainer.Content = home;
        };
        ButtonStackPanel.Children.Add(button);
    }

    private void LoadButtons(List<TransModButton> buttons)
    {
        ButtonStackPanel.Children.Clear();
        buttons.ForEach(type =>
        {
            ButtonStackPanel.Children.Add(App.CreateButton(GetButtonFunction(type), _buttonStyle, this));
        });
        ButtonStackPanel.InvalidateVisual();
        ButtonStackPanel.UpdateLayout();
    }

    public IButtonData GetButtonFunction(TransModButton button)
    {
        switch (button)
        {
            case TransModButton.DISCOUNT:
                {
                    return new ButtonData
                    {
                        Name = "Discount Transaction",
                        Permission = null,
                        OnClick = w =>
                        {
                            return;
                        }
                    };
                }

            case TransModButton.VOID:
                {
                    return new ButtonData
                    {
                        Name = "Void Transaction",
                        Permission = OperatorBoolPermission.POS_TransMod_TransVoid,
                        OnClick = w =>
                        {
                            w.POSViewContainer.Content = new YesNoDialogue("Are you sure you want to void this transaction?", () =>
                            {
                                // Yes
                                _controller.VoidTransaction();
                            }, () =>
                            {
                                // No
                                w.POSViewContainer.Content = App.AppHost.Services.GetRequiredService<TransModMenuView>();
                            });
                            return;
                        }
                    };
                }
            default:
                {
                    return null;
                }
        }
    }
}
