using BT_POS.Buttons.Menu;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace BT_POS.Views
{
    /// <summary>
    /// Interaction logic for POSHome.xaml
    /// </summary>
    public partial class POSHome : UserControl
    {
        private readonly MainWindow _mainWindow;
        private List<POSMenuButton> buttons;
        private readonly Style _buttonStyle;

        public POSHome(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
            buttons = new List<POSMenuButton>
            {
                POSMenuButton.ADMIN,
                POSMenuButton.LOGOUT
            };

            _buttonStyle = FindResource("BTButton") as Style;

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
                ButtonStackPanel.UpdateLayout();
            });
        }
    }
}
