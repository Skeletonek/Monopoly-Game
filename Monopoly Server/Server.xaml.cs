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
        List<string> usersList;
        public class Game
        {
            public byte[] playerlocation = new byte[4] { 0, 0, 0, 0 };
            public int[] playercash = new int[4] { 1500, 1500, 1500, 1500 };
            public byte[] playerRailroadOwned = new byte[4] { 0, 0, 0, 0 };
            public byte[] playerArrestedTurns = new byte[4] { 0, 0, 0, 0 };
            public bool[] playerAvailable = new bool[4] { false, false, false, false };
            public bool[] playerBankrupt = new bool[4] { false, false, false, false };
            public byte turn = 0;
            public byte dice1;
            public byte dice2;
            public byte selectedField = 0;
            public byte[] fieldHouse = new byte[41];
            public byte[] fieldOwner = new byte[41];
            public byte[] fieldPlayers = new byte[41];
            public int taxmoney = 0;
        }
        Game game = new Game();
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
            Server.ReceiveBufferSize = 300;
            Server.NoDelay = false;
            Label_ServerStatus.Content = "Uruchomiono server";
            ListBox_PlayboardTheme.IsEnabled = true;
        }

        void Server_onConnection(string id)
        {
            if (!GameStarted)
            {
                usersList = Server.Users;
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
                if(GameStarted)
                {
                    Server.Brodcast(ASCIIEncoding.ASCII.GetBytes("YouAreLate!-0-0"));
                }
                SendData(id);
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
                usersList = Server.Users;
                var users = usersList;
                Server.Brodcast(ASCIIEncoding.ASCII.GetBytes("NewData"));
                foreach (string user in users)
                {
                    Server.Brodcast(ASCIIEncoding.ASCII.GetBytes(user));
                    Server.SendData(id, ASCIIEncoding.ASCII.GetBytes("+-" + "0" + "-" + "0"));
                }
                Server.Brodcast(ASCIIEncoding.ASCII.GetBytes("EndCommunication"));
            }
        }

        void Server_DataReceived(string ID, byte[] Data)
        {
            var users = usersList;
            string broadcast = UTF8Encoding.UTF8.GetString(Data);
            string[] serverResponse = broadcast.Split(new char[] { '-' });
            if (serverResponse[0] == "a")
                ServerLog.Text = "";
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
            usersList = Server.Users;
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

        private void SendData(string ID)
        {
            string data;
            data = "a-" + "0-" + Convert.ToString(game.dice1) + Convert.ToString(game.dice2);
            Server.SendData(ID, ASCIIEncoding.ASCII.GetBytes(data));
            data = "b-" + "0-" + game.turn;
            Server.SendData(ID, ASCIIEncoding.ASCII.GetBytes(data));
            data = "c-" + "0-" + game.selectedField;
            Server.SendData(ID, ASCIIEncoding.ASCII.GetBytes(data));
            data = "d-" + "0-" + game.taxmoney;
            Server.SendData(ID, ASCIIEncoding.ASCII.GetBytes(data));
            for (int i = 0; i < 3; i++)
            {
                data = "e-" + i + "-" + game.playerlocation[i];
                Server.SendData(ID, ASCIIEncoding.ASCII.GetBytes(data));
                data = "f-" + i + "-" + game.playercash[i];
                Server.SendData(ID, ASCIIEncoding.ASCII.GetBytes(data));
                data = "g-" + i + "-" + game.playerRailroadOwned[i];
                Server.SendData(ID, ASCIIEncoding.ASCII.GetBytes(data));
                data = "h-" + i + "-" + game.playerArrestedTurns[i];
                Server.SendData(ID, ASCIIEncoding.ASCII.GetBytes(data));
                data = "j-" + i + "-" + game.playerBankrupt[i];
                Server.SendData(ID, ASCIIEncoding.ASCII.GetBytes(data));
                data = "k-" + game.playerlocation[i] + "-" + game.fieldHouse[game.playerlocation[i]];
                Server.SendData(ID, ASCIIEncoding.ASCII.GetBytes(data));
                data = "l-" + game.playerlocation[i] + "-" + game.fieldOwner[game.playerlocation[i]];
                Server.SendData(ID, ASCIIEncoding.ASCII.GetBytes(data));
                data = "m-" + game.playerlocation[i] + "-" + game.fieldPlayers[game.playerlocation[i]];
                Server.SendData(ID, ASCIIEncoding.ASCII.GetBytes(data));
            }
        }

        void DataGet(byte[] Data)
        {
            string response = ASCIIEncoding.ASCII.GetString(Data);
            string[] serverResponse = response.Split(new char[] { '-' });
            switch (serverResponse[0])
            {
                case "a":
                    try
                    {
                        byte dice1 = byte.Parse(Convert.ToString(serverResponse[2].ToCharArray().ElementAt(0)));
                        byte dice2 = byte.Parse(Convert.ToString(serverResponse[2].ToCharArray().ElementAt(1)));
                    }
                    catch
                    {

                    }
                    break;

                case "b":
                    try
                    {
                        game.turn = byte.Parse(serverResponse[2]);
                    }
                    catch
                    {

                    }
                    break;

                case "c":
                    try
                    {
                        game.selectedField = byte.Parse(serverResponse[2]);
                    }
                    catch
                    {

                    }
                    break;

                case "d":
                    try
                    {
                        game.taxmoney = int.Parse(serverResponse[2]);
                    }
                    catch
                    {

                    }
                    break;

                case "e":
                    try
                    {
                        game.playerlocation[byte.Parse(serverResponse[1])] = byte.Parse(serverResponse[2]);
                    }
                    catch
                    {

                    }
                    break;

                case "f":
                    try
                    {
                        game.playercash[byte.Parse(serverResponse[1])] = int.Parse(serverResponse[2]);
                    }
                    catch
                    {

                    }
                    break;

                case "g":
                    try
                    {
                        game.playerRailroadOwned[byte.Parse(serverResponse[1])] = byte.Parse(serverResponse[2]);
                    }
                    catch
                    {

                    }
                    break;

                case "h":
                    try
                    {
                        game.playerArrestedTurns[byte.Parse(serverResponse[1])] = byte.Parse(serverResponse[2]);
                    }
                    catch
                    {

                    }
                    break;

                case "j":
                    try
                    {
                        game.playerBankrupt[byte.Parse(serverResponse[1])] = bool.Parse(serverResponse[2]);
                    }
                    catch
                    {

                    }
                    break;

                case "k":
                    try
                    {
                        game.fieldHouse[byte.Parse(serverResponse[1])] = byte.Parse(serverResponse[2]);
                    }
                    catch
                    {

                    }
                    break;

                case "l":
                    try
                    {
                        game.fieldOwner[byte.Parse(serverResponse[1])] = byte.Parse(serverResponse[2]);
                    }
                    catch
                    {

                    }
                    break;

                case "m":
                    try
                    {
                        game.fieldPlayers[byte.Parse(serverResponse[1])] = byte.Parse(serverResponse[2]);
                    }
                    catch
                    {

                    }
                    break;
            }
        }
    }
}