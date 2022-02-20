using System.Windows;

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
