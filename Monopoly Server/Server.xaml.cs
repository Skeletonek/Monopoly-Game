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
        string player1;
        string player2;
        string player3;
        string player4;
        public MainWindow()
        {
            Server = new NetComm.Host(2020);    
             		
            InitializeComponent();
        }
        private void btnStartServer_Click(object sender, RoutedEventArgs e)
        {
            Server.StartConnection();
            Server.onConnection += new NetComm.Host.onConnectionEventHandler(Server_onConnection);
            //Server.lostConnection += new NetComm.Host.lostConnectionEventHandler(Server_lostConnection);
            Server.DataReceived += new NetComm.Host.DataReceivedEventHandler(Server_DataReceived);
            Server.SendBufferSize = 400;
            Server.ReceiveBufferSize = 50;
            Server.NoDelay = true;
        }

        void Server_onConnection(string id)
        {
            List<string> usersList = Server.Users;
            var users = usersList;
            Server.Brodcast(ASCIIEncoding.ASCII.GetBytes("NewData"));
            foreach (string user in users)
            {
                Server.Brodcast(ASCIIEncoding.ASCII.GetBytes(user));
            }
            label.Content=id + " connected!" + Environment.NewLine; //Updates the log textbox when new user joined
            Server.Brodcast(ASCIIEncoding.ASCII.GetBytes("EndCommunication"));
        }

        void Server_lostConnection(string id)
        {
            //Log.AppendText(id + " disconnected" +
            //Environment.NewLine); //Updates the log textbox when user leaves the room
        }

        void Server_DataReceived(string ID, byte[] Data)
        {
            label.Content = ASCIIEncoding.ASCII.GetString(Data);
            Server.Brodcast(ASCIIEncoding.ASCII.GetBytes("Thank you"));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Server.CloseConnection();
        }
    }
}