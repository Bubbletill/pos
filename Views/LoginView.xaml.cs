﻿using BT_COMMONS.Database;
using BT_COMMONS.DataRepositories;
using BT_COMMONS.Operators;
using BT_COMMONS.Operators.API;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

namespace BT_POS.Views;

public partial class LoginView : UserControl
{
    private readonly POSController _controller;
    private readonly IOperatorRepository _operatorRepository;

    public LoginView(POSController controller, IOperatorRepository operatorRepository)
    {
        InitializeComponent();
        VersionText.Text = "POS Version " + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        _controller = controller;
        _operatorRepository = operatorRepository;
        UserIdBox.Focus();
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        if (UserIdBox.Text == "" || PasswordBox.Password == "")
        {
            _controller.HeaderError("Please enter an operator id and password.");
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
            _controller.HeaderError("Internal error. Please try again later.");
            LoginButton.Content = "Login";
            return;
        }

        if (loginResponse.ID != null)
        {
            var oper = await _operatorRepository.GetOperator((int)loginResponse.ID);
            if (!_controller.RegisterOpen)
            {
                if (!oper.HasBoolPermission(OperatorBoolPermission.POS_Admin_RegManagement_OpenRegister))
                {
                    MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
                    mw.Logout();
                    _controller.HeaderError("Insufficient permission to open this register.");
                    return;
                }
            }

            var status = await _controller.CompleteLogin((int)loginResponse.ID);
            if (!status)
            {
                _controller.HeaderError("Failed to complete login. Please try again later.");
                LoginButton.Content = "Login";
                return;
            }

            App.LoginComplete();
        }
        else
        {
            _controller.HeaderError(loginResponse.Message);
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
            _controller.HeaderError();
            PasswordBox.Focus();
        }
    }

    private void PasswordBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) 
        {
            _controller.HeaderError();
            LoginButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }
    }
}
