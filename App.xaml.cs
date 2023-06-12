using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BT_COMMONS.Transactions;

namespace BT_POS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        Transaction currentTransaction;

        public App()
        {
            Console.WriteLine("Start up");
        }
    }
}
