using NetComm;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Monopoly
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Client client;
        public static string clientname;
        public static string ip;
        Random rng = new Random();
        public static bool connectedToServer = false;
        DispatcherTimer wait = new DispatcherTimer();
        public static int cheat = 0;
        byte diceScore;
        MediaPlayer sfx = new MediaPlayer();
        string sfxfile = @"Resources\music.mp3";
        DispatcherTimer reload = new DispatcherTimer();
        public class Game
        {
            public bool multiplayer = false;
            public string[] playername = new string[5] { "Gracz 1", "Gracz 2", "Gracz 3", "Gracz 4", "Mr. Nobody" };
            public byte[] playerlocation = new byte[4] { 0, 0, 0, 0 };
            public int[] playercash = new int[4] { 1500, 1500, 1500, 1500 };
            public byte[] playerRailroadOwned = new byte[4] { 0, 0, 0, 0 };
            public byte[] playerArrestedTurns = new byte[4] { 0, 0, 0, 0 };
            public bool[] playerAvailable = new bool[4] { false, false, false, false };
            public bool[] playerBankrupt = new bool[4] { false, false, false, false };
            public byte playerBankruptNeededToWin = 0;
            public byte clientplayer = 0;
            public byte turn = 0;
            public byte dice1;
            public byte dice2;
            public byte selectedField = 0;
            public int currentFieldPrice = 0;
            public bool currentFieldForSale = false;
            public byte[] fieldHouse = new byte[41];
            public byte[] fieldOwner = new byte[41];
            public byte[] fieldPlayers = new byte[41];
            public int taxmoney = 0;
            public bool sellmode = false;
            public bool dangerzone = false;
        }

        BoardLocations boardLocations = new BoardLocations();
        BoardData boardData = new BoardData();
        Game game = new Game();
        public MainWindow()
        {
            for (int i = 0; i < 40; i++)
            {
                game.fieldOwner[i] = 4;
                game.fieldHouse[i] = 0;
                game.fieldPlayers[i] = 0;
            }
            game.fieldPlayers[0] = 4;
            boardData.gameDataWriter();
            wait.Interval = TimeSpan.FromMilliseconds(300);
            wait.Tick += JumpingAnimation_Tick;
            reload.Interval = TimeSpan.FromSeconds(10);
            reload.Tick += Reload_Tick;
            InitializeComponent();
            sfxfile = @"Resources\music_wait.mp3";
            sfx.Open(new Uri(sfxfile, UriKind.Relative));
            sfx.Volume = 0.5;
            sfx.Play();
            sfx.MediaEnded += Sfx_MediaEnded;
        }

        private void Sfx_MediaEnded(object sender, EventArgs e)
        {
            //sfx.Play();
        }

        // SERVER CODE
        // //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void Reload_Tick(object sender, EventArgs e)
        {
            SendData();
        }
        void client_DataReceived(byte[] Data, string ID)
        {
            string response = ASCIIEncoding.ASCII.GetString(Data);
            string[] serverResponse = response.Split(new char[] { '-' });
            switch (serverResponse[0])
            {
                case "0":
                    game.playerAvailable[0] = true;
                    game.playername[0] = serverResponse[2];
                    if (game.playername[0] == clientname)
                    {
                        game.clientplayer = 0;
                        Label_Player1Name.FontWeight = FontWeights.Bold;
                        Button_ThrowDice.IsEnabled = true;
                    }
                    break;

                case "1":
                    game.playerAvailable[1] = true;
                    game.playername[1] = serverResponse[2];
                    if (game.playername[1] == clientname)
                    {
                        game.clientplayer = 1;
                        Label_Player2Name.FontWeight = FontWeights.Bold;
                    }
                    break;

                case "2":
                    game.playerAvailable[2] = true;
                    game.playername[2] = serverResponse[2];
                    if (game.playername[2] == clientname)
                    {
                        game.clientplayer = 2;
                        Label_Player3Name.FontWeight = FontWeights.Bold;
                    }
                    break;

                case "3":
                    game.playerAvailable[3] = true;
                    game.playername[3] = serverResponse[2];
                    if (game.playername[3] == clientname)
                    {
                        game.clientplayer = 3;
                        Label_Player4Name.FontWeight = FontWeights.Bold;
                    }
                    break;

                case "+":
                    game.multiplayer = true;
                    StartNewGame();
                    break;

                case "a":
                    try
                    {
                        byte dice1 = byte.Parse(Convert.ToString(serverResponse[2].ToCharArray().ElementAt(0)));
                        byte dice2 = byte.Parse(Convert.ToString(serverResponse[2].ToCharArray().ElementAt(1)));
                        Dice1.Content = Convert.ToString(dice1);
                        Dice2.Content = Convert.ToString(dice2);
                        diceScore = Convert.ToByte(dice1 + dice2);
                        DiceScore.Content = Convert.ToString(diceScore);
                    }
                    catch
                    {

                    }
                    break;

                case "b":
                    try
                    {
                        game.turn = byte.Parse(serverResponse[2]);
                        if (game.turn == game.clientplayer && game.playerArrestedTurns[game.clientplayer] == 0)
                        {
                            if (game.playerBankrupt[game.turn] == false)
                                EnableMove();
                            else
                            {
                                game.turn++;
                                SendData();
                            }
                        }
                        else if (game.turn == game.clientplayer && game.playerArrestedTurns[game.turn] != 0)
                        {
                            DisableMove();
                        }
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
                    Jump();
                    break;

                case "f":
                    try
                    {
                        game.playercash[byte.Parse(serverResponse[1])] = int.Parse(serverResponse[2]);
                    }
                    catch
                    {

                    }
                    PlayerStatusRefresh();
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
                        PlayerStatusRefresh();
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

                case "z":
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
            data = "a-" + "0-" + Convert.ToString(game.dice1) + Convert.ToString(game.dice2);
            client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
            data = "b-" + "0-" + game.turn;
            client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
            data = "c-" + "0-" + game.selectedField;
            client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
            data = "d-" + "0-" + game.taxmoney;
            client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
            for (int i = 0; i < 3; i++)
            {
                data = "e-" + i + "-" + game.playerlocation[i];
                client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
                data = "f-" + i + "-" + game.playercash[i];
                client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
                data = "g-" + i + "-" + game.playerRailroadOwned[i];
                client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
                data = "h-" + i + "-" + game.playerArrestedTurns[i];
                client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
                data = "j-" + i + "-" + game.playerBankrupt[i];
                client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
                data = "k-" + game.playerlocation[i] + "-" + game.fieldHouse[game.playerlocation[i]];
                client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
                data = "l-" + game.playerlocation[i] + "-" + game.fieldOwner[game.playerlocation[i]];
                client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
                data = "m-" + game.playerlocation[i] + "-" + game.fieldPlayers[game.playerlocation[i]];
                client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
            }
        }

        private void SendGameLog(string text)
        {
            string data;
            data = "z-" + "0-" + text;
            client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
        }
        private void SendMoveData()
        {
            string data;
            data = "e-" + game.turn + "-" + game.playerlocation[game.turn];
            client.SendData(ASCIIEncoding.ASCII.GetBytes(data));
        }

        private void client_Connected()
        {
            //pause the game
        }

        private void client_Disconnected()
        {
            Reconnecting reconnecting = new Reconnecting();
            reconnecting.Show();
        }
        private void btnConnectToServer_Click(object sender, RoutedEventArgs e)
        {
            ConnectionToServer connectionToServer = new ConnectionToServer();
            connectionToServer.ShowDialog();
            client.DataReceived += new Client.DataReceivedEventHandler(client_DataReceived);
            client.Connected += new NetComm.Client.ConnectedEventHandler(client_Connected);
            client.Disconnected += new Client.DisconnectedEventHandler(client_Disconnected);
        }

        private void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            //client.SendData(ASCIIEncoding.ASCII.GetBytes("You are nice"));
        }

        // GAME CODE
        // //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void StartNewGame()
        {
            if (game.playerAvailable[0])
            {
                Player1.Visibility = Visibility.Visible;
                Player1_Icon.Visibility = Visibility.Visible;
                Label_Player1Cash.Visibility = Visibility.Visible;
                Label_Player1Name.Visibility = Visibility.Visible;
                Label_Player1Name.Content = game.playername[0];
                game.playerBankruptNeededToWin++;
            }
            if (game.playerAvailable[1])
            {
                Player2.Visibility = Visibility.Visible;
                Player2_Icon.Visibility = Visibility.Visible;
                Label_Player2Cash.Visibility = Visibility.Visible;
                Label_Player2Name.Visibility = Visibility.Visible;
                Label_Player2Name.Content = game.playername[1];
                game.playerBankruptNeededToWin++;
            }
            if (game.playerAvailable[2])
            {
                Player3.Visibility = Visibility.Visible;
                Player3_Icon.Visibility = Visibility.Visible;
                Label_Player3Cash.Visibility = Visibility.Visible;
                Label_Player3Name.Visibility = Visibility.Visible;
                Label_Player3Name.Content = game.playername[2];
                game.playerBankruptNeededToWin++;
            }
            if (game.playerAvailable[3])
            {
                Player4.Visibility = Visibility.Visible;
                Player4_Icon.Visibility = Visibility.Visible;
                Label_Player4Cash.Visibility = Visibility.Visible;
                Label_Player4Name.Visibility = Visibility.Visible;
                Label_Player4Name.Content = game.playername[3];
                game.playerBankruptNeededToWin++;
            }
            if (!connectedToServer)
            {
                game.clientplayer = 0;
                Label_Player1Name.FontWeight = FontWeights.Bold;
                Button_ThrowDice.IsEnabled = true;
            }
            game.playercash[0] = 1500;
            game.playercash[1] = 1500;
            game.playercash[2] = 1500;
            game.playercash[3] = 1500;
            game.playerlocation[0] = 0;
            game.playerlocation[1] = 0;
            game.playerlocation[2] = 0;
            game.playerlocation[3] = 0;
            for (int i = 0; i < 41; i++)
            {
                game.fieldOwner[i] = 4;
                game.fieldHouse[i] = 0;
                game.fieldPlayers[i] = 0;
            }
            game.fieldPlayers[0] = 4;
            boardData.gameDataWriter();
            GameCanvas.Children.RemoveRange(44, 100);
            GameLog.Text = "";
            sfx.Stop();
        }
        private void TurnCheck()
        {
            if (game.playerAvailable[3])
            {
                if (game.turn > 3)
                {
                    game.turn = 0;
                }
            }
            else if (game.playerAvailable[2])
            {
                if (game.turn > 2)
                {
                    game.turn = 0;
                }
            }
            else
            {
                if (game.turn > 1)
                {
                    game.turn = 0;
                }
            }
        }

        private void EndTurn()
        {
            Button_EndTurn.IsEnabled = false;
            TurnCheck();
            if (game.turn == 0 && game.playerArrestedTurns[game.turn] == 0)
            {
                if (game.playerBankrupt[game.turn] == false)
                    EnableMove();
                else
                    game.turn++;
            }
            else if (game.turn == 0 && game.playerArrestedTurns[game.turn] != 0)
            {
                DisableMove();
            }
            if (game.turn == 1 && game.playerArrestedTurns[game.turn] == 0)
            {
                if (game.playerBankrupt[game.turn] == false)
                    EnableMove();
                else
                {
                    game.turn++;
                    TurnCheck();
                }
            }
            else if (game.turn == 1 && game.playerArrestedTurns[game.turn] != 0)
            {
                DisableMove();
            }
            if (game.turn == 2 && game.playerArrestedTurns[game.turn] == 0)
            {
                if (game.playerBankrupt[game.turn] == false)
                    EnableMove();
                else
                {
                    game.turn++;
                    TurnCheck();
                }
            }
            else if (game.turn == 2 && game.playerArrestedTurns[game.turn] != 0)
            {
                DisableMove();
            }
            if (game.turn == 3 && game.playerArrestedTurns[game.turn] == 0)
            {
                if (game.playerBankrupt[game.turn] == false)
                    EnableMove();
                else
                {
                    game.turn++;
                    TurnCheck();
                    EndTurn();
                }
            }
            else if (game.turn == 3 && game.playerArrestedTurns[game.turn] != 0)
            {
                DisableMove();
            }
        }

        private void bankrupt()
        {
            GameLog.Text += game.playername[game.turn] + " OGŁASZA BANKRUCTWO!" + Environment.NewLine + Environment.NewLine;
            if (game.multiplayer)
            {
                SendGameLog(game.playername[game.turn] + " OGŁASZA BANKRUCTWO!" + Environment.NewLine + Environment.NewLine);
            }
            if (game.turn == game.clientplayer)
            {
                Button_ThrowDice.IsEnabled = false;
                Button_EndTurn.IsEnabled = false;
            }
            game.playerBankrupt[game.turn] = true;
            game.fieldPlayers[game.playerlocation[game.turn]]--;
            switch (game.turn)
            {
                case 0:
                    Player1.Visibility = Visibility.Hidden;
                    Player1_Icon.Visibility = Visibility.Hidden;
                    break;

                case 1:
                    Player2.Visibility = Visibility.Hidden;
                    Player2_Icon.Visibility = Visibility.Hidden;
                    break;

                case 2:
                    Player3.Visibility = Visibility.Hidden;
                    Player3_Icon.Visibility = Visibility.Hidden;
                    break;

                case 3:
                    Player4.Visibility = Visibility.Hidden;
                    Player4_Icon.Visibility = Visibility.Hidden;
                    break;
            }
            game.playerBankruptNeededToWin--;
            LeaveDangerZone();
            if (game.playerBankruptNeededToWin <= 1)
            {
                MessageBox.Show("Koniec gry!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                if (game.multiplayer)
                {
                    client.Disconnect();
                }
                this.Close();
            }
            EndTurn();

        }
        private void DangerZone()
        {
            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = Color.FromArgb(255, 255, 100, 100);
            this.Background = brush;
            Button_EndTurn.Background = brush;
            Button_EndTurn.Content = "Zapłać";
            Button_EndTurn.IsEnabled = true;
            Button_ThrowDice.Background = brush;
            Button_ThrowDice.Content = "Ogłoś bankructwo";
            Button_ThrowDice.IsEnabled = true;
            game.sellmode = true;
            Button_MouseMode.Content = "Tryb sprzedawania ulic";
            if (game.turn == game.clientplayer)
                MessageBox.Show("Znajdujesz się w strefie zagrożenia! Sprzedaj budynki lub ulice aby móc zapłacić. Jeżeli nie możesz zrobić nic więcej, ogłoś swoje bankructwo", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            game.dangerzone = true;

        }
        private void LeaveDangerZone()
        {
            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = Color.FromArgb(255, 221, 221, 221);
            Button_EndTurn.Background = brush;
            Button_EndTurn.Content = "Zakończ turę";
            Button_EndTurn.IsEnabled = true;
            Button_ThrowDice.Background = brush;
            Button_ThrowDice.Content = "Rzuć koścmi";
            Button_ThrowDice.IsEnabled = false;
            brush.Color = Color.FromArgb(255, 255, 255, 255);
            this.Background = brush;
            game.sellmode = false;
            Button_MouseMode.Content = "Tryb budowania domów";
            game.dangerzone = false;
        }
        private void EnableMove()
        {
            if (game.clientplayer == game.turn)
            {
                game.sellmode = false;
                Button_MouseMode.Content = "Tryb budowania domów";
                Button_ThrowDice.IsEnabled = true;
                DiceScore.Content = "Twoja tura!";
                sfx.Open(new Uri(@"Resources\correct.wav", UriKind.Relative));
                sfx.Play();
            }
            else
            {
                if (!game.multiplayer)
                {
                    ThrowDiceAndMove();
                }
            }
            if (game.multiplayer)
                SendData();
        }

        private void DisableMove()
        {
            game.playerArrestedTurns[game.turn]--;
            if (game.multiplayer)
                SendData();
            if (game.clientplayer == game.turn)
            {
                Button_EndTurn.IsEnabled = true;
                if (game.playerArrestedTurns[game.turn] == 0)
                {
                    game.playerlocation[game.turn] = 10;
                }
            }
            else
            {
                if (!game.multiplayer)
                {
                    if (game.playerArrestedTurns[game.turn] == 0)
                    {
                        game.playerlocation[game.turn] = 10;
                    }
                    game.turn++;
                    EndTurn();
                }
            }
        }

        private void ThrowDiceAndMove()
        {
            game.dice1 = Convert.ToByte(rng.Next(1, 7));
            game.dice2 = Convert.ToByte(rng.Next(1, 7));
            Dice1.Content = game.dice1;
            Dice2.Content = game.dice2;
            diceScore = Convert.ToByte(game.dice1 + game.dice2);
            DiceScore.Content = diceScore;
            if (game.multiplayer)
            {
                SendData();
            }
            wait.Start();
        }

        private void JumpingAnimation_Tick(object sender, EventArgs e)
        {
            if (diceScore > 0)
            {
                game.playerlocation[game.turn]++;
                game.fieldPlayers[game.playerlocation[game.turn] - 1]--;
                game.fieldPlayers[game.playerlocation[game.turn]]++;
                if (game.playerlocation[game.turn] >= 40)
                {
                    game.fieldPlayers[40]--;
                    game.fieldPlayers[0]++;
                    game.playerlocation[game.turn] = 0;
                    game.playercash[game.turn] = game.playercash[game.turn] + 200;
                    GameLog.Text += game.playername[game.turn] + " otrzymuje 200$ za przejście przez start!" + Environment.NewLine + Environment.NewLine;
                    if (game.multiplayer)
                    {
                        SendGameLog(game.playername[game.turn] + " otrzymuje 200$ za przejście przez start!" + Environment.NewLine + Environment.NewLine);
                    }
                    PlayerStatusRefresh();
                }
                Jump();
                diceScore--;
                if (game.multiplayer)
                    SendMoveData();
            }
            else
            {
                wait.Stop();
                game.selectedField = game.playerlocation[game.turn];
                OverviewRefresh();
                FieldCheck();
                if (game.turn != 0 && !game.multiplayer)
                {
                    game.turn++;
                    if (game.turn > 3)
                    {
                        game.turn = 0;
                    }
                    EndTurn();
                }
                else
                {
                    Button_EndTurn.IsEnabled = true;
                }
                if (game.multiplayer)
                    SendData();
            }
        }

        private void FieldCheck()
        {
            byte currentPlayerLocation = game.playerlocation[game.turn];
            int rent = boardData.fieldNoSetRent[currentPlayerLocation];
            if (boardData.fieldChance[currentPlayerLocation] == true)
            {
                byte chanceCard = Convert.ToByte(rng.Next(0, 5));
                if (game.turn == game.clientplayer)
                {
                    MessageBox.Show(boardData.chanceText[chanceCard], "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (!doChanceCard(chanceCard))
                    {
                        MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                        game.dangerzone = true;
                        DangerZone();
                    }
                    else
                    {
                        if (game.dangerzone == true)
                        {
                            LeaveDangerZone();
                        }
                        GameLog.Text += game.playername[game.turn] + " otrzymuje kartę szansy: " + boardData.chanceText[chanceCard] + "!" + Environment.NewLine + Environment.NewLine;
                        if (game.multiplayer)
                        {
                            SendGameLog(game.playername[game.turn] + " otrzymuje kartę szansy: " + boardData.chanceText[chanceCard] + "!" + Environment.NewLine + Environment.NewLine);
                        }
                    }
                }
                else
                {
                    if (!game.multiplayer)
                    {
                        if (doChanceCard(chanceCard))
                        {
                            GameLog.Text += game.playername[game.turn] + " otrzymuje kartę szansy: " + boardData.chanceText[chanceCard] + "!" + Environment.NewLine + Environment.NewLine;
                        }
                        else
                        {
                            MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                            bankrupt();
                        }
                    }
                }

            }
            else if (boardData.fieldCommChest[currentPlayerLocation] == true)
            {
                byte commChestCard = Convert.ToByte(rng.Next(0, 11));
                if (game.turn == game.clientplayer)
                {
                    MessageBox.Show(boardData.commChestText[commChestCard], "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (!doCommChestCard(commChestCard))
                    {
                        MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                        game.dangerzone = true;
                        DangerZone();
                    }
                    else
                    {
                        if (game.dangerzone == true)
                        {
                            LeaveDangerZone();
                        }
                        GameLog.Text += game.playername[game.turn] + " otrzymuje kartę kasy społecznej: " + boardData.commChestText[commChestCard] + "!" + Environment.NewLine + Environment.NewLine;
                        if (game.multiplayer)
                        {
                            SendGameLog(game.playername[game.turn] + " otrzymuje kartę kasy społecznej: " + boardData.commChestText[commChestCard] + "!" + Environment.NewLine + Environment.NewLine);
                        }
                    }
                }
                else
                {
                    if (!game.multiplayer)
                    {
                        if (doCommChestCard(commChestCard))
                        {
                            GameLog.Text += game.playername[game.turn] + " otrzymuje kartę kasy społecznej: " + boardData.commChestText[commChestCard] + "!" + Environment.NewLine + Environment.NewLine;
                        }
                        else
                        {
                            MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                            bankrupt();
                        }
                    }
                }
            }
            else if (boardData.fieldRailroad[currentPlayerLocation] == true)
            {
                if (game.fieldOwner[currentPlayerLocation] == 4)
                {
                    if (game.turn == game.clientplayer)
                    {
                        MessageBoxResult result = MessageBox.Show("Czy chcesz kupić " + boardData.fieldName[currentPlayerLocation] + "?", "Monopoly", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        switch (result)
                        {
                            case MessageBoxResult.Yes:
                                if (!buyField(currentPlayerLocation))
                                {
                                    MessageBox.Show("Nie stać Cię na ten dworzec!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                else
                                {
                                    game.playerRailroadOwned[game.turn]++;
                                    GameLog.Text += game.playername[game.turn] + " kupuje " + boardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                                    if (game.multiplayer)
                                    {
                                        SendGameLog(game.playername[game.turn] + " kupuje " + boardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine);
                                    }
                                }
                                break;

                            case MessageBoxResult.No:
                                break;
                        }
                    }
                    else
                    {
                        if (!game.multiplayer)
                        {
                            if (buyField(currentPlayerLocation))
                            {
                                game.playerRailroadOwned[game.turn]++;
                                GameLog.Text += game.playername[game.turn] + " kupuje " + boardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;

                            }
                        }
                    }
                }
                else if (game.fieldOwner[currentPlayerLocation] != game.turn)
                {
                    if (game.playerRailroadOwned[game.fieldOwner[currentPlayerLocation]] == 1)
                    {
                        rent = boardData.field1Rent[currentPlayerLocation];
                    }
                    else if (game.playerRailroadOwned[game.fieldOwner[currentPlayerLocation]] == 2)
                    {
                        rent = boardData.field2Rent[currentPlayerLocation];
                    }
                    else if (game.playerRailroadOwned[game.fieldOwner[currentPlayerLocation]] == 3)
                    {
                        rent = boardData.field3Rent[currentPlayerLocation];
                    }
                    else if (game.playerRailroadOwned[game.fieldOwner[currentPlayerLocation]] >= 4)
                    {
                        rent = boardData.field4Rent[currentPlayerLocation];
                    }
                    if (game.turn == game.clientplayer)
                    {
                        MessageBox.Show("Stanąłeś na dworcu gracza " + game.fieldOwner[currentPlayerLocation] + ". Musisz mu zapłacić: " + rent, "Monopoly", MessageBoxButton.OK, MessageBoxImage.Warning);
                        if (!payRent(currentPlayerLocation, rent))
                        {
                            MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                            game.dangerzone = true;
                            DangerZone();
                        }
                        else
                        {
                            if (game.dangerzone == true)
                            {
                                LeaveDangerZone();
                            }
                            GameLog.Text += game.playername[game.turn] + " płaci " + rent + "$ graczowi " + game.playername[game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                            if (game.multiplayer)
                            {
                                SendGameLog(game.playername[game.turn] + " płaci " + rent + "$ graczowi " + game.playername[game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine);
                            }
                        }
                    }
                    else
                    {
                        if (!game.multiplayer)
                        {
                            if (!payRent(currentPlayerLocation, rent))
                            {
                                MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                                bankrupt();
                            }
                            else
                            {
                                GameLog.Text += game.playername[game.turn] + " płaci " + rent + "$ graczowi " + game.playername[game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                            }
                        }
                    }
                }
            }
            else if (boardData.fieldTax[currentPlayerLocation] == true)
            {
                if (game.turn == game.clientplayer)
                {
                    MessageBox.Show("Musisz zapłacić podatek w wysokości " + boardData.fieldTaxCost[currentPlayerLocation] + "$.", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    if (!payTax(currentPlayerLocation))
                    {
                        MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                        game.dangerzone = true;
                        DangerZone();
                    }
                    else
                    {
                        if (game.dangerzone == true)
                        {
                            LeaveDangerZone();
                        }
                        GameLog.Text += game.playername[game.turn] + " płaci podatek w wysokości " + boardData.fieldTaxCost[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                        if (game.multiplayer)
                        {
                            SendGameLog(game.playername[game.turn] + " płaci podatek w wysokości " + boardData.fieldTaxCost[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine);
                        }
                    }
                }
                else
                {
                    if (!game.multiplayer)
                    {
                        if (payTax(currentPlayerLocation))
                        {
                            GameLog.Text += game.playername[game.turn] + " płaci podatek w wysokości " + boardData.fieldTaxCost[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                        }
                        else
                        {
                            MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                            bankrupt();
                        }
                    }
                }
            }
            else if (boardData.fieldExtra[currentPlayerLocation] == true)
            {
                switch (currentPlayerLocation)
                {
                    case 10: //Prison
                        GameLog.Text += game.playername[game.turn] + " odwiedza więźniów! Jaki miły z niego człowiek!" + Environment.NewLine + Environment.NewLine;
                        if (game.multiplayer)
                        {
                            SendGameLog(game.playername[game.turn] + " odwiedza więźniów! Jaki miły z niego człowiek!" + Environment.NewLine + Environment.NewLine);
                        }
                        break;

                    case 20: //Parking Lot
                        game.playercash[game.turn] = game.playercash[game.turn] + game.taxmoney;
                        PlayerStatusRefresh();
                        GameLog.Text += game.playername[game.turn] + " zdobywa " + game.taxmoney + "$!" + Environment.NewLine + Environment.NewLine;
                        if (game.multiplayer)
                        {
                            SendGameLog(game.playername[game.turn] + " zdobywa " + game.taxmoney + "$!" + Environment.NewLine + Environment.NewLine);
                        }
                        if (game.turn == game.clientplayer)
                        {
                            MessageBox.Show("Zdobywasz " + game.taxmoney + "$!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        game.taxmoney = 0;
                        break;

                    case 30: //Go To Jail
                        if (game.turn == game.clientplayer)
                        {
                            MessageBox.Show("Idziesz do więzienia!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        game.playerlocation[game.turn] = 40;
                        game.playerArrestedTurns[game.turn] = 2;
                        Jump();
                        GameLog.Text += game.playername[game.turn] + " zostaje aresztowany!" + Environment.NewLine + Environment.NewLine;
                        if (game.multiplayer)
                        {
                            SendGameLog(game.playername[game.turn] + " zostaje aresztowany!" + Environment.NewLine + Environment.NewLine);
                        }
                        break;

                    case 12: //Electric Company
                        if (game.fieldOwner[currentPlayerLocation] == 4)
                        {
                            if (game.turn == game.clientplayer)
                            {
                                MessageBoxResult result = MessageBox.Show("Czy chcesz kupić elektrownię?", "Monopoly", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                switch (result)
                                {
                                    case MessageBoxResult.Yes:
                                        if (!buyField(currentPlayerLocation))
                                        {
                                            MessageBox.Show("Nie stać Cię na elektrownię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                        else
                                        {
                                            GameLog.Text += game.playername[game.turn] + " kupuje " + boardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                                            if (game.multiplayer)
                                            {
                                                SendGameLog(game.playername[game.turn] + " kupuje " + boardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine);
                                            }
                                        }
                                        break;

                                    case MessageBoxResult.No:
                                        break;
                                }
                            }
                            else
                            {
                                if (!game.multiplayer)
                                {
                                    if (buyField(currentPlayerLocation))
                                    {
                                        GameLog.Text += game.playername[game.turn] + " kupuje " + boardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                                    }
                                }
                            }
                        }
                        else if (game.fieldOwner[currentPlayerLocation] != game.turn)
                        {
                            int calculatedMoney = calculateExtraFieldMultiplier(currentPlayerLocation);
                            if (game.turn == game.clientplayer)
                            {
                                MessageBox.Show("Stanąłeś na elektrowni gracza " + game.fieldOwner[currentPlayerLocation] + ". Musisz mu zapłacić: " + calculatedMoney, "Monopoly", MessageBoxButton.OK, MessageBoxImage.Warning);
                                if (!payExtraFieldMultiplier(calculatedMoney, currentPlayerLocation))
                                {
                                    MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                                    game.dangerzone = true;
                                    DangerZone();
                                }
                                else
                                {
                                    if (game.dangerzone == true)
                                    {
                                        LeaveDangerZone();
                                    }
                                    GameLog.Text += game.playername[game.turn] + " płaci " + calculatedMoney + "$ graczowi " + game.playername[game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                                    if (game.multiplayer)
                                    {
                                        SendGameLog(game.playername[game.turn] + " płaci " + calculatedMoney + "$ graczowi " + game.playername[game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine);
                                    }
                                }
                            }
                            else
                            {
                                if (!game.multiplayer)
                                {
                                    if (!payExtraFieldMultiplier(calculatedMoney, currentPlayerLocation))
                                    {
                                        MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                                        bankrupt();
                                    }
                                    else
                                    {
                                        GameLog.Text += game.playername[game.turn] + " płaci " + calculatedMoney + "$ graczowi " + game.playername[game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                                    }
                                }
                            }
                        }
                        break;

                    case 28: //Waterworks
                        if (game.fieldOwner[currentPlayerLocation] == 4)
                        {
                            if (game.turn == game.clientplayer)
                            {
                                MessageBoxResult result = MessageBox.Show("Czy chcesz kupić wodociągi?", "Monopoly", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                switch (result)
                                {
                                    case MessageBoxResult.Yes:
                                        if (!buyField(currentPlayerLocation))
                                        {
                                            MessageBox.Show("Nie stać Cię na wodociągi!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                        else
                                        {
                                            GameLog.Text += game.playername[game.turn] + " kupuje " + boardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                                            if (game.multiplayer)
                                            {
                                                SendGameLog(game.playername[game.turn] + " kupuje " + boardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine);
                                            }
                                        }
                                        break;

                                    case MessageBoxResult.No:
                                        break;
                                }
                            }
                            else
                            {
                                if (!game.multiplayer)
                                {
                                    if (buyField(currentPlayerLocation))
                                    {
                                        GameLog.Text += game.playername[game.turn] + " kupuje " + boardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                                    }
                                }
                            }
                        }
                        else if (game.fieldOwner[currentPlayerLocation] != game.turn)
                        {
                            int calculatedMoney = calculateExtraFieldMultiplier(currentPlayerLocation);
                            if (game.turn == game.clientplayer)
                            {
                                MessageBox.Show("Stanąłeś na wodociągach gracza " + game.fieldOwner[currentPlayerLocation] + ". Musisz mu zapłacić: " + calculatedMoney, "Monopoly", MessageBoxButton.OK, MessageBoxImage.Warning);
                                if (!payExtraFieldMultiplier(calculatedMoney, currentPlayerLocation))
                                {
                                    MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                                    game.dangerzone = true;
                                    DangerZone();
                                }
                                else
                                {
                                    if (game.dangerzone == true)
                                    {
                                        LeaveDangerZone();
                                    }
                                    GameLog.Text += game.playername[game.turn] + " płaci " + calculatedMoney + "$ graczowi " + game.playername[game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                                    if (game.multiplayer)
                                    {
                                        SendGameLog(game.playername[game.turn] + " płaci " + calculatedMoney + "$ graczowi " + game.playername[game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine);
                                    }
                                }
                            }
                            else
                            {
                                if (!game.multiplayer)
                                {
                                    if (!payExtraFieldMultiplier(calculatedMoney, currentPlayerLocation))
                                    {
                                        MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                                        bankrupt();
                                    }
                                    else
                                    {
                                        GameLog.Text += game.playername[game.turn] + " płaci " + calculatedMoney + "$ graczowi " + game.playername[game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                                    }
                                }
                            }
                        }
                        break;
                }
            }
            else // For normal estates
            {
                if (game.fieldOwner[currentPlayerLocation] == 4)
                {
                    if (game.turn == game.clientplayer)
                    {
                        MessageBoxResult result = MessageBox.Show("Czy chcesz kupić ulicę " + boardData.fieldName[currentPlayerLocation] + "?", "Monopoly", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        switch (result)
                        {
                            case MessageBoxResult.Yes:
                                if (!buyField(currentPlayerLocation))
                                {
                                    MessageBox.Show("Nie stać Cię na tą ulicę!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                else
                                {
                                    GameLog.Text += game.playername[game.turn] + " kupuje " + boardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                                    if (game.multiplayer)
                                    {
                                        SendGameLog(game.playername[game.turn] + " kupuje " + boardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine);
                                    }
                                    if (game.fieldOwner[currentPlayerLocation] != 4 && game.fieldOwner[currentPlayerLocation] == game.fieldOwner[boardData.fieldSet1[currentPlayerLocation]] && game.fieldOwner[currentPlayerLocation] == game.fieldOwner[boardData.fieldSet2[currentPlayerLocation]] || boardData.fieldSet2[currentPlayerLocation] == 0)
                                    {
                                        MessageBox.Show("Od teraz możesz kupować domy w tej dzielnicy! Aby kupić, kliknij na dane pole lewym przyciskiem myszy przed zakończeniem tury", "Monopoly", MessageBoxButton.YesNo, MessageBoxImage.Information);
                                    }
                                }
                                break;

                            case MessageBoxResult.No:
                                break;
                        }
                    }
                    else
                    {
                        if (!game.multiplayer)
                        {
                            if (buyField(currentPlayerLocation))
                            {
                                GameLog.Text += game.playername[game.turn] + " kupuje " + boardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                            }
                        }
                    }
                }
                else if (game.fieldOwner[currentPlayerLocation] != game.turn)
                {
                    bool hasSet = false;
                    if (boardData.fieldSet2[currentPlayerLocation] == 0)
                    {
                        if (game.fieldOwner[currentPlayerLocation] == game.turn && game.fieldOwner[currentPlayerLocation] != 4 && game.fieldOwner[currentPlayerLocation] == game.fieldOwner[boardData.fieldSet1[currentPlayerLocation]])
                        {
                            hasSet = true;
                        }
                    }
                    else
                    {
                        if (game.fieldOwner[currentPlayerLocation] == game.turn && game.fieldOwner[currentPlayerLocation] != 4 && game.fieldOwner[currentPlayerLocation] == game.fieldOwner[boardData.fieldSet1[currentPlayerLocation]] && game.fieldOwner[currentPlayerLocation] == game.fieldOwner[boardData.fieldSet2[currentPlayerLocation]])
                        {
                            hasSet = true;
                        }
                    }
                    if (hasSet)
                    {
                        rent = boardData.fieldNoSetRent[currentPlayerLocation] * 2;
                    }
                    if (game.fieldHouse[currentPlayerLocation] == 1)
                    {
                        rent = boardData.field1Rent[currentPlayerLocation];
                    }
                    else if (game.fieldHouse[currentPlayerLocation] == 2)
                    {
                        rent = boardData.field2Rent[currentPlayerLocation];
                    }
                    else if (game.fieldHouse[currentPlayerLocation] == 3)
                    {
                        rent = boardData.field3Rent[currentPlayerLocation];
                    }
                    else if (game.fieldHouse[currentPlayerLocation] == 4)
                    {
                        rent = boardData.field4Rent[currentPlayerLocation];
                    }
                    else if (game.fieldHouse[currentPlayerLocation] >= 5)
                    {
                        rent = boardData.fieldHRent[currentPlayerLocation];
                    }
                    if (game.turn == game.clientplayer)
                    {
                        MessageBox.Show("Stanąłeś na dzielnicy gracz " + game.playername[game.fieldOwner[currentPlayerLocation]] + ". Musisz mu zapłacić: " + rent, "Monopoly", MessageBoxButton.OK, MessageBoxImage.Warning);
                        if (!payRent(currentPlayerLocation, rent))
                        {
                            MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                            game.dangerzone = true;
                            DangerZone();
                        }
                        else
                        {
                            if (game.dangerzone == true)
                            {
                                LeaveDangerZone();
                            }
                            GameLog.Text += game.playername[game.turn] + " płaci " + rent + "$ graczowi " + game.playername[game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                            if (game.multiplayer)
                            {
                                SendGameLog(game.playername[game.turn] + " płaci " + rent + "$ graczowi " + game.playername[game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine);
                            }
                        }
                    }
                    else
                    {
                        if (!game.multiplayer)
                        {
                            if (!payRent(currentPlayerLocation, rent))
                            {
                                MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                                bankrupt();
                            }
                            else
                            {
                                GameLog.Text += game.playername[game.turn] + " płaci " + rent + "$ graczowi " + game.playername[game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                            }
                        }
                    }
                }
            }
            PlayerStatusRefresh();
            if (game.multiplayer)
                SendData();
        }
        private bool buyField(byte currentPlayerLocation)
        {
            if (game.playercash[game.turn] >= boardData.fieldPrice[currentPlayerLocation])
            {
                game.playercash[game.turn] = game.playercash[game.turn] - boardData.fieldPrice[currentPlayerLocation];
                game.fieldOwner[currentPlayerLocation] = game.turn;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool payRent(byte currentPlayerLocation, int rent)
        {
            if (game.playercash[game.turn] >= rent)
            {
                game.playercash[game.turn] = game.playercash[game.turn] - rent;
                game.playercash[game.fieldOwner[currentPlayerLocation]] = game.playercash[game.fieldOwner[currentPlayerLocation]] + rent;
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool payTax(byte currentPlayerLocation)
        {
            if (game.playercash[game.turn] >= boardData.fieldTaxCost[currentPlayerLocation])
            {
                game.playercash[game.turn] = game.playercash[game.turn] - boardData.fieldTaxCost[currentPlayerLocation];
                game.taxmoney = game.taxmoney + boardData.fieldTaxCost[currentPlayerLocation];
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool doChanceCard(byte chanceCard)
        {
            if (boardData.chanceAction[chanceCard] == 0)
            {
                // No function here currently
                return true;
            }
            if (boardData.chanceAction[chanceCard] == 1)
            {
                if (game.playercash[game.turn] >= boardData.chanceXValue[chanceCard])
                {
                    game.playercash[game.turn] = game.playercash[game.turn] - boardData.chanceXValue[chanceCard];
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                MessageBox.Show("Błąd związany z kartą szansy! Dalsza gra może zawierać błędy! Zrestartuj aplikację i zgłoś błąd twórcy!", "Ups...", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new System.InvalidOperationException("Błąd związany z kartą szansy! Dalsza gra może zawierać błędy! Zrestartuj aplikację i zgłoś błąd twórcy!");
            }
        }

        private bool doCommChestCard(byte commChestCard)
        {
            if (boardData.commChestAction[commChestCard] == 0)
            {
                game.playercash[game.turn] = game.playercash[game.turn] + boardData.commChestXValue[commChestCard];
                return true;
            }
            if (boardData.commChestAction[commChestCard] == 1)
            {
                if (game.playercash[game.turn] >= boardData.commChestXValue[commChestCard])
                {
                    game.playercash[game.turn] = game.playercash[game.turn] - boardData.commChestXValue[commChestCard];
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                MessageBox.Show("Błąd związany z kartą kasy społecznej! Dalsza gra może zawierać błędy! Zrestartuj aplikację i zgłoś błąd twórcy!", "Ups...", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new System.InvalidOperationException("Błąd związany z kartą kasy społecznej! Dalsza gra może zawierać błędy! Zrestartuj aplikację i zgłoś błąd twórcy!");
            }
        }
        private int calculateExtraFieldMultiplier(byte currentPlayerLocation)
        {
            if (game.fieldOwner[boardData.fieldSet1[currentPlayerLocation]] != game.turn)
            {
                return game.dice1 + game.dice2 * 4;
            }
            else
            {
                return game.dice1 + game.dice2 * 10;
            }
        }

        private bool payExtraFieldMultiplier(int calculatedMoney, byte currentPlayerLocation)
        {
            if (game.playercash[game.turn] >= calculatedMoney)
            {
                game.playercash[game.turn] = game.playercash[game.turn] - calculatedMoney;
                game.playercash[game.fieldOwner[currentPlayerLocation]] = game.playercash[game.fieldOwner[currentPlayerLocation]] + calculatedMoney;
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool buyHouse(byte selectedField)
        {
            bool hasSet = false;
            if (boardData.fieldSet2[selectedField] == 0)
            {
                if (game.fieldOwner[selectedField] == game.turn && game.fieldOwner[selectedField] != 4 && game.fieldOwner[selectedField] == game.fieldOwner[boardData.fieldSet1[selectedField]])
                {
                    hasSet = true;
                }
            }
            else
            {
                if (game.fieldOwner[selectedField] == game.turn && game.fieldOwner[selectedField] != 4 && game.fieldOwner[selectedField] == game.fieldOwner[boardData.fieldSet1[selectedField]] && game.fieldOwner[selectedField] == game.fieldOwner[boardData.fieldSet2[selectedField]])
                {
                    hasSet = true;
                }
            }
            if (hasSet == true)
            {
                if (selectedField > 0 && selectedField < 9)
                {
                    if (game.fieldHouse[selectedField] < 5)
                    {
                        if (game.playercash[game.turn] >= 50)
                        {
                            game.playercash[game.turn] = game.playercash[game.turn] - 50;
                            buyHouse2(selectedField);
                            return true;
                        }
                        return false;
                    }
                    return false;
                }
                else if (selectedField > 10 && selectedField < 20)
                {
                    if (game.fieldHouse[selectedField] < 5)
                    {
                        if (game.playercash[game.turn] >= 100)
                        {
                            game.playercash[game.turn] = game.playercash[game.turn] - 100;
                            buyHouse2(selectedField);
                            return true;
                        }
                        return false;
                    }
                    return false;
                }
                else if (selectedField > 20 && selectedField < 30)
                {
                    if (game.fieldHouse[selectedField] < 5)
                    {
                        if (game.playercash[game.turn] >= 150)
                        {
                            game.playercash[game.turn] = game.playercash[game.turn] - 150;
                            buyHouse2(selectedField);
                            return true;
                        }
                        return false;
                    }
                    return false;
                }
                else if (selectedField > 30 && selectedField < 40)
                {
                    if (game.fieldHouse[selectedField] < 5)
                    {
                        if (game.playercash[game.turn] >= 200)
                        {
                            game.playercash[game.turn] = game.playercash[game.turn] - 200;
                            buyHouse2(selectedField);
                            return true;
                        }
                        return false;
                    }
                    return false;
                }
                return false;
            }
            return false;
        }
        private void buyHouse2(byte selectedField)
        {
            game.fieldHouse[selectedField]++;
            GameLog.Text += game.playername[game.clientplayer] + " kupuje budynek w dzielnicy " + boardData.fieldName[game.selectedField] + Environment.NewLine + Environment.NewLine;
            if (game.multiplayer)
            {
                SendGameLog(game.playername[game.clientplayer] + " kupuje budynek w dzielnicy " + boardData.fieldName[game.selectedField] + Environment.NewLine + Environment.NewLine);
            }
            switch (game.fieldHouse[selectedField])
            {
                case 1:
                    DrawHouses(selectedField, 1);
                    break;

                case 2:
                    DrawHouses(selectedField, 2);
                    break;

                case 3:
                    DrawHouses(selectedField, 3);
                    break;

                case 4:
                    DrawHouses(selectedField, 4);
                    break;

                case 5:
                    DrawHouses(selectedField, 5);
                    break;

                default:
                    MessageBox.Show("Wystąpił błąd podczas wywołania instrukcji DrawHouses!", "Ups...", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw new InvalidOperationException("Wystąpił błąd podczas wywołania instrukcji DrawHouses!");
            }
            OverviewRefresh();
            PlayerStatusRefresh();
        }
        private bool sellHouse(byte selectedField)
        {
            bool hasSet = false;
            if (boardData.fieldSet2[selectedField] == 0)
            {
                if (game.fieldOwner[selectedField] == game.turn && game.fieldOwner[selectedField] != 4 && game.fieldOwner[selectedField] == game.fieldOwner[boardData.fieldSet1[selectedField]])
                {
                    hasSet = true;
                }
            }
            else
            {
                if (game.fieldOwner[selectedField] == game.turn && game.fieldOwner[selectedField] != 4 && game.fieldOwner[selectedField] == game.fieldOwner[boardData.fieldSet1[selectedField]] && game.fieldOwner[selectedField] == game.fieldOwner[boardData.fieldSet2[selectedField]])
                {
                    hasSet = true;
                }
            }
            if (hasSet == true)
            {
                if (selectedField > 0 && selectedField < 9)
                {
                    if (game.fieldHouse[selectedField] > 0)
                    {
                        game.playercash[game.turn] = game.playercash[game.turn] + 25;
                        sellHouse2(selectedField);
                        return true;
                    }
                    return false;
                }
                else if (selectedField > 10 && selectedField < 20)
                {
                    if (game.fieldHouse[selectedField] > 0)
                    {
                        game.playercash[game.turn] = game.playercash[game.turn] + 50;
                        sellHouse2(selectedField);
                        return true;
                    }
                    return false;
                }
                else if (selectedField > 20 && selectedField < 30)
                {
                    if (game.fieldHouse[selectedField] > 0)
                    {
                        game.playercash[game.turn] = game.playercash[game.turn] + 75;
                        sellHouse2(selectedField);
                        return true;
                    }
                    return false;
                }
                else if (selectedField > 30 && selectedField < 40)
                {
                    if (game.fieldHouse[selectedField] > 0)
                    {
                        game.playercash[game.turn] = game.playercash[game.turn] + 100;
                        sellHouse2(selectedField);
                        return true;
                    }
                    return false;
                }
                return false;
            }
            return false;
        }

        private void sellHouse2(byte selectedField)
        {
            game.fieldHouse[selectedField]--;
            GameLog.Text += game.playername[game.clientplayer] + " sprzedaje budynek w dzielnicy " + boardData.fieldName[game.selectedField] + Environment.NewLine + Environment.NewLine;
            if (game.multiplayer)
            {
                SendGameLog(game.playername[game.clientplayer] + " sprzedaje budynek w dzielnicy " + boardData.fieldName[game.selectedField] + Environment.NewLine + Environment.NewLine);
            }
            switch (game.fieldHouse[selectedField])
            {
                case 0:
                    DrawHouses(selectedField, 0);
                    break;

                case 1:
                    DrawHouses(selectedField, 1);
                    break;

                case 2:
                    DrawHouses(selectedField, 2);
                    break;

                case 3:
                    DrawHouses(selectedField, 3);
                    break;

                case 4:
                    DrawHouses(selectedField, 4);
                    break;

                default:
                    MessageBox.Show("Wystąpił błąd podczas wywołania instrukcji DrawHouses!", "Ups...", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw new InvalidOperationException("Wystąpił błąd podczas wywołania instrukcji DrawHouses!");
            }
            OverviewRefresh();
            PlayerStatusRefresh();
        }
        private bool sellField(byte selectedField)
        {
            if (game.fieldHouse[selectedField] == 0 && game.fieldOwner[selectedField] == game.turn)
            {
                game.playercash[game.fieldOwner[selectedField]] = game.playercash[game.fieldOwner[selectedField]] + (boardData.fieldPrice[selectedField] / 2);
                game.fieldOwner[selectedField] = 4;
                OverviewRefresh();
                PlayerStatusRefresh();
                return true;
            }
            return false;
        }

        // UI Programming
        // //////////////////////////////////////////////////////////////////////////////////////////////////////
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!game.dangerzone)
            {
                Button_ThrowDice.IsEnabled = false;
                ThrowDiceAndMove();
            }
            else
            {
                bankrupt();
            }
        }

        private void EndTurn_Click(object sender, RoutedEventArgs e)
        {
            if (!game.dangerzone)
            {
                game.turn++;
                EndTurn();
            }
            else
            {
                FieldCheck();
            }
        }

        private void OverviewRefresh()
        {
            Overview_Picture.Source = boardData.fieldIcon[game.selectedField];
            Overview_Name.Content = boardData.fieldName[game.selectedField];
            Overview_Price.Content = boardData.fieldPrice[game.selectedField] + " $";
            Overview_Houses.Content = game.fieldHouse[game.selectedField];
            Overview_Owner.Content = game.playername[game.fieldOwner[game.selectedField]];
            Overview_NoSetRent.Content = boardData.fieldNoSetRent[game.selectedField] + " $";
            Overview_1Rent.Content = boardData.field1Rent[game.selectedField] + " $";
            Overview_2Rent.Content = boardData.field2Rent[game.selectedField] + " $";
            Overview_3Rent.Content = boardData.field3Rent[game.selectedField] + " $";
            Overview_4Rent.Content = boardData.field4Rent[game.selectedField] + " $";
            Overview_HRent.Content = boardData.fieldHRent[game.selectedField] + " $";
        }
        private void PlayerStatusRefresh()
        {
            Label_Player1Cash.Content = game.playercash[0] + "$";
            Label_Player2Cash.Content = game.playercash[1] + "$";
            Label_Player3Cash.Content = game.playercash[2] + "$";
            Label_Player4Cash.Content = game.playercash[3] + "$";
        }
        private void Jump()
        {
            int xcord = 0;
            int ycord = 0;
            if (game.fieldPlayers[game.playerlocation[game.turn]] <= 1)
            {
                xcord = boardLocations.playerlocation(true, game.playerlocation[game.turn]);
                ycord = boardLocations.playerlocation(false, game.playerlocation[game.turn]);
            }
            else if (game.fieldPlayers[game.playerlocation[game.turn]] == 2)
            {
                xcord = boardLocations.playerlocation(true, game.playerlocation[game.turn]) + 22;
                ycord = boardLocations.playerlocation(false, game.playerlocation[game.turn]);
            }
            else if (game.fieldPlayers[game.playerlocation[game.turn]] == 3)
            {
                xcord = boardLocations.playerlocation(true, game.playerlocation[game.turn]);
                ycord = boardLocations.playerlocation(false, game.playerlocation[game.turn]) + 22;
            }
            else if (game.fieldPlayers[game.playerlocation[game.turn]] >= 4)
            {
                xcord = boardLocations.playerlocation(true, game.playerlocation[game.turn]) + 22;
                ycord = boardLocations.playerlocation(false, game.playerlocation[game.turn]) + 22;
            }
            switch (game.turn)
            {
                case 0:
                    Canvas.SetLeft(Player1, xcord);
                    Canvas.SetTop(Player1, ycord);
                    break;

                case 1:
                    Canvas.SetLeft(Player2, xcord);
                    Canvas.SetTop(Player2, ycord);
                    break;

                case 2:
                    Canvas.SetLeft(Player3, xcord);
                    Canvas.SetTop(Player3, ycord);
                    break;

                case 3:
                    Canvas.SetLeft(Player4, xcord);
                    Canvas.SetTop(Player4, ycord);
                    break;
            }
        }

        private void DrawOwner(byte field, byte status)
        {
            switch (field)
            {
                case 1:
                    if (status == 0)
                        Field2.Source = new BitmapImage(new Uri(@"Resources\BluePlayer.png", UriKind.Relative));
                    if (status == 1)
                        Field2.Source = new BitmapImage(new Uri(@"Resources\GreenPlayer.png", UriKind.Relative));
                    if (status == 2)
                        Field2.Source = new BitmapImage(new Uri(@"Resources\YellowPlayer.png", UriKind.Relative));
                    if (status == 3)
                        Field2.Source = new BitmapImage(new Uri(@"Resources\RedPlayer.png", UriKind.Relative));
                    if (status == 4)
                        Field2.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    break;

                case 3:
                    if (status == 0)
                        Field4.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field4.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field4.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field4.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field4.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                case 6:
                    if (status == 0)
                        Field7.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field7.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field7.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field7.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field7.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                case 8:
                    if (status == 0)
                        Field9.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field9.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field9.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field9.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field9.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                case 9:
                    if (status == 0)
                        Field10.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field10.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field10.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field10.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field10.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                case 11:
                    if (status == 0)
                        Field12.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field12.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field12.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field12.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field12.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                case 13:
                    if (status == 0)
                        Field14.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field14.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field14.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field14.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field14.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                case 14:
                    if (status == 0)
                        Field15.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field15.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field15.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field15.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field15.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                case 16:
                    if (status == 0)
                        Field17.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field17.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field17.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field17.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field17.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                case 18:
                    if (status == 0)
                        Field19.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field19.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field19.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field19.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field19.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                case 19:
                    if (status == 0)
                        Field20.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field20.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field20.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field20.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field20.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                case 21:
                    if (status == 0)
                        Field22.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field22.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field22.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field22.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field22.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                case 23:
                    if (status == 0)
                        Field24.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field24.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field24.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field24.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field24.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                case 24:
                    if (status == 0)
                        Field25.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field25.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field25.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field25.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field25.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                case 26:
                    if (status == 0)
                        Field27.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field27.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field27.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field27.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field27.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                case 27:
                    if (status == 0)
                        Field28.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field28.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field28.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field28.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field28.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                case 29:
                    if (status == 0)
                        Field30.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field30.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field30.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field30.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field30.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                case 31:
                    if (status == 0)
                        Field32.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field32.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field32.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field32.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field32.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                case 32:
                    if (status == 0)
                        Field33.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field33.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field33.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field33.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field33.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                case 34:
                    if (status == 0)
                        Field35.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field35.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field35.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field35.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field35.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                case 37:
                    if (status == 0)
                        Field38.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field38.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field38.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field38.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field38.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                case 39:
                    if (status == 0)
                        Field40.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field40.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field40.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field40.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field40.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    break;

                default:
                    MessageBox.Show("Wystąpił błąd podczas renderowania informacji o właścicielu pola!", "Ups...", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw new InvalidOperationException("Wystąpił błąd podczas renderowania informacji o właścicielu pola!");
            }
        }

        private void DrawHouses(byte field, byte status)
        {
            switch (field)
            {
                case 1:
                    if (status == 0)
                        Field2.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field2.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field2.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field2.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field2.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field2.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 3:
                    if (status == 0)
                        Field4.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field4.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field4.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field4.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field4.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field4.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 6:
                    if (status == 0)
                        Field7.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field7.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field7.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field7.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field7.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field7.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 8:
                    if (status == 0)
                        Field9.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field9.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field9.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field9.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field9.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field9.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 9:
                    if (status == 0)
                        Field10.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field10.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field10.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field10.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field10.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field10.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 11:
                    if (status == 0)
                        Field12.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field12.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field12.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field12.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field12.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field12.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 13:
                    if (status == 0)
                        Field14.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field14.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field14.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field14.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field14.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field14.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 14:
                    if (status == 0)
                        Field15.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field15.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field15.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field15.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field15.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field15.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 16:
                    if (status == 0)
                        Field17.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field17.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field17.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field17.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field17.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field17.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 18:
                    if (status == 0)
                        Field19.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field19.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field19.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field19.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field19.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field19.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 19:
                    if (status == 0)
                        Field20.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field20.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field20.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field20.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field20.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field20.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 21:
                    if (status == 0)
                        Field22.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field22.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field22.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field22.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field22.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field22.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 23:
                    if (status == 0)
                        Field24.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field24.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field24.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field24.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field24.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field24.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 24:
                    if (status == 0)
                        Field25.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field25.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field25.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field25.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field25.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field25.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 26:
                    if (status == 0)
                        Field27.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field27.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field27.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field27.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field27.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field27.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 27:
                    if (status == 0)
                        Field28.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field28.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field28.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field28.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field28.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field28.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 29:
                    if (status == 0)
                        Field30.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field30.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field30.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field30.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field30.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field30.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 31:
                    if (status == 0)
                        Field32.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field32.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field32.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field32.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field32.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field32.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 32:
                    if (status == 0)
                        Field33.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field33.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field33.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field33.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field33.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field33.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 34:
                    if (status == 0)
                        Field35.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field35.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field35.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field35.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field35.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field35.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 37:
                    if (status == 0)
                        Field38.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field38.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field38.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field38.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field38.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field38.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                case 39:
                    if (status == 0)
                        Field40.Source = new BitmapImage(new Uri(@"Resources\NoAlpha.png", UriKind.Relative));
                    if (status == 1)
                        Field40.Source = new BitmapImage(new Uri(@"Resources\House1.png", UriKind.Relative));
                    if (status == 2)
                        Field40.Source = new BitmapImage(new Uri(@"Resources\House2.png", UriKind.Relative));
                    if (status == 3)
                        Field40.Source = new BitmapImage(new Uri(@"Resources\House3.png", UriKind.Relative));
                    if (status == 4)
                        Field40.Source = new BitmapImage(new Uri(@"Resources\House4.png", UriKind.Relative));
                    if (status == 5)
                        Field40.Source = new BitmapImage(new Uri(@"Resources\Trivago.png", UriKind.Relative));
                    break;

                default:
                    MessageBox.Show("Wystąpił błąd podczas renderowania domów!", "Ups...", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw new InvalidOperationException("Wystąpił błąd podczas renderowania domów!");
            }
        }
        private void Field1_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 0;
            OverviewRefresh();
        }

        private void Field2_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 1;
            OverviewRefresh();
        }

        private void Field3_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 2;
            OverviewRefresh();
        }

        private void Field4_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 3;
            OverviewRefresh();
        }

        private void Field5_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 4;
            OverviewRefresh();
        }

        private void Field6_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 5;
            OverviewRefresh();
        }

        private void Field7_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 6;
            OverviewRefresh();
        }

        private void Field8_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 7;
            OverviewRefresh();
        }

        private void Field9_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 8;
            OverviewRefresh();
        }

        private void Field10_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 9;
            OverviewRefresh();
        }

        private void Field11_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 10;
            OverviewRefresh();
        }

        private void Field12_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 11;
            OverviewRefresh();
        }

        private void Field13_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 12;
            OverviewRefresh();
        }

        private void Field14_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 13;
            OverviewRefresh();
        }

        private void Field15_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 14;
            OverviewRefresh();
        }

        private void Field16_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 15;
            OverviewRefresh();
        }

        private void Field17_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 16;
            OverviewRefresh();
        }

        private void Field18_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 17;
            OverviewRefresh();
        }

        private void Field19_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 18;
            OverviewRefresh();
        }

        private void Field20_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 19;
            OverviewRefresh();
        }

        private void Field21_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 20;
            OverviewRefresh();
        }

        private void Field22_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 21;
            OverviewRefresh();
        }

        private void Field23_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 22;
            OverviewRefresh();
        }

        private void Field24_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 23;
            OverviewRefresh();
        }

        private void Field25_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 24;
            OverviewRefresh();
        }

        private void Field26_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 25;
            OverviewRefresh();
        }

        private void Field27_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 26;
            OverviewRefresh();
        }

        private void Field28_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 27;
            OverviewRefresh();
        }

        private void Field29_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 28;
            OverviewRefresh();
        }

        private void Field30_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 29;
            OverviewRefresh();
        }

        private void Field31_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 30;
            OverviewRefresh();
        }

        private void Field32_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 31;
            OverviewRefresh();
        }

        private void Field33_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 32;
            OverviewRefresh();
        }

        private void Field34_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 33;
            OverviewRefresh();
        }

        private void Field35_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 34;
            OverviewRefresh();
        }

        private void Field36_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 35;
            OverviewRefresh();
        }

        private void Field37_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 36;
            OverviewRefresh();
        }

        private void Field38_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 37;
            OverviewRefresh();
        }

        private void Field39_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 38;
            OverviewRefresh();
        }

        private void Field40_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedField = 39;
            OverviewRefresh();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            CheatWindow cheatWindow = new CheatWindow();
            cheatWindow.ShowDialog();
            Button_ThrowDice.IsEnabled = false;
            diceScore = Convert.ToByte(cheat);
            DiceScore.Content = diceScore;
            wait.Start();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //Spotify spotify = new Spotify();
            //spotify.Show();
        }

        private void Field1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                MessageBox.Show("Na tym polu nie możesz kupować domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                MessageBox.Show("Nie możesz sprzedać tego pola!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Field2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(1))
                {
                    MessageBox.Show("Nie można kupić budynku!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(1))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field3_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                MessageBox.Show("Na tym polu nie możesz kupować domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                MessageBox.Show("Nie możesz sprzedać tego pola!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Field4_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(3))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(3))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field5_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                MessageBox.Show("Na tym polu nie możesz kupować domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                MessageBox.Show("Nie możesz sprzedać tego pola!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Field6_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                MessageBox.Show("Na tym polu nie możesz kupować domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(5))
                {
                    MessageBox.Show("Nie można sprzedać tego pola!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field7_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(6))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(6))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field8_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                MessageBox.Show("Na tym polu nie możesz kupować domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                MessageBox.Show("Nie możesz sprzedać tego pola!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Field9_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(8))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(8))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field10_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(9))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(9))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field11_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                MessageBox.Show("Na tym polu nie możesz kupować domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                MessageBox.Show("Nie możesz sprzedać tego pola!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Field12_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(11))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(11))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field13_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                MessageBox.Show("Na tym polu nie możesz kupować domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(12))
                {
                    MessageBox.Show("Nie można sprzedać tego pola!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field14_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(13))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(13))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field15_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(14))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(14))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field16_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                MessageBox.Show("Na tym polu nie możesz kupować domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(15))
                {
                    MessageBox.Show("Nie można sprzedać tego pola!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field17_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(16))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(16))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field18_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                MessageBox.Show("Na tym polu nie możesz kupować domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                MessageBox.Show("Nie możesz sprzedać tego pola!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Field19_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(18))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(18))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field20_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(19))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(19))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field21_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                MessageBox.Show("Na tym polu nie możesz kupować domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                MessageBox.Show("Nie możesz sprzedać tego pola!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Field22_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(21))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(21))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field23_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                MessageBox.Show("Na tym polu nie możesz kupować domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                MessageBox.Show("Nie możesz sprzedać tego pola!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Field24_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(23))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(23))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field25_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(24))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(24))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field26_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                MessageBox.Show("Na tym polu nie możesz kupować domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(25))
                {
                    MessageBox.Show("Nie można sprzedać tego pola!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field27_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(26))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(26))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field28_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(27))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(27))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field29_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                MessageBox.Show("Na tym polu nie możesz kupować domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(28))
                {
                    MessageBox.Show("Nie można sprzedać tego pola!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field30_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(29))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(29))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field31_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                MessageBox.Show("Na tym polu nie możesz kupować domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                MessageBox.Show("Nie możesz sprzedać tego pola!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Field32_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(31))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(31))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field33_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(32))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(32))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field34_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                MessageBox.Show("Na tym polu nie możesz kupować domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                MessageBox.Show("Nie możesz sprzedać tego pola!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Field35_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(34))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(34))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field36_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                MessageBox.Show("Na tym polu nie możesz kupować domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(35))
                {
                    MessageBox.Show("Nie można sprzedać tego pola!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field37_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                MessageBox.Show("Na tym polu nie możesz kupować domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                MessageBox.Show("Nie możesz sprzedać tego pola!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Field38_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(37))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(37))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Field39_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                MessageBox.Show("Na tym polu nie możesz kupować domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                MessageBox.Show("Nie możesz sprzedać tego pola!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Field40_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer && !game.sellmode)
            {
                if (!buyHouse(39))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (game.turn == game.clientplayer && game.sellmode)
            {
                if (!sellField(39))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuItem_StartNewSingle(object sender, RoutedEventArgs e)
        {
            NewSingleplayerGame newSingleplayerGame = new NewSingleplayerGame();
            newSingleplayerGame.ShowDialog();
            game.playername[0] = newSingleplayerGame.TextBox_Player1.Text;
            game.playername[1] = newSingleplayerGame.TextBox_Player2.Text;
            game.playername[2] = newSingleplayerGame.TextBox_Player3.Text;
            game.playername[3] = newSingleplayerGame.TextBox_Player4.Text;
            game.playerAvailable[0] = true;
            game.playerAvailable[1] = true;
            game.playerAvailable[2] = Convert.ToBoolean(newSingleplayerGame.CheckBox_AIActive1.IsChecked);
            game.playerAvailable[3] = Convert.ToBoolean(newSingleplayerGame.CheckBox_AIActive2.IsChecked);
            game.multiplayer = false;
            //game.playboardTheme = newSinglePlayerGame.ListBox_PlayboardTheme.SelectedIndex;
            StartNewGame();
        }
        private void Button_MouseMode_Click(object sender, RoutedEventArgs e)
        {
            if (!game.sellmode)
            {
                game.sellmode = true;
                Button_MouseMode.Content = "Tryb sprzedawania ulic";
            }
            else
            {
                game.sellmode = false;
                Button_MouseMode.Content = "Tryb budowania domów";
            }
        }

        private void Field1_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Na tym polu nie możesz sprzedawać domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void Field2_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(1))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field3_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Na tym polu nie możesz sprzedawać domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void Field4_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(3))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field5_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Na tym polu nie możesz sprzedawać domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void Field6_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Na tym polu nie możesz sprzedawać domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void Field7_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(6))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field8_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Na tym polu nie możesz sprzedawać domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void Field9_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(8))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field10_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(9))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field11_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Na tym polu nie możesz sprzedawać domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void Field12_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(11))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field13_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Na tym polu nie możesz sprzedawać domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void Field14_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(13))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field15_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(14))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field16_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Na tym polu nie możesz sprzedawać domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void Field17_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(16))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field18_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Na tym polu nie możesz sprzedawać domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void Field19_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(18))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field20_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(19))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field21_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Na tym polu nie możesz sprzedawać domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void Field22_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(21))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field23_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Na tym polu nie możesz sprzedawać domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void Field24_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(23))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field25_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(24))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field26_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Na tym polu nie możesz sprzedawać domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void Field27_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(26))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field28_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(27))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field29_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Na tym polu nie możesz sprzedawać domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void Field30_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(29))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field31_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Na tym polu nie możesz sprzedawać domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void Field32_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(31))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field33_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(32))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field34_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Na tym polu nie możesz sprzedawać domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void Field35_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(34))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field36_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Na tym polu nie możesz sprzedawać domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void Field37_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Na tym polu nie możesz sprzedawać domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void Field38_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(37))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Field39_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Na tym polu nie możesz sprzedawać domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void Field40_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (game.turn == game.clientplayer)
            {
                if (!sellHouse(39))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
