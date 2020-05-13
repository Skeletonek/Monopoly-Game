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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.IO;
using NetComm;

namespace Monopoly_Server
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Host Server;
        bool GameStarted;
        public MainWindow()
        {
            Server = new NetComm.Host(2020);    
             		
            InitializeComponent();
        }
        private void btnStartServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Server.StartConnection();
            }
            catch
            {
                MessageBox.Show("Couldn't start the server. Check internet connection.", "MonopolyServer", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new InvalidOperationException("Couldn't start the server. Check internet connection.");
            }
            Server.onConnection += new NetComm.Host.onConnectionEventHandler(Server_onConnection);
            Server.lostConnection += new NetComm.Host.lostConnectionEventHandler(Server_lostConnection);
            Server.DataReceived += new NetComm.Host.DataReceivedEventHandler(Server_DataReceived);
            Server.SendBufferSize = 500;
            Server.ReceiveBufferSize = 200;
            Server.NoDelay = false;
            Label_ServerStatus.Content = "Uruchomiono server";
            ListBox_PlayboardTheme.IsEnabled = true;
        }

        void Server_onConnection(string id)
        {
            if (!GameStarted)
            {
                List<string> usersList = Server.Users;
                var users = usersList;
                Server.Brodcast(ASCIIEncoding.ASCII.GetBytes("NewData"));
                foreach (string user in users)
                {
                    Server.Brodcast(ASCIIEncoding.ASCII.GetBytes(user));
                }
                ListBox_Players.Items.Add(id);
                Server.Brodcast(ASCIIEncoding.ASCII.GetBytes("EndCommunication"));
                if (ListBox_Players.Items.Count >= 2)
                {
                    Button_StartGame.IsEnabled = true;
                }
            }
            else
            {
                //Server.SendData
            }
        }

        void Server_lostConnection(string id)
        {
            if (!GameStarted)
            {
                ListBox_Players.Items.Remove(id);
                if (ListBox_Players.Items.Count < 2)
                {
                    Button_StartGame.IsEnabled = false;
                }
                List<string> usersList = Server.Users;
                var users = usersList;
                Server.Brodcast(ASCIIEncoding.ASCII.GetBytes("NewData"));
                foreach (string user in users)
                {
                    Server.Brodcast(ASCIIEncoding.ASCII.GetBytes(user));
                }
                Server.Brodcast(ASCIIEncoding.ASCII.GetBytes("EndCommunication"));
            }
        }

        void Server_DataReceived(string ID, byte[] Data)
        {
            List<string> usersList = Server.Users;
            var users = usersList;
            string broadcast = ASCIIEncoding.ASCII.GetString(Data);
            ServerLog.Text += broadcast + Environment.NewLine;
            foreach (string user in users)
            {
                if(user != ID)
                Server.SendData(user, ASCIIEncoding.ASCII.GetBytes(broadcast));
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Server.CloseConnection();
        }

        private void Button_StartGame_Click(object sender, RoutedEventArgs e)
        {
            Server.Brodcast(ASCIIEncoding.ASCII.GetBytes("Start the party!-0-0"));
            List<string> usersList = Server.Users;
            var users = usersList;
            byte playernumber = 0;
            foreach (string user in users)
            {
                string playerNumber = Convert.ToString(playernumber);
                Server.Brodcast(ASCIIEncoding.ASCII.GetBytes(playerNumber + "-" + "0-" +user));
                playernumber++;
            }
            Server.Brodcast(ASCIIEncoding.ASCII.GetBytes("+-" + "0" + "-" + "0"));
            GameStarted = true;
        }
    }
}