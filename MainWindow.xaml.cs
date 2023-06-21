﻿using BT_COMMONS.Helpers;
using BT_POS.Views;
using System.Windows;

namespace BT_POS;

public partial class MainWindow : Window
{
    private readonly POSController _posController;
    private readonly IAbstractFactory<POSHome> _posHome;
    private readonly IAbstractFactory<POSLogin> _posLogin;

    public MainWindow(IAbstractFactory<POSLogin> posLogin, IAbstractFactory<POSHome> posHome, POSController posController)
    {
        InitializeComponent();

        _posController = posController;
        _posHome = posHome;
        _posLogin = posLogin;

        POSParentErrorBox.Visibility = Visibility.Hidden;

        POSParentHeader_Register.Text = "Register# " + _posController.RegisterNumber;

        POSViewContainer.Content = posLogin.Create();

    }

    public void HeaderError(string? error = null)
    {
        if (error == null)
        {
            POSParentErrorBox.Visibility = Visibility.Hidden;
            POSParentErrorBoxText.Text = "";
            return;
        }

        POSParentErrorBoxText.Text = error;
        POSParentErrorBox.Visibility = Visibility.Visible;
    }

    public void LoginComplete(POSHome posHome)
    {
        POSParentHeader_Operator.Text = "Operator# " + _posController.CurrentOperator.OperatorId;

        if (!_posController.GotInitialControllerData)
        {
            POSParentHeader_Trans.Text = "Transaction# " + _posController.CurrentTransId;
            _posController.GotInitialControllerData = true; 
        }

        POSViewContainer.Content = posHome;
    }

    public void Logout()
    {
        POSParentHeader_Operator.Text = "Operator# ";
        _posController.CurrentOperator = null;
        App.SetAPIToken(null);
        POSViewContainer.Content = _posLogin.Create();
    }
}
