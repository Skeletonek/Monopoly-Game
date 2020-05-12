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
    /// Logika interakcji dla klasy ConnectionToServer.xaml
    /// </summary>
    public partial class ConnectionToServer : Window
    {
        public ConnectionToServer()
        {
            InitializeComponent();
            Warning1.Visibility = Visibility.Hidden;
            Warning2.Visibility = Visibility.Hidden;
        }

        private void Button_Connect_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.client = new NetComm.Client();
            MainWindow.client.Connect(TextBox_IP.Text, 2020, TextBox_Nickname.Text);
            MainWindow.client.Connected += new NetComm.Client.ConnectedEventHandler(client_Connected);
            MainWindow.client.DataReceived += new NetComm.Client.DataReceivedEventHandler(client_DataReceived);
            Button_Connect.IsEnabled = false;
        }
        private void client_Connected()
        {
            MainWindow.connectedToServer = true;
            Label_ConnectionStatus.Content = "Połączono";
            Warning1.Visibility = Visibility.Visible;
            Warning2.Visibility = Visibility.Visible;
        }

        void client_DataReceived(byte[] Data, string ID)
        {
            if (ASCIIEncoding.ASCII.GetString(Data) == "NewData")
                ListBox_PlayersConnected.Items.Clear();
            if (ASCIIEncoding.ASCII.GetString(Data) != "EndCommunication" && ASCIIEncoding.ASCII.GetString(Data) != "NewData")
                ListBox_PlayersConnected.Items.Add(ASCIIEncoding.ASCII.GetString(Data));
        }

    }
}
