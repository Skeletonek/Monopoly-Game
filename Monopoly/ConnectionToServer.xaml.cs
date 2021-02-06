using System;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Monopoly
{
    /// <summary>
    /// Logika interakcji dla klasy ConnectionToServer.xaml
    /// </summary>
    public partial class ConnectionToServer : Window
    {
        DispatcherTimer timeout = new DispatcherTimer();
        bool startgame = false;
        public ConnectionToServer()
        {
            InitializeComponent();
            Warning1.Visibility = Visibility.Hidden;
            Warning2.Visibility = Visibility.Hidden;
            timeout.Interval = TimeSpan.FromSeconds(5);
            timeout.Tick += Timeout_Tick;
        }

        private void Timeout_Tick(object sender, EventArgs e)
        {
            Label_ConnectionStatus.Content = "Nie połączono";
            Button_Connect.IsEnabled = true;
            timeout.Stop();
        }

        private void Button_Connect_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.client = new NetComm.Client();
            MainWindow.client.Connect(TextBox_IP.Text, 2020, TextBox_Nickname.Text);
            MainWindow.client.Connected += new NetComm.Client.ConnectedEventHandler(client_Connected);
            MainWindow.client.DataReceived += new NetComm.Client.DataReceivedEventHandler(client_DataReceived);
            MainWindow.client.SendBufferSize = 500;
            MainWindow.client.ReceiveBufferSize = 300;
            MainWindow.client.NoDelay = false;
            Button_Connect.IsEnabled = false;
            Label_ConnectionStatus.Content = "Łączenie...";
            timeout.Start();
        }
        private void client_Connected()
        {
            MainWindow.clientname = TextBox_Nickname.Text;
            MainWindow.ip = TextBox_IP.Text;
            timeout.Stop();
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
            if (ASCIIEncoding.ASCII.GetString(Data) == "Start the party!-0-0")
            {
                startgame = true;
                this.Close();
            }
            if (ASCIIEncoding.ASCII.GetString(Data) == "YouAreLate!-0-0")
            {
                startgame = true;
                this.Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!startgame && MainWindow.connectedToServer)
            { 
                MainWindow.client.Disconnect(); 
            }
        }
    }
}
