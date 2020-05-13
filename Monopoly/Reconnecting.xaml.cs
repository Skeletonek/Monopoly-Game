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
using System.Windows.Shapes;

namespace Monopoly
{
    /// <summary>
    /// Logika interakcji dla klasy Reconnecting.xaml
    /// </summary>
    public partial class Reconnecting : Window
    {
        public Reconnecting()
        {
            InitializeComponent();
            MainWindow.client = new NetComm.Client();
            MainWindow.client.Connect(MainWindow.ip, 2020, MainWindow.clientname);
            MainWindow.client.Connected += new NetComm.Client.ConnectedEventHandler(client_Connected);
        }
        private void client_Connected()
        {
            this.Close();
        }
    }
}
