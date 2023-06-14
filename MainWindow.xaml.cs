using BT_COMMONS.Helpers;
using BT_POS.Views;
using System.Windows;

namespace BT_POS;

public partial class MainWindow : Window
{
    private readonly POSController _posController;
    private readonly IAbstractFactory<POSHome> _posHome;

    public MainWindow(IAbstractFactory<POSLogin> posLogin, IAbstractFactory<POSHome> posHome, POSController posController)
    {
        InitializeComponent();

        _posController = posController;
        _posHome = posHome;

        //POSParentHeader.Visibility = Visibility.Hidden;

        POSParentHeader_Register.Text = "Register# " + _posController.RegisterNumber;

        POSViewContainer.Content = posLogin.Create();

    }

    public void LoginComplete(POSHome posHome)
    {
        POSParentHeader_Operator.Text = "Oper# " + _posController.CurrentOperator.OperatorId;

        POSViewContainer.Content = posHome;
    }
}
