using BT_COMMONS.Operators.API;
using BT_COMMONS.Operators;
using Microsoft.Extensions.DependencyInjection;
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
using BT_COMMONS.DataRepositories;
using BT_COMMONS.Operators.PermissionAttributes;
using BT_COMMONS.Transactions;

namespace BT_POS.Views.Dialogues;

/// <summary>
/// Interaction logic for AuthDialogue.xaml
/// </summary>
public partial class BoolAuthDialogue : UserControl
{
    private readonly OperatorBoolPermission _permission;
    private readonly Action _onAuthentication;
    private readonly Action _onCancel;

    private readonly POSController _controller;
    private readonly IOperatorRepository _operatorRepository;

    public BoolAuthDialogue(OperatorBoolPermission permission, Action onAuthentication, Action onCancel)
    {
        InitializeComponent();
        _permission = permission;
        _onAuthentication = onAuthentication;
        _onCancel = onCancel;
        _controller = App.AppHost.Services.GetRequiredService<POSController>();
        _operatorRepository = App.AppHost.Services.GetRequiredService<IOperatorRepository>();

        InfoText.Text += permission.GetPromptName();
    }


    private async void AuthenticateButton_Click(object sender, RoutedEventArgs e)
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
            _controller.HeaderError("Internal error. Please try again later. (LN)");
            LoginButton.Content = "Approve";
            return;
        }

        if (loginResponse.ID != null)
        {
            var oper = await _operatorRepository.GetOperator((int)loginResponse.ID);
            if (!oper.HasBoolPermission(_permission))
            {
                _controller.HeaderError("Insufficient permission.");
                LoginButton.Content = "Approve";
                return;
            } 
            else
            {
                if (_controller.CurrentTransaction == null)
                    _controller.TransactionLogQueue.Add(new TransactionLog(TransactionLogType.NSGeneral, _permission.GetPromptName() + " approved by " + oper.ReducedName() + ", ID " + oper.OperatorId));
                else
                    _controller.CurrentTransaction.Logs.Add(new TransactionLog(TransactionLogType.NSGeneral, _permission.GetPromptName() + " approved by " + oper.ReducedName() + ", ID " + oper.OperatorId));
                _onAuthentication();
            }
        } 
        else
        {
            _controller.HeaderError("Invalid operator id or password.");
            LoginButton.Content = "Approve";
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        _onCancel();
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
