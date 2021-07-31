using NetComm;
using System;
using System.Linq;
using System.Text;
using System.Windows;

namespace Monopoly
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // SERVER CODE
        // //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void Reload_Tick(object sender, EventArgs e)
        {
            SendData();
        }
        void client_DataReceived(byte[] Data, string ID)
        {
            string response = UTF8Encoding.UTF8.GetString(Data);
            //string response = ASCIIEncoding.ASCII.GetString(Data);
            string[] serverResponse = response.Split(new char[] { '-' });
            switch (serverResponse[0])
            {
                case "0":
                    Game.playerAvailable[0] = true;
                    Game.playername[0] = serverResponse[2];
                    if (Game.playername[0] == clientname)
                    {
                        Game.clientplayer = 0;
                        Label_Player1Name.FontWeight = FontWeights.Bold;
                        Button_ThrowDice.IsEnabled = true;
                    }
                    break;

                case "1":
                    Game.playerAvailable[1] = true;
                    Game.playername[1] = serverResponse[2];
                    if (Game.playername[1] == clientname)
                    {
                        Game.clientplayer = 1;
                        Label_Player2Name.FontWeight = FontWeights.Bold;
                    }
                    break;

                case "2":
                    Game.playerAvailable[2] = true;
                    Game.playername[2] = serverResponse[2];
                    if (Game.playername[2] == clientname)
                    {
                        Game.clientplayer = 2;
                        Label_Player3Name.FontWeight = FontWeights.Bold;
                    }
                    break;

                case "3":
                    Game.playerAvailable[3] = true;
                    Game.playername[3] = serverResponse[2];
                    if (Game.playername[3] == clientname)
                    {
                        Game.clientplayer = 3;
                        Label_Player4Name.FontWeight = FontWeights.Bold;
                    }
                    break;

                case "+":
                    Game.multiplayer = true;
                    StartNewGame();
                    break;

                case "a": //Dice
                    try
                    {
                        byte dice1 = byte.Parse(Convert.ToString(serverResponse[2].ToCharArray().ElementAt(0)));
                        byte dice2 = byte.Parse(Convert.ToString(serverResponse[2].ToCharArray().ElementAt(1)));
                        DiceShow(Game.dice1, Game.dice2);
                        diceScore = Convert.ToByte(dice1 + dice2);
                        DiceScore.Content = Convert.ToString(diceScore);
                    }
                    catch
                    {

                    }
                    break;

                case "b": // Which player turn
                    try
                    {
                        Game.turn = byte.Parse(serverResponse[2]);
                        if (Game.turn == Game.clientplayer && Game.playerArrestedTurns[Game.clientplayer] == 0)
                        {
                            if (Game.playerBankrupt[Game.turn] == false)
                                EnableMove();
                            else
                            {
                                Game.turn++;
                                SendData();
                            }
                        }
                        else if (Game.turn == Game.clientplayer && Game.playerArrestedTurns[Game.turn] != 0)
                        {
                            DisableMove();
                        }
                    }
                    catch
                    {

                    }
                    break;

                case "c": //Chosen field
                    try
                    {
                        Game.selectedField = byte.Parse(serverResponse[2]);
                    }
                    catch
                    {

                    }
                    break;

                case "d": //How much money from tax
                    try
                    {
                        Game.taxmoney = int.Parse(serverResponse[2]);
                    }
                    catch
                    {

                    }
                    break;

                case "e": //Location of player
                    try
                    {
                        Game.playerlocation[byte.Parse(serverResponse[1])] = byte.Parse(serverResponse[2]);
                    }
                    catch
                    {

                    }
                    Jump();
                    break;

                case "f": //Player money
                    try
                    {
                        Game.playercash[byte.Parse(serverResponse[1])] = int.Parse(serverResponse[2]);
                    }
                    catch
                    {

                    }
                    PlayerStatusRefresh();
                    break;

                case "g": //How many railroads owned
                    try
                    {
                        Game.playerRailroadOwned[byte.Parse(serverResponse[1])] = byte.Parse(serverResponse[2]);
                    }
                    catch
                    {

                    }
                    break;

                case "h": //For how long arrested
                    try
                    {
                        Game.playerArrestedTurns[byte.Parse(serverResponse[1])] = byte.Parse(serverResponse[2]);
                    }
                    catch
                    {

                    }
                    break;

                case "j": //Bankrupcy
                    try
                    {
                        Game.playerBankrupt[byte.Parse(serverResponse[1])] = bool.Parse(serverResponse[2]);
                        PlayerStatusRefresh();
                    }
                    catch
                    {

                    }
                    break;

                case "k": //Bought house
                    try
                    {
                        Game.fieldHouse[byte.Parse(serverResponse[1])] = byte.Parse(serverResponse[2]);
                        DrawHouses(byte.Parse(serverResponse[1]), byte.Parse(serverResponse[2]));
                    }
                    catch
                    {

                    }
                    break;

                case "l": //Own field
                    try
                    {
                        Game.fieldOwner[byte.Parse(serverResponse[1])] = byte.Parse(serverResponse[2]);
                        DrawOwner(byte.Parse(serverResponse[1]), byte.Parse(serverResponse[2]));
                    }
                    catch
                    {

                    }
                    break;

                case "m": //How many players on filed
                    try
                    {
                        Game.fieldPlayers[byte.Parse(serverResponse[1])] = byte.Parse(serverResponse[2]);
                    }
                    catch
                    {

                    }
                    break;

                case "z": //GameLog
                    try
                    {
                        GameLog.Text += serverResponse[2];
                    }
                    catch
                    {

                    }
                    break;
            }
        }
        private void SendData()
        {
            string data;
            data = "a-" + "0-" + Convert.ToString(Game.dice1) + Convert.ToString(Game.dice2);
            client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
            data = "b-" + "0-" + Game.turn;
            client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
            data = "c-" + "0-" + Game.selectedField;
            client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
            data = "d-" + "0-" + Game.taxmoney;
            client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
            for (int i = 0; i < 3; i++)
            {
                data = "e-" + i + "-" + Game.playerlocation[i];
                client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
                data = "f-" + i + "-" + Game.playercash[i];
                client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
                data = "g-" + i + "-" + Game.playerRailroadOwned[i];
                client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
                data = "h-" + i + "-" + Game.playerArrestedTurns[i];
                client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
                data = "j-" + i + "-" + Game.playerBankrupt[i];
                client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
                data = "k-" + Game.playerlocation[i] + "-" + Game.fieldHouse[Game.playerlocation[i]];
                client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
                data = "l-" + Game.playerlocation[i] + "-" + Game.fieldOwner[Game.playerlocation[i]];
                client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
                data = "m-" + Game.playerlocation[i] + "-" + Game.fieldPlayers[Game.playerlocation[i]];
                client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
            }
        }

        private void SendGameLog(string text)
        {
            string data;
            data = "z-" + "0-" + text;
            client.SendData(UTF8Encoding.UTF8.GetBytes(data));
            //client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
        }
        private void SendMoveData()
        {
            string data;
            data = "e-" + Game.turn + "-" + Game.playerlocation[Game.turn];
            client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
        }

        private void client_Connected()
        {
            ConnectionStatus.Header = "Connected to server";
        }

        private void client_Disconnected()
        {
            ConnectionStatus.Header = "Disconnected from server. Reconnecting...";
            client.Connect(ip, 2020, clientname);
        }
        private void btnConnectToServer_Click(object sender, RoutedEventArgs e)
        {
            ConnectionToServer connectionToServer = new ConnectionToServer();
            connectionToServer.ShowDialog();
            if (connectedToServer)
            {
                client.DataReceived += new Client.DataReceivedEventHandler(client_DataReceived);
                client.Connected += new NetComm.Client.ConnectedEventHandler(client_Connected);
                client.Disconnected += new Client.DisconnectedEventHandler(client_Disconnected);
                ConnectionStatus.Header = "Connected to server";
            }
        }

        private void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            client.Disconnect();
            if (Game.multiplayer)
                ResetUI();
            Game.multiplayer = false;
            //client.SendData(ASCIIEncoding.ASCII.GetBytes("You are nice"));
        }
    }
}
