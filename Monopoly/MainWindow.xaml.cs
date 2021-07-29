using NetComm;
using System;
using System.IO;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Collections.Generic;

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
        public static bool connectedToServer = false;
        public static int cheat = 0;
        public static bool cheat_allowTradeWindow = false;
        public static List<string> ThemeBoards;

        Random rng = new Random();
        byte diceScore;
        string playboardTheme = "Monopoly Standard";
        public static string currentThemeDir = "Resources";

        DispatcherTimer wait = new DispatcherTimer();
        DispatcherTimer reload = new DispatcherTimer();

        BoardLocations boardLocations = new BoardLocations();
        Audio audio = new Audio();
        AI ai = new AI();
        public MainWindow()
        {
            this.Dispatcher.UnhandledException += App_DispatcherUnhandledException;
            for (int i = 0; i < 40; i++)
            {
                Game.fieldOwner[i] = 4;
                Game.fieldHouse[i] = 0;
                Game.fieldPlayers[i] = 0;
            }
            Game.fieldPlayers[0] = 4;
            BoardData.gameDataWriter();
            wait.Interval = TimeSpan.FromMilliseconds(300);
            wait.Tick += JumpingAnimation_Tick;
            reload.Interval = TimeSpan.FromSeconds(10);
            reload.Tick += Reload_Tick;
            ThemeBoards = GetAvailableBoards();
            InitializeComponent();
            audio.music.Open(new Uri(audio.musicfile, UriKind.Relative));
            audio.sfx.Volume = 0.5;
            audio.music.Volume = audio.sfx.Volume / 2;
            audio.playMusic();
            audio.music.MediaEnded += Sfx_MediaEnded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MenuItem_Volume50.IsChecked = true;
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if(!Directory.Exists("logs"))
            {
                Directory.CreateDirectory("logs");
            }
            FileStream fs = File.Create(@"logs\crash_report_" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss") + ".crash");
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(e.Exception);
            sw.Close();
            MessageBox.Show("Crash! Unhandled exception occured.\nCrash report saved to file:\n" + fs.Name, "Boom!", MessageBoxButton.OK, MessageBoxImage.Error);
            // Prevent default unhandled exception processing
            e.Handled = true;
            System.Windows.Application.Current.Shutdown();
        }

        private void Sfx_MediaEnded(object sender, EventArgs e)
        {
            //sfx.Play();
        }

        private void LoadGame()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "monopolysave";
            if(openFileDialog.ShowDialog() == true)
            {
                FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                string line;
                int index = 0;
                while((line = sr.ReadLine()) != null)
                {
                    switch(index)
                    {
                        case 0:
                            Game.playername = line.Split();
                            break;
                    }
                    index++;
                }
                sr.Close();
                Button_ThrowDice.IsEnabled = true;
            }
        }
        private void SaveGame()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "monopolysave";
            if(saveFileDialog.ShowDialog() == true)
            {
                FileStream fs = File.Create(saveFileDialog.FileName);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(Game.playername);
                sw.WriteLine(Game.playerlocation);
                sw.WriteLine(Game.playercash);
                sw.WriteLine(Game.playerRailroadOwned);
                sw.WriteLine(Game.playerArrestedTurns);
                sw.WriteLine(Game.playerAvailable);
                sw.WriteLine(Game.playerBankrupt);
                sw.WriteLine(Game.playerBankruptNeededToWin);
                sw.WriteLine(Game.clientplayer);
                sw.WriteLine(Game.turn);
                sw.WriteLine(Game.dice1);
                sw.WriteLine(Game.dice2);
                sw.WriteLine(Game.selectedField);
                sw.WriteLine(Game.currentFieldPrice);
                sw.WriteLine(Game.currentFieldForSale);
                sw.WriteLine(Game.fieldHouse);
                sw.WriteLine(Game.fieldOwner);
                sw.WriteLine(Game.fieldPlayers);
                sw.WriteLine(Game.taxmoney);
                sw.WriteLine(Game.sellmode);
                sw.WriteLine(Game.dangerzone);
                sw.Close();
            }
        }
        private List<string> GetAvailableBoards()
        {
            List<string> AvailableBoards = new List<string>();
            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
            DirectoryInfo[] dirs = di.GetDirectories();
            foreach(DirectoryInfo dir in dirs)
            {
                FileInfo[] files = dir.GetFiles();
                foreach(FileInfo file in files)
                {
                    if(file.Name == "BoardName.mtf")
                    {
                        AvailableBoards.Add(File.ReadAllText(file.FullName)); 
                    }
                }
            }
            return AvailableBoards;
        }
        private void LoadCurrentThemeDir()
        {
            foreach(string x in ThemeBoards)
            {
                string[] splittedText = x.Split(';');
                if(splittedText[0] == playboardTheme)
                {
                    currentThemeDir = splittedText[1];
                }
            }
        }

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

        // Game CODE
        // //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void StartNewGame()
        {
            if (Game.playerAvailable[0])
            {
                Player1.Visibility = Visibility.Visible;
                Player1_Icon.Visibility = Visibility.Visible;
                Label_Player1Cash.Visibility = Visibility.Visible;
                Label_Player1Name.Visibility = Visibility.Visible;
                Label_Player1Name.Content = Game.playername[0];
                Game.playerBankruptNeededToWin++;
            }
            if (Game.playerAvailable[1])
            {
                Player2.Visibility = Visibility.Visible;
                Player2_Icon.Visibility = Visibility.Visible;
                Label_Player2Cash.Visibility = Visibility.Visible;
                Label_Player2Name.Visibility = Visibility.Visible;
                Label_Player2Name.Content = Game.playername[1];
                Game.playerBankruptNeededToWin++;
            }
            if (Game.playerAvailable[2])
            {
                Player3.Visibility = Visibility.Visible;
                Player3_Icon.Visibility = Visibility.Visible;
                Label_Player3Cash.Visibility = Visibility.Visible;
                Label_Player3Name.Visibility = Visibility.Visible;
                Label_Player3Name.Content = Game.playername[2];
                Game.playerBankruptNeededToWin++;
            }
            if (Game.playerAvailable[3])
            {
                Player4.Visibility = Visibility.Visible;
                Player4_Icon.Visibility = Visibility.Visible;
                Label_Player4Cash.Visibility = Visibility.Visible;
                Label_Player4Name.Visibility = Visibility.Visible;
                Label_Player4Name.Content = Game.playername[3];
                Game.playerBankruptNeededToWin++;
            }
            if (!connectedToServer)
            {
                Game.clientplayer = 0;
                Label_Player1Name.FontWeight = FontWeights.Bold;
                Button_ThrowDice.IsEnabled = true;
            }
            Game.playercash[0] = 1500;
            Game.playercash[1] = 1500;
            Game.playercash[2] = 1500;
            Game.playercash[3] = 1500;
            Game.playerlocation[0] = 0;
            Game.playerlocation[1] = 0;
            Game.playerlocation[2] = 0;
            Game.playerlocation[3] = 0;
            for (int i = 0; i < 41; i++)
            {
                Game.fieldOwner[i] = 4;
                Game.fieldHouse[i] = 0;
                Game.fieldPlayers[i] = 0;
            }
            Game.fieldPlayers[0] = 4;
            BoardData.gameDataWriter();
            //GameCanvas.Children.RemoveRange(44, 100);
            GameLog.Text = "";
            audio.music.Stop();
        }
        private void TurnCheck()
        {
            if (Game.playerAvailable[3])
            {
                if (Game.turn > 3)
                {
                    Game.turn = 0;
                }
            }
            else if (Game.playerAvailable[2])
            {
                if (Game.turn > 2)
                {
                    Game.turn = 0;
                }
            }
            else
            {
                if (Game.turn > 1)
                {
                    Game.turn = 0;
                }
            }
        }

        private void EndTurn()
        {
            Button_EndTurn.IsEnabled = false;
            Button_Trade.IsEnabled = false;
            TurnCheck();
            if (Game.turn == 0 && Game.playerArrestedTurns[Game.turn] == 0)
            {
                if (Game.playerBankrupt[Game.turn] == false)
                    EnableMove();
                else
                    Game.turn++;
            }
            else if (Game.turn == 0 && Game.playerArrestedTurns[Game.turn] != 0)
            {
                DisableMove();
            }
            if (Game.turn == 1 && Game.playerArrestedTurns[Game.turn] == 0)
            {
                if (Game.playerBankrupt[Game.turn] == false)
                    EnableMove();
                else
                {
                    Game.turn++;
                    TurnCheck();
                }
            }
            else if (Game.turn == 1 && Game.playerArrestedTurns[Game.turn] != 0)
            {
                DisableMove();
            }
            if (Game.turn == 2 && Game.playerArrestedTurns[Game.turn] == 0)
            {
                if (Game.playerBankrupt[Game.turn] == false)
                    EnableMove();
                else
                {
                    Game.turn++;
                    TurnCheck();
                }
            }
            else if (Game.turn == 2 && Game.playerArrestedTurns[Game.turn] != 0)
            {
                DisableMove();
            }
            if (Game.turn == 3 && Game.playerArrestedTurns[Game.turn] == 0)
            {
                if (Game.playerBankrupt[Game.turn] == false)
                    EnableMove();
                else
                {
                    Game.turn++;
                    TurnCheck();
                    EndTurn();
                }
            }
            else if (Game.turn == 3 && Game.playerArrestedTurns[Game.turn] != 0)
            {
                DisableMove();
            }
        }

        private void bankrupt()
        {
            GameLog.Text += Game.playername[Game.turn] + " OGŁASZA BANKRUCTWO!" + Environment.NewLine + Environment.NewLine;
            if (Game.multiplayer)
            {
                SendGameLog(Game.playername[Game.turn] + " OGŁASZA BANKRUCTWO!" + Environment.NewLine + Environment.NewLine);
            }
            if (Game.turn == Game.clientplayer)
            {
                Button_ThrowDice.IsEnabled = false;
                Button_EndTurn.IsEnabled = false;
            }
            Game.playerBankrupt[Game.turn] = true;
            Game.fieldPlayers[Game.playerlocation[Game.turn]]--;
            switch (Game.turn)
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
            Game.playerBankruptNeededToWin--;
            LeaveDangerZone();
            if (Game.playerBankruptNeededToWin <= 1)
            {
                MessageBox.Show("Koniec gry!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                if (Game.multiplayer)
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
            Game.sellmode = true;
            Button_MouseMode.Content = "Tryb sprzedawania ulic";
            audio.playSFX("incorrect");
            if (Game.turn == Game.clientplayer)
                MessageBox.Show("Znajdujesz się w strefie zagrożenia! Sprzedaj budynki lub ulice aby móc zapłacić. Jeżeli nie możesz zrobić nic więcej, ogłoś swoje bankructwo", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            Game.dangerzone = true;

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
            Game.sellmode = false;
            Button_MouseMode.Content = "Tryb budowania domów";
            Game.dangerzone = false;
        }
        private void EnableMove()
        {
            if (Game.clientplayer == Game.turn)
            {
                Game.sellmode = false;
                Button_MouseMode.Content = "Tryb budowania domów";
                Button_ThrowDice.IsEnabled = true;
                //if(!Game.multiplayer)
                //Button_Trade.IsEnabled = true;
                GameLog.Text += "TWOJA TURA!" + Environment.NewLine + Environment.NewLine;
                audio.playSFX("correct");
            }
            else
            {
                if (!Game.multiplayer)
                {
                    ThrowDiceAndMove();
                }
            }
            if (Game.multiplayer)
                SendData();
        }

        private void DisableMove()
        {
            Game.playerArrestedTurns[Game.turn]--;
            if (Game.multiplayer)
                SendData();
            if (Game.clientplayer == Game.turn)
            {
                Button_EndTurn.IsEnabled = true;
                if (Game.playerArrestedTurns[Game.turn] == 0)
                {
                    Game.playerlocation[Game.turn] = 10;
                }
            }
            else
            {
                if (!Game.multiplayer)
                {
                    if (Game.playerArrestedTurns[Game.turn] == 0)
                    {
                        Game.playerlocation[Game.turn] = 10;
                    }
                    Game.turn++;
                    EndTurn();
                }
            }
        }

        private void ThrowDiceAndMove()
        {
            Game.dice1 = Convert.ToByte(rng.Next(1, 7));
            Game.dice2 = Convert.ToByte(rng.Next(1, 7));
            DiceShow(Game.dice1, Game.dice2);
            diceScore = Convert.ToByte(Game.dice1 + Game.dice2);
            DiceScore.Content = diceScore;
            if (Game.multiplayer)
            {
                SendData();
            }
            wait.Start();
        }

        private void JumpingAnimation_Tick(object sender, EventArgs e)
        {
            if (diceScore > 0)
            {
                Game.playerlocation[Game.turn]++;
                Game.fieldPlayers[Game.playerlocation[Game.turn] - 1]--;
                Game.fieldPlayers[Game.playerlocation[Game.turn]]++;
                if (Game.playerlocation[Game.turn] >= 40)
                {
                    Game.fieldPlayers[40]--;
                    Game.fieldPlayers[0]++;
                    Game.playerlocation[Game.turn] = 0;
                    Game.playercash[Game.turn] = Game.playercash[Game.turn] + 200;
                    GameLog.Text += Game.playername[Game.turn] + " otrzymuje 200$ za przejście przez start!" + Environment.NewLine + Environment.NewLine;
                    if (Game.multiplayer)
                    {
                        SendGameLog(Game.playername[Game.turn] + " otrzymuje 200$ za przejście przez start!" + Environment.NewLine + Environment.NewLine);
                    }
                    PlayerStatusRefresh();
                }
                Jump();
                diceScore--;
                if (Game.multiplayer)
                    SendMoveData();
            }
            else
            {
                wait.Stop();
                Game.selectedField = Game.playerlocation[Game.turn];
                OverviewRefresh();
                FieldCheck();
                if (Game.turn != 0 && !Game.multiplayer)
                {
                    Game.turn++;
                    if (Game.turn > 3)
                    {
                        Game.turn = 0;
                    }
                    EndTurn();
                }
                else
                {
                    Button_EndTurn.IsEnabled = true;
                }
                if (Game.multiplayer)
                    SendData();
            }
        }

        private void FieldCheck()
        {
            byte currentPlayerLocation = Game.playerlocation[Game.turn];
            int rent = BoardData.fieldNoSetRent[currentPlayerLocation];
            if (BoardData.fieldChance[currentPlayerLocation] == true)
            {
                byte chanceCard = Convert.ToByte(rng.Next(0, 5));
                if (Game.turn == Game.clientplayer)
                {
                    MessageBox.Show(BoardData.chanceText[chanceCard], "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (!doChanceCard(chanceCard))
                    {
                        MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                        Game.dangerzone = true;
                        DangerZone();
                    }
                    else
                    {
                        if (Game.dangerzone == true)
                        {
                            LeaveDangerZone();
                        }
                        GameLog.Text += Game.playername[Game.turn] + " otrzymuje kartę szansy: " + BoardData.chanceText[chanceCard] + "!" + Environment.NewLine + Environment.NewLine;
                        if (Game.multiplayer)
                        {
                            SendGameLog(Game.playername[Game.turn] + " otrzymuje kartę szansy: " + BoardData.chanceText[chanceCard] + "!" + Environment.NewLine + Environment.NewLine);
                        }
                    }
                }
                else
                {
                    if (!Game.multiplayer)
                    {
                        if (doChanceCard(chanceCard))
                        {
                            GameLog.Text += Game.playername[Game.turn] + " otrzymuje kartę szansy: " + BoardData.chanceText[chanceCard] + "!" + Environment.NewLine + Environment.NewLine;
                        }
                        else
                        {
                            MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                            bankrupt();
                        }
                    }
                }

            }
            else if (BoardData.fieldCommChest[currentPlayerLocation] == true)
            {
                byte commChestCard = Convert.ToByte(rng.Next(0, 11));
                if (Game.turn == Game.clientplayer)
                {
                    MessageBox.Show(BoardData.commChestText[commChestCard], "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (!doCommChestCard(commChestCard))
                    {
                        MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                        Game.dangerzone = true;
                        DangerZone();
                    }
                    else
                    {
                        if (Game.dangerzone == true)
                        {
                            LeaveDangerZone();
                        }
                        GameLog.Text += Game.playername[Game.turn] + " otrzymuje kartę kasy społecznej: " + BoardData.commChestText[commChestCard] + "!" + Environment.NewLine + Environment.NewLine;
                        if (Game.multiplayer)
                        {
                            SendGameLog(Game.playername[Game.turn] + " otrzymuje kartę kasy społecznej: " + BoardData.commChestText[commChestCard] + "!" + Environment.NewLine + Environment.NewLine);
                        }
                    }
                }
                else
                {
                    if (!Game.multiplayer)
                    {
                        if (doCommChestCard(commChestCard))
                        {
                            GameLog.Text += Game.playername[Game.turn] + " otrzymuje kartę kasy społecznej: " + BoardData.commChestText[commChestCard] + "!" + Environment.NewLine + Environment.NewLine;
                        }
                        else
                        {
                            MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                            bankrupt();
                        }
                    }
                }
            }
            else if (BoardData.fieldRailroad[currentPlayerLocation] == true)
            {
                if (Game.fieldOwner[currentPlayerLocation] == 4)
                {
                    if (Game.turn == Game.clientplayer)
                    {
                        MessageBoxResult result = MessageBox.Show("Czy chcesz kupić " + BoardData.fieldName[currentPlayerLocation] + "?", "Monopoly", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        switch (result)
                        {
                            case MessageBoxResult.Yes:
                                if (!buyField(currentPlayerLocation))
                                {
                                    MessageBox.Show("Nie stać Cię na ten dworzec!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                else
                                {
                                    Game.playerRailroadOwned[Game.turn]++;
                                    GameLog.Text += Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                                    if (Game.multiplayer)
                                    {
                                        SendGameLog(Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine);
                                    }
                                }
                                break;

                            case MessageBoxResult.No:
                                break;
                        }
                    }
                    else
                    {
                        if (!Game.multiplayer)
                        {
                            if (buyField(currentPlayerLocation))
                            {
                                Game.playerRailroadOwned[Game.turn]++;
                                GameLog.Text += Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;

                            }
                        }
                    }
                }
                else if (Game.fieldOwner[currentPlayerLocation] != Game.turn)
                {
                    if (Game.playerRailroadOwned[Game.fieldOwner[currentPlayerLocation]] == 1)
                    {
                        rent = BoardData.field1Rent[currentPlayerLocation];
                    }
                    else if (Game.playerRailroadOwned[Game.fieldOwner[currentPlayerLocation]] == 2)
                    {
                        rent = BoardData.field2Rent[currentPlayerLocation];
                    }
                    else if (Game.playerRailroadOwned[Game.fieldOwner[currentPlayerLocation]] == 3)
                    {
                        rent = BoardData.field3Rent[currentPlayerLocation];
                    }
                    else if (Game.playerRailroadOwned[Game.fieldOwner[currentPlayerLocation]] >= 4)
                    {
                        rent = BoardData.field4Rent[currentPlayerLocation];
                    }
                    if (Game.turn == Game.clientplayer)
                    {
                        MessageBox.Show("Stanąłeś na dworcu gracza " + Game.fieldOwner[currentPlayerLocation] + ". Musisz mu zapłacić: " + rent, "Monopoly", MessageBoxButton.OK, MessageBoxImage.Warning);
                        if (!payRent(currentPlayerLocation, rent))
                        {
                            MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                            Game.dangerzone = true;
                            DangerZone();
                        }
                        else
                        {
                            if (Game.dangerzone == true)
                            {
                                LeaveDangerZone();
                            }
                            GameLog.Text += Game.playername[Game.turn] + " płaci " + rent + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                            if (Game.multiplayer)
                            {
                                SendGameLog(Game.playername[Game.turn] + " płaci " + rent + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine);
                            }
                        }
                    }
                    else
                    {
                        if (!Game.multiplayer)
                        {
                            if (!payRent(currentPlayerLocation, rent))
                            {
                                MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                                bankrupt();
                            }
                            else
                            {
                                GameLog.Text += Game.playername[Game.turn] + " płaci " + rent + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                            }
                        }
                    }
                }
            }
            else if (BoardData.fieldTax[currentPlayerLocation] == true)
            {
                if (Game.turn == Game.clientplayer)
                {
                    MessageBox.Show("Musisz zapłacić podatek w wysokości " + BoardData.fieldTaxCost[currentPlayerLocation] + "$.", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    if (!payTax(currentPlayerLocation))
                    {
                        MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                        Game.dangerzone = true;
                        DangerZone();
                    }
                    else
                    {
                        if (Game.dangerzone == true)
                        {
                            LeaveDangerZone();
                        }
                        GameLog.Text += Game.playername[Game.turn] + " płaci podatek w wysokości " + BoardData.fieldTaxCost[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                        if (Game.multiplayer)
                        {
                            SendGameLog(Game.playername[Game.turn] + " płaci podatek w wysokości " + BoardData.fieldTaxCost[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine);
                        }
                    }
                }
                else
                {
                    if (!Game.multiplayer)
                    {
                        if (payTax(currentPlayerLocation))
                        {
                            GameLog.Text += Game.playername[Game.turn] + " płaci podatek w wysokości " + BoardData.fieldTaxCost[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                        }
                        else
                        {
                            MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                            bankrupt();
                        }
                    }
                }
            }
            else if (BoardData.fieldExtra[currentPlayerLocation] == true)
            {
                switch (currentPlayerLocation)
                {
                    case 10: //Prison
                        GameLog.Text += Game.playername[Game.turn] + " odwiedza więźniów! Jaki miły z niego człowiek!" + Environment.NewLine + Environment.NewLine;
                        if (Game.multiplayer)
                        {
                            SendGameLog(Game.playername[Game.turn] + " odwiedza więźniów! Jaki miły z niego człowiek!" + Environment.NewLine + Environment.NewLine);
                        }
                        break;

                    case 20: //Parking Lot
                        Game.playercash[Game.turn] = Game.playercash[Game.turn] + Game.taxmoney;
                        PlayerStatusRefresh();
                        GameLog.Text += Game.playername[Game.turn] + " zdobywa " + Game.taxmoney + "$!" + Environment.NewLine + Environment.NewLine;
                        if (Game.multiplayer)
                        {
                            SendGameLog(Game.playername[Game.turn] + " zdobywa " + Game.taxmoney + "$!" + Environment.NewLine + Environment.NewLine);
                        }
                        if (Game.turn == Game.clientplayer)
                        {
                            MessageBox.Show("Zdobywasz " + Game.taxmoney + "$!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        Game.taxmoney = 0;
                        break;

                    case 30: //Go To Jail
                        if (Game.turn == Game.clientplayer)
                        {
                            MessageBox.Show("Idziesz do więzienia!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        Game.playerlocation[Game.turn] = 40;
                        Game.playerArrestedTurns[Game.turn] = 2;
                        Jump();
                        GameLog.Text += Game.playername[Game.turn] + " zostaje aresztowany!" + Environment.NewLine + Environment.NewLine;
                        if (Game.multiplayer)
                        {
                            SendGameLog(Game.playername[Game.turn] + " zostaje aresztowany!" + Environment.NewLine + Environment.NewLine);
                        }
                        break;

                    case 12: //Electric Company
                        if (Game.fieldOwner[currentPlayerLocation] == 4)
                        {
                            if (Game.turn == Game.clientplayer)
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
                                            GameLog.Text += Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                                            if (Game.multiplayer)
                                            {
                                                SendGameLog(Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine);
                                            }
                                        }
                                        break;

                                    case MessageBoxResult.No:
                                        break;
                                }
                            }
                            else
                            {
                                if (!Game.multiplayer)
                                {
                                    if (buyField(currentPlayerLocation))
                                    {
                                        GameLog.Text += Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                                    }
                                }
                            }
                        }
                        else if (Game.fieldOwner[currentPlayerLocation] != Game.turn)
                        {
                            int calculatedMoney = calculateExtraFieldMultiplier(currentPlayerLocation);
                            if (Game.turn == Game.clientplayer)
                            {
                                MessageBox.Show("Stanąłeś na elektrowni gracza " + Game.fieldOwner[currentPlayerLocation] + ". Musisz mu zapłacić: " + calculatedMoney, "Monopoly", MessageBoxButton.OK, MessageBoxImage.Warning);
                                if (!payExtraFieldMultiplier(calculatedMoney, currentPlayerLocation))
                                {
                                    MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                                    Game.dangerzone = true;
                                    DangerZone();
                                }
                                else
                                {
                                    if (Game.dangerzone == true)
                                    {
                                        LeaveDangerZone();
                                    }
                                    GameLog.Text += Game.playername[Game.turn] + " płaci " + calculatedMoney + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                                    if (Game.multiplayer)
                                    {
                                        SendGameLog(Game.playername[Game.turn] + " płaci " + calculatedMoney + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine);
                                    }
                                }
                            }
                            else
                            {
                                if (!Game.multiplayer)
                                {
                                    if (!payExtraFieldMultiplier(calculatedMoney, currentPlayerLocation))
                                    {
                                        MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                                        bankrupt();
                                    }
                                    else
                                    {
                                        GameLog.Text += Game.playername[Game.turn] + " płaci " + calculatedMoney + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                                    }
                                }
                            }
                        }
                        break;

                    case 28: //Waterworks
                        if (Game.fieldOwner[currentPlayerLocation] == 4)
                        {
                            if (Game.turn == Game.clientplayer)
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
                                            GameLog.Text += Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                                            if (Game.multiplayer)
                                            {
                                                SendGameLog(Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine);
                                            }
                                        }
                                        break;

                                    case MessageBoxResult.No:
                                        break;
                                }
                            }
                            else
                            {
                                if (!Game.multiplayer)
                                {
                                    if (buyField(currentPlayerLocation))
                                    {
                                        GameLog.Text += Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                                    }
                                }
                            }
                        }
                        else if (Game.fieldOwner[currentPlayerLocation] != Game.turn)
                        {
                            int calculatedMoney = calculateExtraFieldMultiplier(currentPlayerLocation);
                            if (Game.turn == Game.clientplayer)
                            {
                                MessageBox.Show("Stanąłeś na wodociągach gracza " + Game.fieldOwner[currentPlayerLocation] + ". Musisz mu zapłacić: " + calculatedMoney, "Monopoly", MessageBoxButton.OK, MessageBoxImage.Warning);
                                if (!payExtraFieldMultiplier(calculatedMoney, currentPlayerLocation))
                                {
                                    MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                                    Game.dangerzone = true;
                                    DangerZone();
                                }
                                else
                                {
                                    if (Game.dangerzone == true)
                                    {
                                        LeaveDangerZone();
                                    }
                                    GameLog.Text += Game.playername[Game.turn] + " płaci " + calculatedMoney + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                                    if (Game.multiplayer)
                                    {
                                        SendGameLog(Game.playername[Game.turn] + " płaci " + calculatedMoney + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine);
                                    }
                                }
                            }
                            else
                            {
                                if (!Game.multiplayer)
                                {
                                    if (!payExtraFieldMultiplier(calculatedMoney, currentPlayerLocation))
                                    {
                                        MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                                        bankrupt();
                                    }
                                    else
                                    {
                                        GameLog.Text += Game.playername[Game.turn] + " płaci " + calculatedMoney + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                                    }
                                }
                            }
                        }
                        break;
                }
            }
            else // For normal estates
            {
                if (Game.fieldOwner[currentPlayerLocation] == 4)
                {
                    if (Game.turn == Game.clientplayer)
                    {
                        MessageBoxResult result = MessageBox.Show("Czy chcesz kupić ulicę " + BoardData.fieldName[currentPlayerLocation] + "?", "Monopoly", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        switch (result)
                        {
                            case MessageBoxResult.Yes:
                                if (!buyField(currentPlayerLocation))
                                {
                                    MessageBox.Show("Nie stać Cię na tą ulicę!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                else
                                {
                                    GameLog.Text += Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                                    if (Game.multiplayer)
                                    {
                                        SendGameLog(Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine);
                                    }
                                    if (Game.fieldOwner[currentPlayerLocation] != 4 && Game.fieldOwner[currentPlayerLocation] == Game.fieldOwner[BoardData.fieldSet1[currentPlayerLocation]] && Game.fieldOwner[currentPlayerLocation] == Game.fieldOwner[BoardData.fieldSet2[currentPlayerLocation]] || BoardData.fieldSet2[currentPlayerLocation] == 0)
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
                        if (!Game.multiplayer)
                        {
                            if (buyField(currentPlayerLocation))
                            {
                                GameLog.Text += Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                            }
                        }
                    }
                }
                else if (Game.fieldOwner[currentPlayerLocation] != Game.turn)
                {
                    bool hasSet = false;
                    if (BoardData.fieldSet2[currentPlayerLocation] == 0)
                    {
                        if (Game.fieldOwner[currentPlayerLocation] == Game.turn && Game.fieldOwner[currentPlayerLocation] != 4 && Game.fieldOwner[currentPlayerLocation] == Game.fieldOwner[BoardData.fieldSet1[currentPlayerLocation]])
                        {
                            hasSet = true;
                        }
                    }
                    else
                    {
                        if (Game.fieldOwner[currentPlayerLocation] == Game.turn && Game.fieldOwner[currentPlayerLocation] != 4 && Game.fieldOwner[currentPlayerLocation] == Game.fieldOwner[BoardData.fieldSet1[currentPlayerLocation]] && Game.fieldOwner[currentPlayerLocation] == Game.fieldOwner[BoardData.fieldSet2[currentPlayerLocation]])
                        {
                            hasSet = true;
                        }
                    }
                    if (hasSet)
                    {
                        rent = BoardData.fieldNoSetRent[currentPlayerLocation] * 2;
                    }
                    if (Game.fieldHouse[currentPlayerLocation] == 1)
                    {
                        rent = BoardData.field1Rent[currentPlayerLocation];
                    }
                    else if (Game.fieldHouse[currentPlayerLocation] == 2)
                    {
                        rent = BoardData.field2Rent[currentPlayerLocation];
                    }
                    else if (Game.fieldHouse[currentPlayerLocation] == 3)
                    {
                        rent = BoardData.field3Rent[currentPlayerLocation];
                    }
                    else if (Game.fieldHouse[currentPlayerLocation] == 4)
                    {
                        rent = BoardData.field4Rent[currentPlayerLocation];
                    }
                    else if (Game.fieldHouse[currentPlayerLocation] >= 5)
                    {
                        rent = BoardData.fieldHRent[currentPlayerLocation];
                    }
                    if (Game.turn == Game.clientplayer)
                    {
                        MessageBox.Show("Stanąłeś na dzielnicy gracz " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + ". Musisz mu zapłacić: " + rent, "Monopoly", MessageBoxButton.OK, MessageBoxImage.Warning);
                        if (!payRent(currentPlayerLocation, rent))
                        {
                            MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                            Game.dangerzone = true;
                            DangerZone();
                        }
                        else
                        {
                            if (Game.dangerzone == true)
                            {
                                LeaveDangerZone();
                            }
                            GameLog.Text += Game.playername[Game.turn] + " płaci " + rent + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                            if (Game.multiplayer)
                            {
                                SendGameLog(Game.playername[Game.turn] + " płaci " + rent + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine);
                            }
                        }
                    }
                    else
                    {
                        if (!Game.multiplayer)
                        {
                            if (!payRent(currentPlayerLocation, rent))
                            {
                                MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                                bankrupt();
                            }
                            else
                            {
                                GameLog.Text += Game.playername[Game.turn] + " płaci " + rent + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                            }
                        }
                    }
                }
            }
            PlayerStatusRefresh();
            if (Game.multiplayer)
                SendData();
        }
        private bool buyField(byte currentPlayerLocation)
        {
            if (Game.playercash[Game.turn] >= BoardData.fieldPrice[currentPlayerLocation])
            {
                Game.playercash[Game.turn] = Game.playercash[Game.turn] - BoardData.fieldPrice[currentPlayerLocation];
                Game.fieldOwner[currentPlayerLocation] = Game.turn;
                DrawOwner(currentPlayerLocation, Game.turn);
                audio.playSFX("money");
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool payRent(byte currentPlayerLocation, int rent)
        {
            if (Game.playercash[Game.turn] >= rent)
            {
                Game.playercash[Game.turn] = Game.playercash[Game.turn] - rent;
                Game.playercash[Game.fieldOwner[currentPlayerLocation]] = Game.playercash[Game.fieldOwner[currentPlayerLocation]] + rent;
                audio.playSFX("money");
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool payTax(byte currentPlayerLocation)
        {
            if (Game.playercash[Game.turn] >= BoardData.fieldTaxCost[currentPlayerLocation])
            {
                Game.playercash[Game.turn] = Game.playercash[Game.turn] - BoardData.fieldTaxCost[currentPlayerLocation];
                Game.taxmoney = Game.taxmoney + BoardData.fieldTaxCost[currentPlayerLocation];
                audio.playSFX("money");
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool doChanceCard(byte chanceCard)
        {
            if (BoardData.chanceAction[chanceCard] == 0)
            {
                // No function here currently
                return true;
            }
            if (BoardData.chanceAction[chanceCard] == 1)
            {
                if (Game.playercash[Game.turn] >= BoardData.chanceXValue[chanceCard])
                {
                    Game.playercash[Game.turn] = Game.playercash[Game.turn] - BoardData.chanceXValue[chanceCard];
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
                throw new System.Exception("Błąd związany z kartą szansy! Dalsza gra może zawierać błędy! Zrestartuj aplikację i zgłoś błąd twórcy!");
            }
        }

        private bool doCommChestCard(byte commChestCard)
        {
            if (BoardData.commChestAction[commChestCard] == 0)
            {
                Game.playercash[Game.turn] = Game.playercash[Game.turn] + BoardData.commChestXValue[commChestCard];
                return true;
            }
            if (BoardData.commChestAction[commChestCard] == 1)
            {
                if (Game.playercash[Game.turn] >= BoardData.commChestXValue[commChestCard])
                {
                    Game.playercash[Game.turn] = Game.playercash[Game.turn] - BoardData.commChestXValue[commChestCard];
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
            if (Game.fieldOwner[BoardData.fieldSet1[currentPlayerLocation]] != Game.turn)
            {
                return Game.dice1 + Game.dice2 * 4;
            }
            else
            {
                return Game.dice1 + Game.dice2 * 10;
            }
        }

        private bool payExtraFieldMultiplier(int calculatedMoney, byte currentPlayerLocation)
        {
            if (Game.playercash[Game.turn] >= calculatedMoney)
            {
                Game.playercash[Game.turn] = Game.playercash[Game.turn] - calculatedMoney;
                Game.playercash[Game.fieldOwner[currentPlayerLocation]] = Game.playercash[Game.fieldOwner[currentPlayerLocation]] + calculatedMoney;
                audio.playSFX("money");
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
            if (BoardData.fieldSet2[selectedField] == 0)
            {
                if (Game.fieldOwner[selectedField] == Game.turn && Game.fieldOwner[selectedField] != 4 && Game.fieldOwner[selectedField] == Game.fieldOwner[BoardData.fieldSet1[selectedField]])
                {
                    hasSet = true;
                }
            }
            else
            {
                if (Game.fieldOwner[selectedField] == Game.turn && Game.fieldOwner[selectedField] != 4 && Game.fieldOwner[selectedField] == Game.fieldOwner[BoardData.fieldSet1[selectedField]] && Game.fieldOwner[selectedField] == Game.fieldOwner[BoardData.fieldSet2[selectedField]])
                {
                    hasSet = true;
                }
            }
            if (hasSet == true)
            {
                if (selectedField > 0 && selectedField < 9)
                {
                    if (Game.fieldHouse[selectedField] < 5)
                    {
                        if (Game.playercash[Game.turn] >= 50)
                        {
                            Game.playercash[Game.turn] = Game.playercash[Game.turn] - 50;
                            buyHouse2(selectedField);
                            return true;
                        }
                        return false;
                    }
                    return false;
                }
                else if (selectedField > 10 && selectedField < 20)
                {
                    if (Game.fieldHouse[selectedField] < 5)
                    {
                        if (Game.playercash[Game.turn] >= 100)
                        {
                            Game.playercash[Game.turn] = Game.playercash[Game.turn] - 100;
                            buyHouse2(selectedField);
                            return true;
                        }
                        return false;
                    }
                    return false;
                }
                else if (selectedField > 20 && selectedField < 30)
                {
                    if (Game.fieldHouse[selectedField] < 5)
                    {
                        if (Game.playercash[Game.turn] >= 150)
                        {
                            Game.playercash[Game.turn] = Game.playercash[Game.turn] - 150;
                            buyHouse2(selectedField);
                            return true;
                        }
                        return false;
                    }
                    return false;
                }
                else if (selectedField > 30 && selectedField < 40)
                {
                    if (Game.fieldHouse[selectedField] < 5)
                    {
                        if (Game.playercash[Game.turn] >= 200)
                        {
                            Game.playercash[Game.turn] = Game.playercash[Game.turn] - 200;
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
            Game.fieldHouse[selectedField]++;
            audio.playSFX("build");
            GameLog.Text += Game.playername[Game.clientplayer] + " kupuje budynek w dzielnicy " + BoardData.fieldName[Game.selectedField] + Environment.NewLine + Environment.NewLine;
            if (Game.multiplayer)
            {
                SendGameLog(Game.playername[Game.clientplayer] + " kupuje budynek w dzielnicy " + BoardData.fieldName[Game.selectedField] + Environment.NewLine + Environment.NewLine);
            }
            switch (Game.fieldHouse[selectedField])
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
            if (BoardData.fieldSet2[selectedField] == 0)
            {
                if (Game.fieldOwner[selectedField] == Game.turn && Game.fieldOwner[selectedField] != 4 && Game.fieldOwner[selectedField] == Game.fieldOwner[BoardData.fieldSet1[selectedField]])
                {
                    hasSet = true;
                }
            }
            else
            {
                if (Game.fieldOwner[selectedField] == Game.turn && Game.fieldOwner[selectedField] != 4 && Game.fieldOwner[selectedField] == Game.fieldOwner[BoardData.fieldSet1[selectedField]] && Game.fieldOwner[selectedField] == Game.fieldOwner[BoardData.fieldSet2[selectedField]])
                {
                    hasSet = true;
                }
            }
            if (hasSet == true)
            {
                if (selectedField > 0 && selectedField < 9)
                {
                    if (Game.fieldHouse[selectedField] > 0)
                    {
                        Game.playercash[Game.turn] = Game.playercash[Game.turn] + 25;
                        sellHouse2(selectedField);
                        return true;
                    }
                    return false;
                }
                else if (selectedField > 10 && selectedField < 20)
                {
                    if (Game.fieldHouse[selectedField] > 0)
                    {
                        Game.playercash[Game.turn] = Game.playercash[Game.turn] + 50;
                        sellHouse2(selectedField);
                        return true;
                    }
                    return false;
                }
                else if (selectedField > 20 && selectedField < 30)
                {
                    if (Game.fieldHouse[selectedField] > 0)
                    {
                        Game.playercash[Game.turn] = Game.playercash[Game.turn] + 75;
                        sellHouse2(selectedField);
                        return true;
                    }
                    return false;
                }
                else if (selectedField > 30 && selectedField < 40)
                {
                    if (Game.fieldHouse[selectedField] > 0)
                    {
                        Game.playercash[Game.turn] = Game.playercash[Game.turn] + 100;
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
            Game.fieldHouse[selectedField]--;
            audio.playSFX("destroy");
            GameLog.Text += Game.playername[Game.clientplayer] + " sprzedaje budynek w dzielnicy " + BoardData.fieldName[Game.selectedField] + Environment.NewLine + Environment.NewLine;
            if (Game.multiplayer)
            {
                SendGameLog(Game.playername[Game.clientplayer] + " sprzedaje budynek w dzielnicy " + BoardData.fieldName[Game.selectedField] + Environment.NewLine + Environment.NewLine);
            }
            switch (Game.fieldHouse[selectedField])
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
            if (Game.fieldHouse[selectedField] == 0 && Game.fieldOwner[selectedField] == Game.turn)
            {
                Game.playercash[Game.fieldOwner[selectedField]] = Game.playercash[Game.fieldOwner[selectedField]] + (BoardData.fieldPrice[selectedField] / 2);
                Game.fieldOwner[selectedField] = 4;
                OverviewRefresh();
                PlayerStatusRefresh();
                DrawOwner(selectedField, Game.turn);
                return true;
            }
            return false;
        }

        // UI Programming
        // //////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma warning disable CS0108 // Składowa ukrywa dziedziczoną składową; brak słowa kluczowego new
        private void SizeChangedEvent(object sender, SizeChangedEventArgs e)
#pragma warning restore CS0108 // Składowa ukrywa dziedziczoną składową; brak słowa kluczowego new
        {
            CanvasRenderScale();
        }
        private void StateChangedEvent(object sender, EventArgs e)
        {
            CanvasRenderScale();
        }
        public void CanvasRenderScale()
        {
            double ScaleW = this.ActualWidth / 1100;
            double ScaleH = this.ActualHeight / 700;
            if (ScaleW < ScaleH)
            {
                GameCanvas.RenderTransform = new ScaleTransform(ScaleW, ScaleW);
            }
            else
            {
                GameCanvas.RenderTransform = new ScaleTransform(ScaleH, ScaleH);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Game.dangerzone)
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
            if (!Game.dangerzone)
            {
                Game.turn++;
                EndTurn();
            }
            else
            {
                FieldCheck();
            }
        }
        private void DiceShow(byte dice1, byte dice2)
        {
            switch (dice1)
            {
                case 1:
                    Dice1.Source = new BitmapImage(new Uri(@"Resources/dice_1.png", UriKind.Relative));
                    break;
                case 2:
                    Dice1.Source = new BitmapImage(new Uri(@"Resources/dice_2.png", UriKind.Relative));
                    break;
                case 3:
                    Dice1.Source = new BitmapImage(new Uri(@"Resources/dice_3.png", UriKind.Relative));
                    break;
                case 4:
                    Dice1.Source = new BitmapImage(new Uri(@"Resources/dice_4.png", UriKind.Relative));
                    break;
                case 5:
                    Dice1.Source = new BitmapImage(new Uri(@"Resources/dice_5.png", UriKind.Relative));
                    break;
                case 6:
                    Dice1.Source = new BitmapImage(new Uri(@"Resources/dice_6.png", UriKind.Relative));
                    break;
            }
            switch (dice2)
            {
                case 1:
                    Dice2.Source = new BitmapImage(new Uri(@"Resources/dice_1.png", UriKind.Relative));
                    break;
                case 2:
                    Dice2.Source = new BitmapImage(new Uri(@"Resources/dice_2.png", UriKind.Relative));
                    break;
                case 3:
                    Dice2.Source = new BitmapImage(new Uri(@"Resources/dice_3.png", UriKind.Relative));
                    break;
                case 4:
                    Dice2.Source = new BitmapImage(new Uri(@"Resources/dice_4.png", UriKind.Relative));
                    break;
                case 5:
                    Dice2.Source = new BitmapImage(new Uri(@"Resources/dice_5.png", UriKind.Relative));
                    break;
                case 6:
                    Dice2.Source = new BitmapImage(new Uri(@"Resources/dice_6.png", UriKind.Relative));
                    break;
            }
        }
        private void RefreshOwnersUI()
        {
            for(byte i = 1; i < 41; i++)
            {
                DrawOwner(i, Game.fieldOwner[i]);
            }
        }
        private void OverviewRefresh()
        {
            Overview_Picture.Source = BoardData.fieldIcon[Game.selectedField];
            Overview_Name.Content = BoardData.fieldName[Game.selectedField];
            Overview_Price.Content = BoardData.fieldPrice[Game.selectedField] + " $";
            Overview_Houses.Content = Game.fieldHouse[Game.selectedField];
            Overview_Owner.Content = Game.playername[Game.fieldOwner[Game.selectedField]];
            Overview_NoSetRent.Content = BoardData.fieldNoSetRent[Game.selectedField] + " $";
            Overview_1Rent.Content = BoardData.field1Rent[Game.selectedField] + " $";
            Overview_2Rent.Content = BoardData.field2Rent[Game.selectedField] + " $";
            Overview_3Rent.Content = BoardData.field3Rent[Game.selectedField] + " $";
            Overview_4Rent.Content = BoardData.field4Rent[Game.selectedField] + " $";
            Overview_HRent.Content = BoardData.fieldHRent[Game.selectedField] + " $";
        }
        private void PlayerStatusRefresh()
        {
            Label_Player1Cash.Content = Game.playercash[0] + "$";
            Label_Player2Cash.Content = Game.playercash[1] + "$";
            Label_Player3Cash.Content = Game.playercash[2] + "$";
            Label_Player4Cash.Content = Game.playercash[3] + "$";
        }
        private void Jump()
        {
            int xcord = 0;
            int ycord = 0;
            if (Game.fieldPlayers[Game.playerlocation[Game.turn]] <= 1)
            {
                xcord = boardLocations.playerlocation(true, Game.playerlocation[Game.turn]);
                ycord = boardLocations.playerlocation(false, Game.playerlocation[Game.turn]);
            }
            else if (Game.fieldPlayers[Game.playerlocation[Game.turn]] == 2)
            {
                xcord = boardLocations.playerlocation(true, Game.playerlocation[Game.turn]) + 22;
                ycord = boardLocations.playerlocation(false, Game.playerlocation[Game.turn]);
            }
            else if (Game.fieldPlayers[Game.playerlocation[Game.turn]] == 3)
            {
                xcord = boardLocations.playerlocation(true, Game.playerlocation[Game.turn]);
                ycord = boardLocations.playerlocation(false, Game.playerlocation[Game.turn]) + 22;
            }
            else if (Game.fieldPlayers[Game.playerlocation[Game.turn]] >= 4)
            {
                xcord = boardLocations.playerlocation(true, Game.playerlocation[Game.turn]) + 22;
                ycord = boardLocations.playerlocation(false, Game.playerlocation[Game.turn]) + 22;
            }
            switch (Game.turn)
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
                    Field2Owner.Source = OwnerStatusCheck(status);
                    break;

                case 3:
                    Field4Owner.Source = OwnerStatusCheck(status);
                    break;

                case 5:
                    Field6Owner.Source = OwnerStatusCheck(status);
                    break;

                case 6:
                    Field7Owner.Source = OwnerStatusCheck(status);
                    break;

                case 8:
                    Field9Owner.Source = OwnerStatusCheck(status);
                    break;

                case 9:
                    Field10Owner.Source = OwnerStatusCheck(status);
                    break;

                case 11:
                    Field12Owner.Source = OwnerStatusCheck(status);
                    break;

                case 12:
                    Field13Owner.Source = OwnerStatusCheck(status);
                    break;

                case 13:
                    Field14Owner.Source = OwnerStatusCheck(status);
                    break;

                case 14:
                    Field15Owner.Source = OwnerStatusCheck(status);
                    break;

                case 15:
                    Field16Owner.Source = OwnerStatusCheck(status);
                    break;

                case 16:
                    Field17Owner.Source = OwnerStatusCheck(status);
                    break;

                case 18:
                    Field19Owner.Source = OwnerStatusCheck(status);
                    break;

                case 19:
                    Field20Owner.Source = OwnerStatusCheck(status);
                    break;

                case 21:
                    Field22Owner.Source = OwnerStatusCheck(status);
                    break;

                case 23:
                    Field24Owner.Source = OwnerStatusCheck(status);
                    break;

                case 24:
                    Field25Owner.Source = OwnerStatusCheck(status);
                    break;

                case 25:
                    Field26Owner.Source = OwnerStatusCheck(status);
                    break;

                case 26:
                    Field27Owner.Source = OwnerStatusCheck(status);
                    break;

                case 27:
                    Field28Owner.Source = OwnerStatusCheck(status);
                    break;

                case 28:
                    Field29Owner.Source = OwnerStatusCheck(status);
                    break;

                case 29:
                    Field30Owner.Source = OwnerStatusCheck(status);
                    break;

                case 31:
                    Field32Owner.Source = OwnerStatusCheck(status);
                    break;

                case 32:
                    Field33Owner.Source = OwnerStatusCheck(status);
                    break;

                case 34:
                    Field35Owner.Source = OwnerStatusCheck(status);
                    break;

                case 35:
                    Field36Owner.Source = OwnerStatusCheck(status);
                    break;

                case 37:
                    Field38Owner.Source = OwnerStatusCheck(status);
                    break;

                case 39:
                    Field40Owner.Source = OwnerStatusCheck(status);
                    break;
            }
        }

        private BitmapImage OwnerStatusCheck(byte status)
        {
            switch (status)
            {
                case 0:
                    return new BitmapImage(new Uri(currentThemeDir + @"\BlueField.png", UriKind.Relative));

                case 1:
                    return new BitmapImage(new Uri(currentThemeDir + @"\GreenField.png", UriKind.Relative));

                case 2:
                    return new BitmapImage(new Uri(currentThemeDir + @"\YellowField.png", UriKind.Relative));

                case 3:
                    return new BitmapImage(new Uri(currentThemeDir + @"\RedField.png", UriKind.Relative));

                case 4:
                    return new BitmapImage(new Uri(currentThemeDir + @"\NoAlpha.png", UriKind.Relative));
            }
            return null;
        }

        private void DrawHouses(byte field, byte status)
        {
            switch (field)
            {
                case 1:
                    Field2.Source = HousesStatusCheck(status);
                    break;

                case 3:
                    Field4.Source = HousesStatusCheck(status);
                    break;

                case 6:
                    Field7.Source = HousesStatusCheck(status);
                    break;

                case 8:
                    Field9.Source = HousesStatusCheck(status);
                    break;

                case 9:
                    Field10.Source = HousesStatusCheck(status);
                    break;

                case 11:
                    Field12.Source = HousesStatusCheck(status);
                    break;

                case 13:
                    Field14.Source = HousesStatusCheck(status);
                    break;

                case 14:
                    Field15.Source = HousesStatusCheck(status);
                    break;

                case 16:
                    Field17.Source = HousesStatusCheck(status);
                    break;

                case 18:
                    Field19.Source = HousesStatusCheck(status);
                    break;

                case 19:
                    Field20.Source = HousesStatusCheck(status);
                    break;

                case 21:
                    Field22.Source = HousesStatusCheck(status);
                    break;

                case 23:
                    Field24.Source = HousesStatusCheck(status);
                    break;

                case 24:
                    Field25.Source = HousesStatusCheck(status);
                    break;

                case 26:
                    Field27.Source = HousesStatusCheck(status);
                    break;

                case 27:
                    Field28.Source = HousesStatusCheck(status);
                    break;

                case 29:
                    Field30.Source = HousesStatusCheck(status);
                    break;

                case 31:
                    Field32.Source = HousesStatusCheck(status);
                    break;

                case 32:
                    Field33.Source = HousesStatusCheck(status);
                    break;

                case 34:
                    Field35.Source = HousesStatusCheck(status);
                    break;

                case 37:
                    Field38.Source = HousesStatusCheck(status);
                    break;

                case 39:
                    Field40.Source = HousesStatusCheck(status);
                    break;
            }
        }

        private BitmapImage HousesStatusCheck(byte status)
        {
            switch (status)
            {
                case 0:
                    return new BitmapImage(new Uri(currentThemeDir + @"\NoAlpha.png", UriKind.Relative));
                case 1:
                    return new BitmapImage(new Uri(currentThemeDir + @"\House1.png", UriKind.Relative));
                case 2:
                    return new BitmapImage(new Uri(currentThemeDir + @"\House2.png", UriKind.Relative));
                case 3:
                    return new BitmapImage(new Uri(currentThemeDir + @"\House3.png", UriKind.Relative));
                case 4:
                    return new BitmapImage(new Uri(currentThemeDir + @"\House4.png", UriKind.Relative));
                case 5:
                    return new BitmapImage(new Uri(currentThemeDir + @"\Trivago.png", UriKind.Relative));
            }
            return null;
        }
        private void Field1_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 0;
            OverviewRefresh();
        }

        private void Field2_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 1;
            OverviewRefresh();
        }

        private void Field3_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 2;
            OverviewRefresh();
        }

        private void Field4_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 3;
            OverviewRefresh();
        }

        private void Field5_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 4;
            OverviewRefresh();
        }

        private void Field6_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 5;
            OverviewRefresh();
        }

        private void Field7_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 6;
            OverviewRefresh();
        }

        private void Field8_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 7;
            OverviewRefresh();
        }

        private void Field9_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 8;
            OverviewRefresh();
        }

        private void Field10_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 9;
            OverviewRefresh();
        }

        private void Field11_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 10;
            OverviewRefresh();
        }

        private void Field12_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 11;
            OverviewRefresh();
        }

        private void Field13_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 12;
            OverviewRefresh();
        }

        private void Field14_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 13;
            OverviewRefresh();
        }

        private void Field15_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 14;
            OverviewRefresh();
        }

        private void Field16_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 15;
            OverviewRefresh();
        }

        private void Field17_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 16;
            OverviewRefresh();
        }

        private void Field18_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 17;
            OverviewRefresh();
        }

        private void Field19_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 18;
            OverviewRefresh();
        }

        private void Field20_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 19;
            OverviewRefresh();
        }

        private void Field21_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 20;
            OverviewRefresh();
        }

        private void Field22_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 21;
            OverviewRefresh();
        }

        private void Field23_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 22;
            OverviewRefresh();
        }

        private void Field24_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 23;
            OverviewRefresh();
        }

        private void Field25_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 24;
            OverviewRefresh();
        }

        private void Field26_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 25;
            OverviewRefresh();
        }

        private void Field27_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 26;
            OverviewRefresh();
        }

        private void Field28_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 27;
            OverviewRefresh();
        }

        private void Field29_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 28;
            OverviewRefresh();
        }

        private void Field30_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 29;
            OverviewRefresh();
        }

        private void Field31_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 30;
            OverviewRefresh();
        }

        private void Field32_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 31;
            OverviewRefresh();
        }

        private void Field33_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 32;
            OverviewRefresh();
        }

        private void Field34_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 33;
            OverviewRefresh();
        }

        private void Field35_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 34;
            OverviewRefresh();
        }

        private void Field36_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 35;
            OverviewRefresh();
        }

        private void Field37_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 36;
            OverviewRefresh();
        }

        private void Field38_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 37;
            OverviewRefresh();
        }

        private void Field39_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 38;
            OverviewRefresh();
        }

        private void Field40_MouseEnter(object sender, MouseEventArgs e)
        {
            Game.selectedField = 39;
            OverviewRefresh();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Pause)
            {
                CheatWindow cheatWindow = new CheatWindow();
                cheatWindow.ShowDialog();
                if (cheat_allowTradeWindow)
                {
                    Button_Trade.IsEnabled = true;
                }
                if (cheat != 0)
                {
                    Button_ThrowDice.IsEnabled = false;
                    diceScore = Convert.ToByte(cheat);
                    DiceScore.Content = diceScore;
                    wait.Start();
                }
            }
        }

        private void Field1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CantBuyHouseNorSellThisField();
        }

        private void Field2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(1);
        }

        private void Field3_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CantBuyHouseNorSellThisField();
        }

        private void Field4_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(3);
        }

        private void Field5_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CantBuyHouseNorSellThisField();
        }

        private void Field6_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CantBuyHouseOrSellField(5);
        }

        private void Field7_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(6);
        }

        private void Field8_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CantBuyHouseNorSellThisField();
        }

        private void Field9_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(8);
        }

        private void Field10_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(9);
        }

        private void Field11_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CantBuyHouseNorSellThisField();
        }

        private void Field12_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(11);
        }

        private void Field13_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CantBuyHouseOrSellField(12);
        }

        private void Field14_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(13);
        }

        private void Field15_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(14);
        }

        private void Field16_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CantBuyHouseOrSellField(15);
        }

        private void Field17_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(16);
        }

        private void Field18_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CantBuyHouseNorSellThisField();
        }

        private void Field19_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(18);
        }

        private void Field20_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(19);
        }
        private void Field21_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CantBuyHouseNorSellThisField();
        }

        private void Field22_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(21);
        }

        private void Field23_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CantBuyHouseNorSellThisField();
        }

        private void Field24_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(23);
        }

        private void Field25_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(24);
        }

        private void Field26_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CantBuyHouseOrSellField(25);
        }

        private void Field27_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(26);
        }

        private void Field28_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(27);
        }

        private void Field29_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CantBuyHouseOrSellField(28);
        }

        private void Field30_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(29);
        }

        private void Field31_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CantBuyHouseNorSellThisField();
        }

        private void Field32_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(31);
        }

        private void Field33_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(32);
        }

        private void Field34_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CantBuyHouseNorSellThisField();
        }

        private void Field35_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(34);
        }

        private void Field36_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CantBuyHouseOrSellField(35);
        }

        private void Field37_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CantBuyHouseNorSellThisField();
        }

        private void Field38_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(37);
        }

        private void Field39_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CantBuyHouseNorSellThisField();
        }

        private void Field40_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuyHouseOrSellField(39);
        }

        private void CantBuyHouseNorSellThisField()
        {
            if (Game.turn == Game.clientplayer && !Game.sellmode)
            {
                MessageBox.Show("Na tym polu nie możesz kupować domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (Game.turn == Game.clientplayer && Game.sellmode)
            {
                MessageBox.Show("Nie możesz sprzedać tego pola!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BuyHouseOrSellField(byte field)
        {
            if (Game.turn == Game.clientplayer && !Game.sellmode)
            {
                if (!buyHouse(field))
                {
                    MessageBox.Show("Nie można kupić budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (Game.turn == Game.clientplayer && Game.sellmode)
            {
                if (!sellField(field))
                {
                    MessageBox.Show("Nie można sprzedać ulicy!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void CantBuyHouseOrSellField(byte field)
        {
            if (Game.turn == Game.clientplayer && !Game.sellmode)
            {
                MessageBox.Show("Na tym polu nie możesz kupować domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (Game.turn == Game.clientplayer && Game.sellmode)
            {
                if (!sellField(field))
                {
                    MessageBox.Show("Nie można sprzedać tego pola!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuItem_StartNewSingle(object sender, RoutedEventArgs e)
        {
            NewSingleplayerGame newSingleplayerGame = new NewSingleplayerGame(ThemeBoards);
            newSingleplayerGame.ShowDialog();
            Game.playername[0] = newSingleplayerGame.TextBox_Player1.Text;
            Game.playername[1] = newSingleplayerGame.TextBox_Player2.Text;
            Game.playername[2] = newSingleplayerGame.TextBox_Player3.Text;
            Game.playername[3] = newSingleplayerGame.TextBox_Player4.Text;
            Game.playerAvailable[0] = true;
            Game.playerAvailable[1] = true;
            Game.playerAvailable[2] = Convert.ToBoolean(newSingleplayerGame.CheckBox_AIActive1.IsChecked);
            Game.playerAvailable[3] = Convert.ToBoolean(newSingleplayerGame.CheckBox_AIActive2.IsChecked);
            Game.multiplayer = false;
            playboardTheme = (string)newSingleplayerGame.ListBox_PlayboardTheme.SelectedItem;
            LoadTheme();
            StartNewGame();
        }

        private void LoadTheme()
        {
            LoadCurrentThemeDir();
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri(currentThemeDir + @"\monopolyboard.jpg", UriKind.Relative));
            GameCanvas.Background = imageBrush;
            Player1.Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/" + MainWindow.currentThemeDir + @"/BluePlayer.png"));
            Player2.Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/" + MainWindow.currentThemeDir + @"/GreenPlayer.png"));
            Player3.Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/" + MainWindow.currentThemeDir + @"/YellowPlayer.png"));
            Player4.Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/" + MainWindow.currentThemeDir + @"/RedPlayer.png"));
        }

        private void ResetUI()
        {
            int xcord;
            int ycord;
            Dice1.Source = new BitmapImage(new Uri(@"Resources/dice_1.png", UriKind.Relative));
            Dice2.Source = new BitmapImage(new Uri(@"Resources/dice_1.png", UriKind.Relative));
            Player1.Visibility = Visibility.Hidden;
            Player1_Icon.Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/" + MainWindow.currentThemeDir + @"/BluePlayer.png"));
            Player1_Icon.Visibility = Visibility.Hidden;
            Label_Player1Cash.Content = "1500 $";
            Label_Player1Cash.Visibility = Visibility.Hidden;
            Label_Player1Name.Visibility = Visibility.Hidden;
            Player2.Visibility = Visibility.Hidden;
            Player1_Icon.Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/" + MainWindow.currentThemeDir + @"/GreenPlayer.png"));
            Player2_Icon.Visibility = Visibility.Hidden;
            Label_Player2Cash.Content = "1500 $";
            Label_Player2Cash.Visibility = Visibility.Hidden;
            Label_Player2Name.Visibility = Visibility.Hidden;
            Player3.Visibility = Visibility.Hidden;
            Player1_Icon.Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/" + MainWindow.currentThemeDir + @"/YellowPlayer.png"));
            Player3_Icon.Visibility = Visibility.Hidden;
            Label_Player3Cash.Content = "1500 $";
            Label_Player3Cash.Visibility = Visibility.Hidden;
            Label_Player3Name.Visibility = Visibility.Hidden;
            Player4.Visibility = Visibility.Hidden;
            Player1_Icon.Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/" + MainWindow.currentThemeDir + @"/RedPlayer.png"));
            Player4_Icon.Visibility = Visibility.Hidden;
            Label_Player4Cash.Content = "1500 $";
            Label_Player4Cash.Visibility = Visibility.Hidden;
            Label_Player4Name.Visibility = Visibility.Hidden;
            xcord = boardLocations.playerlocation(true, 0);
            ycord = boardLocations.playerlocation(false, 0);
            Canvas.SetLeft(Player1, xcord);
            Canvas.SetTop(Player1, ycord);
            Canvas.SetLeft(Player2, xcord);
            Canvas.SetTop(Player2, ycord);
            Canvas.SetLeft(Player3, xcord);
            Canvas.SetTop(Player3, ycord);
            Canvas.SetLeft(Player3, xcord);
            Canvas.SetTop(Player3, ycord);
        }
        private void MenuItem_StopGame_Click(object sender, RoutedEventArgs e)
        {
            if (!Game.multiplayer)
                ResetUI();
        }
        private void Button_MouseMode_Click(object sender, RoutedEventArgs e)
        {
            if (!Game.sellmode)
            {
                Game.sellmode = true;
                Button_MouseMode.Content = "Tryb sprzedawania ulic";
            }
            else
            {
                Game.sellmode = false;
                Button_MouseMode.Content = "Tryb budowania domów";
            }
        }

        private void Field1_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CantSellHouse();
        }
        private void Field2_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(1);
        }
        private void Field3_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CantSellHouse();
        }
        private void Field4_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(3);
        }
        private void Field5_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CantSellHouse();
        }
        private void Field6_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CantSellHouse();
        }
        private void Field7_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(6);
        }
        private void Field8_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CantSellHouse();
        }
        private void Field9_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(8);
        }
        private void Field10_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(9);
        }
        private void Field11_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CantSellHouse();
        }
        private void Field12_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(11);
        }
        private void Field13_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CantSellHouse();
        }
        private void Field14_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(13);
        }
        private void Field15_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(14);
        }
        private void Field16_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CantSellHouse();
        }
        private void Field17_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(16);
        }
        private void Field18_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CantSellHouse();
        }
        private void Field19_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(18);
        }
        private void Field20_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(19);
        }
        private void Field21_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CantSellHouse();
        }
        private void Field22_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(21);
        }
        private void Field23_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CantSellHouse();
        }
        private void Field24_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(23);
        }
        private void Field25_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(24);
        }
        private void Field26_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CantSellHouse();
        }
        private void Field27_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(26);
        }
        private void Field28_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(27);
        }
        private void Field29_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CantSellHouse();
        }
        private void Field30_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(29);
        }
        private void Field31_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CantSellHouse();
        }
        private void Field32_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(31);
        }
        private void Field33_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(32);
        }
        private void Field34_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CantSellHouse();
        }
        private void Field35_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(34);
        }
        private void Field36_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CantSellHouse();
        }
        private void Field37_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CantSellHouse();
        }
        private void Field38_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(37);
        }
        private void Field39_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CantSellHouse();
        }
        private void Field40_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanSellHouse(39);
        }
        private void CantSellHouse()
        {
            MessageBox.Show("Na tym polu nie możesz sprzedawać domów!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void CanSellHouse(byte field)
        {
            if (Game.turn == Game.clientplayer)
            {
                if (!sellHouse(field))
                {
                    MessageBox.Show("Nie można sprzedać budynku", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void MusicVolumeSet(byte status)
        {
            if (status != 0)
                MenuItem_Volume10.IsChecked = false;
            if (status != 1)
                MenuItem_Volume20.IsChecked = false;
            if (status != 2)
                MenuItem_Volume30.IsChecked = false;
            if (status != 3)
                MenuItem_Volume40.IsChecked = false;
            if (status != 4)
                MenuItem_Volume50.IsChecked = false;
            if (status != 5)
                MenuItem_Volume60.IsChecked = false;
            if (status != 6)
                MenuItem_Volume70.IsChecked = false;
            if (status != 7)
                MenuItem_Volume80.IsChecked = false;
            if (status != 8)
                MenuItem_Volume90.IsChecked = false;
            if (status != 9)
                MenuItem_Volume100.IsChecked = false;
            audio.music.Volume = audio.sfx.Volume / 2;
        }
        private void MenuItem_Checked_100(object sender, RoutedEventArgs e)
        {
            audio.sfx.Volume = 1;
            MusicVolumeSet(9);
            MenuItem_Volume100.IsChecked = true;
        }
        private void MenuItem_Checked_90(object sender, RoutedEventArgs e)
        {
            audio.sfx.Volume = 0.9;
            MusicVolumeSet(8);
            MenuItem_Volume90.IsChecked = true;
        }
        private void MenuItem_Checked_80(object sender, RoutedEventArgs e)
        {
            audio.sfx.Volume = 0.8;
            MusicVolumeSet(7);
            MenuItem_Volume80.IsChecked = true;
        }
        private void MenuItem_Checked_70(object sender, RoutedEventArgs e)
        {
            audio.sfx.Volume = 0.7;
            MusicVolumeSet(6);
            MenuItem_Volume70.IsChecked = true;
        }
        private void MenuItem_Checked_60(object sender, RoutedEventArgs e)
        {
            audio.sfx.Volume = 0.6;
            MusicVolumeSet(5);
            MenuItem_Volume60.IsChecked = true;
        }
        private void MenuItem_Checked_50(object sender, RoutedEventArgs e)
        {
            audio.sfx.Volume = 0.5;
            MusicVolumeSet(4);
            MenuItem_Volume50.IsChecked = true;
        }
        private void MenuItem_Checked_40(object sender, RoutedEventArgs e)
        {
            audio.sfx.Volume = 0.4;
            MusicVolumeSet(3);
            MenuItem_Volume40.IsChecked = true;
        }
        private void MenuItem_Checked_30(object sender, RoutedEventArgs e)
        {
            audio.sfx.Volume = 0.3;
            MusicVolumeSet(2);
            MenuItem_Volume30.IsChecked = true;
        }
        private void MenuItem_Checked_20(object sender, RoutedEventArgs e)
        {
            audio.sfx.Volume = 0.2;
            MusicVolumeSet(1);
            MenuItem_Volume20.IsChecked = true;
        }
        private void MenuItem_Checked_10(object sender, RoutedEventArgs e)
        {
            audio.sfx.Volume = 0.1;
            MusicVolumeSet(0);
            MenuItem_Volume10.IsChecked = true;
        }

        private void MenuItem_Sound_Click(object sender, RoutedEventArgs e)
        {
            if (MenuItem_Sound.IsChecked)
            {
                audio.active = true;
            }
            else
            {
                audio.active = false;
                audio.music.Stop();
                audio.sfx.Stop();
            }
        }

        private void Button_Trade_Click(object sender, RoutedEventArgs e)
        {
            TradeWindow trade = new TradeWindow();
            trade.ShowDialog();
            RefreshOwnersUI();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            SaveGame();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            LoadGame();
        }
    }
}
