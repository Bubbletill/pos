using BT_POS.Buttons.Admin;
using BT_POS.Buttons.Menu;
using BT_POS.Components;
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

namespace BT_POS.Views.Admin;

public partial class AdminMenuView : UserControl
{
    private readonly MainWindow _mainWindow;
    private readonly POSController _controller;

    private readonly Style _buttonStyle;

    public AdminMenuView(MainWindow mainWindow, POSController controller)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        _controller = controller;

        InfoComponent.MainBox.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xe0, 0xe0));

        _buttonStyle = FindResource("BTVerticleButton") as Style;

        LoadButtons(App.AdminButtons);
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

    public void LoadButtons(List<AdminButton> buttons)
    {
        ButtonStackPanel.Children.Clear();
        buttons.ForEach(type =>
        {
            ButtonStackPanel.Children.Add(App.CreateButton(AdminButtonGetter.Get(type), _buttonStyle, this));
        });
        ButtonStackPanel.InvalidateVisual();
        ButtonStackPanel.UpdateLayout();
    }
}
