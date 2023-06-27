using BT_COMMONS.Helpers;
using BT_COMMONS.Transactions;
using BT_POS.Views;
using BT_POS.Views.Admin;
using BT_POS.Views.Dialogues;
using System.Diagnostics;
using System.Windows;

namespace BT_POS;

public partial class MainWindow : Window
{
    private readonly POSController _posController;
    private readonly IAbstractFactory<HomeView> _posHome;
    private readonly IAbstractFactory<LoginView> _posLogin;
    private readonly IAbstractFactory<RegClosedView> _posRegClosed;
    private readonly IAbstractFactory<EnterFloatView> _posEnterFloat;

    public MainWindow(IAbstractFactory<LoginView> posLogin, IAbstractFactory<RegClosedView> posRegClosed, IAbstractFactory<HomeView> posHome, IAbstractFactory<EnterFloatView> posEnterFloat, POSController posController)
    {
        InitializeComponent();

        _posController = posController;
        _posHome = posHome;
        _posLogin = posLogin;
        _posRegClosed = posRegClosed;
        _posEnterFloat = posEnterFloat;

        POSParentErrorBox.Visibility = Visibility.Hidden;

        POSParentHeader_Register.Text = "Register# " + _posController.RegisterNumber;

        if (posController.RegisterOpen)
            POSViewContainer.Content = posLogin.Create();
        else
            POSViewContainer.Content = posRegClosed.Create();

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

    public void LoginComplete(HomeView posHome)
    {
        POSParentHeader_Operator.Text = "Operator# " + _posController.CurrentOperator.OperatorId;

        if (!_posController.GotInitialControllerData)
        {
            POSParentHeader_Trans.Text = "Transaction# " + _posController.CurrentTransId;

            _posController.GotInitialControllerData = true; 
        }

        if (!_posController.RegisterOpen)
        {
            _posController.OpenRegister();
        }

        if (!_posController.LoanPrompted)
        {
            _posController.LoanPrompted = true;
            POSViewContainer.Content = new YesNoDialogue("Would you like to declare an opening float?", () =>
            { // Yes
                POSViewContainer.Content = _posEnterFloat.Create();
            }, () =>
            { // No
                POSViewContainer.Content = posHome;
            });
        } 
        else
        {
            POSViewContainer.Content = posHome;
        }
        
    }

    public void Logout()
    {
        POSParentHeader_Operator.Text = "Operator# ";
        _posController.CurrentOperator = null;
        App.SetAPIToken(null);

        if (_posController.RegisterOpen)
            POSViewContainer.Content = _posLogin.Create();
        else
            POSViewContainer.Content = _posRegClosed.Create();
    }
}
