using BT_COMMONS.Database;
using BT_COMMONS.DataRepositories;
using BT_COMMONS.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for POSLogin.xaml
    /// </summary>
    public partial class POSLogin : UserControl
    {
        private readonly POSController _controller;
        private readonly IOperatorRepository _operatorRepository;

        public POSLogin(POSController controller, IOperatorRepository operatorRepository)
        {
            InitializeComponent();
            ErrorBox.Visibility = Visibility.Hidden;
            _controller = controller;
            _operatorRepository = operatorRepository;
            UserIdBox.Focus();
        }

        private void BarError(string error = "")
        {
            if (error == "")
            {
                ErrorBox.Visibility = Visibility.Hidden;
            } else
            {
                ErrorBoxText.Text = error;
                ErrorBox.Visibility = Visibility.Visible;
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserIdBox.Text == "" || PasswordBox.Password == "")
            {
                BarError("Please enter an operator id and password.");
                return;
            }

            LoginButton.Content = "Working...";

            OperatorLoginResponse loginResponse = await _operatorRepository.OperatorLogin(new OperatorLoginRequest
            {
                Id = UserIdBox.Text,
                Password = PasswordBox.Password
            });

            if (loginResponse == null)
            {
                BarError("Internal error. Please try again later.");
                LoginButton.Content = "Login";
                return;
            }

            if (loginResponse.JWT != string.Empty && loginResponse.JWT != null)
            {
                _controller.ControllerAuthenticationToken = loginResponse.JWT;
                var status = await _controller.CompleteLogin();
                if (!status)
                {
                    BarError("Failed to complete login. Please try again later.");
                    LoginButton.Content = "Login";
                    return;
                }

                App.LoginComplete();
            }
            else
            {
                BarError(loginResponse.Message);
                LoginButton.Content = "Login";
            }
        } 

        private void BackOfficeButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void UserIdBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BarError();
                PasswordBox.Focus();
            }
        }

        private void PasswordBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) 
            {
                LoginButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
        }
    }
}
