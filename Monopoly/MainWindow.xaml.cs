using NetComm;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using System.Text.RegularExpressions;

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
        public static bool NewSingleplayerGame_ClosedByGameStart = false;

        byte diceScore;
        string playboardTheme = "Monopoly Standard";
        public static string currentThemeDir = "Resources";

        //DispatcherTimer wait = new DispatcherTimer();
        DispatcherTimer reload = new DispatcherTimer();
        System.Windows.Forms.Timer wait = new System.Windows.Forms.Timer();

        Random rng = new Random();
        LoadingWindow loading = new LoadingWindow();
        BoardLocations boardLocations = new BoardLocations();
        Audio audio = new Audio();
        AI ai = new AI();

        public MainWindow()
        {
            loading.Show();
            this.Dispatcher.UnhandledException += App_DispatcherUnhandledException;
            for (int i = 0; i < 40; i++)
            {
                Game.fieldOwner[i] = 4;
                Game.fieldHouse[i] = 0;
                Game.fieldPlayers[i] = 0;
            }
            Game.fieldPlayers[0] = 4;
            BoardData.gameDataWriter();
            wait.Tick += JumpingAnimation_Tick;
            wait.Interval = 280;
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
            loading.Close();
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (!Directory.Exists("logs"))
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
            if (openFileDialog.ShowDialog() == true)
            {
                FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                string line;
                int index = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    switch (index)
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
            if (saveFileDialog.ShowDialog() == true)
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
            foreach (DirectoryInfo dir in dirs)
            {
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    if (file.Name == "BoardName.mtf")
                    {
                        AvailableBoards.Add(File.ReadAllText(file.FullName));
                    }
                }
            }
            return AvailableBoards;
        }
        private void LoadCurrentThemeDir()
        {
            foreach (string x in ThemeBoards)
            {
                string[] splittedText = x.Split(';');
                if (splittedText[0] == playboardTheme)
                {
                    currentThemeDir = splittedText[1];
                }
            }
        }

        // UI Programming
        // //////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma warning disable CS0108 // Składowa ukrywa dziedziczoną składową; brak słowa kluczowego new
        private void SizeChangedEvent(object sender, SizeChangedEventArgs e) => CanvasRenderScale();
#pragma warning restore CS0108 // Składowa ukrywa dziedziczoną składową; brak słowa kluczowego new
        private void StateChangedEvent(object sender, EventArgs e) => CanvasRenderScale();
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
        private void DiceShow(byte dice1, byte dice2)
        {
            Dice1.Source = new BitmapImage(new Uri(@"Resources/dice_" + dice1 + ".png", UriKind.Relative));
            Dice2.Source = new BitmapImage(new Uri(@"Resources/dice_" + dice2 + ".png", UriKind.Relative));
        }
        private void LoadTheme()
        {
            LoadCurrentThemeDir();
            ImageBrush iB = new ImageBrush();
            iB.ImageSource = new BitmapImage(new Uri(currentThemeDir + @"\monopolyboard.jpg", UriKind.Relative));
            GameCanvas.Background = iB;
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
            DiceScore.Content = "0";
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
            for (byte i = 1; i < 40; i++)
            {
                DrawOwner(i, 4);
                DrawHouses(i, 0);
            }

        }
        private void RefreshUI()
        {
            Player1.Visibility = VisibilityCheck(Game.playerAvailable[0]);
            Player1_Icon.Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/" + MainWindow.currentThemeDir + @"/BluePlayer.png"));
            Player1_Icon.Visibility = VisibilityCheck(Game.playerAvailable[0]);
            if (Game.playerAvailable[0])
            {
                Player1_Icon.Visibility = VisibilityCheck(!Game.playerBankrupt[0]);
                Player1.Visibility = VisibilityCheck(!Game.playerBankrupt[0]);
            }
            Label_Player1Cash.Visibility = VisibilityCheck(Game.playerAvailable[0]);
            Label_Player1Name.Visibility = VisibilityCheck(Game.playerAvailable[0]);
            Player2.Visibility = VisibilityCheck(Game.playerAvailable[1]);
            Player2_Icon.Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/" + MainWindow.currentThemeDir + @"/GreenPlayer.png"));
            Player2_Icon.Visibility = VisibilityCheck(Game.playerAvailable[1]);
            if (Game.playerAvailable[1])
            {
                Player2.Visibility = VisibilityCheck(!Game.playerBankrupt[1]);
                Player2_Icon.Visibility = VisibilityCheck(!Game.playerBankrupt[1]);
            }
            Label_Player2Cash.Visibility = VisibilityCheck(Game.playerAvailable[1]);
            Label_Player2Name.Visibility = VisibilityCheck(Game.playerAvailable[1]);
            Player3.Visibility = VisibilityCheck(Game.playerAvailable[2]);
            Player3_Icon.Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/" + MainWindow.currentThemeDir + @"/YellowPlayer.png"));
            Player3_Icon.Visibility = VisibilityCheck(Game.playerAvailable[2]);
            if (Game.playerAvailable[2])
            {
                Player3.Visibility = VisibilityCheck(!Game.playerBankrupt[2]);
                Player3_Icon.Visibility = VisibilityCheck(!Game.playerBankrupt[2]);
            }
            Label_Player3Cash.Visibility = VisibilityCheck(Game.playerAvailable[2]);
            Label_Player3Name.Visibility = VisibilityCheck(Game.playerAvailable[2]);
            Player4.Visibility = VisibilityCheck(Game.playerAvailable[3]);
            Player4_Icon.Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/" + MainWindow.currentThemeDir + @"/RedPlayer.png"));
            Player4_Icon.Visibility = VisibilityCheck(Game.playerAvailable[3]);
            if (Game.playerAvailable[3])
            {
                Player4.Visibility = VisibilityCheck(!Game.playerBankrupt[3]);
                Player4_Icon.Visibility = VisibilityCheck(!Game.playerBankrupt[3]);
            }
            Label_Player4Cash.Visibility = VisibilityCheck(Game.playerAvailable[3]);
            Label_Player4Name.Visibility = VisibilityCheck(Game.playerAvailable[3]);
            PlayerStatusRefresh();
            Jump();
            RefreshBoardUI();
            if (Game.dangerzone)
            {
                SolidColorBrush brush = new SolidColorBrush();
                brush.Color = Color.FromArgb(255, 255, 100, 100);
                this.Background = brush;
                Button_EndTurn.Background = brush;
                Button_ThrowDice.Background = brush;
                Game.sellmode = true;
            }
            else
            {
                SolidColorBrush brush = new SolidColorBrush();
                brush.Color = Color.FromArgb(255, 221, 221, 221);
                Button_EndTurn.Background = brush;
                Button_ThrowDice.Background = brush;
            }
            RefreshDiceUI();
            if (Game.sellmode)
            {
                Button_MouseMode.Content = "Tryb sprzedawania ulic";
            }
            else
            {
                Button_MouseMode.Content = "Tryb budowania domów";
            }
        }

        private Visibility VisibilityCheck(bool shouldBeVisible)
        {
            if (shouldBeVisible)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }
        private void RefreshBoardUI()
        {
            for (byte i = 1; i < 41; i++)
            {
                DrawOwner(i, Game.fieldOwner[i]);
                DrawHouses(i, Game.fieldHouse[i]);
            }
        }
        private void RefreshDiceUI()
        {
            DiceShow(Game.dice1, Game.dice2);
            DiceScore.Content = Game.dice1 + Game.dice2;
            if (Game.clientCanThrowDice)
            {
                Button_ThrowDice.IsEnabled = true;
            }
            else
            {
                Button_ThrowDice.IsEnabled = false;
            }
            if (Game.clientCanEndTurn)
            {
                Button_EndTurn.IsEnabled = true;
            }
            else
            {
                Button_EndTurn.IsEnabled = false;
            }
            if (Game.dangerzone)
            {
                Button_EndTurn.Content = "Zapłać";
                Button_ThrowDice.Content = "Ogłoś bankructwo";
            }
            else
            {
                Button_ThrowDice.Content = "Rzuć kośćmi";
                Button_EndTurn.Content = "Zakończ turę";
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
        private void MenuItem_StartNewSingle(object sender, RoutedEventArgs e) => StartingNewGame(false);
        private void StartingNewGame(bool hotseat)
        {
            NewSingleplayerGame newSingleplayerGame = new NewSingleplayerGame(ThemeBoards, hotseat);
            newSingleplayerGame.ShowDialog();
            if (NewSingleplayerGame_ClosedByGameStart)
            {
                Game.playername[0] = newSingleplayerGame.TextBox_Player1.Text;
                Game.playername[1] = newSingleplayerGame.TextBox_Player2.Text;
                Game.playername[2] = newSingleplayerGame.TextBox_Player3.Text;
                Game.playername[3] = newSingleplayerGame.TextBox_Player4.Text;
                Game.playerAvailable[0] = true;
                Game.playerAvailable[1] = true;
                Game.playerAvailable[2] = Convert.ToBoolean(newSingleplayerGame.CheckBox_AIActive1.IsChecked);
                Game.playerAvailable[3] = Convert.ToBoolean(newSingleplayerGame.CheckBox_AIActive2.IsChecked);
                Game.playerAI[0] = false;
                Game.playerAI[1] = !hotseat;
                Game.playerAI[2] = Convert.ToBoolean(newSingleplayerGame.CheckBox_AI1.IsChecked);
                Game.playerAI[3] = Convert.ToBoolean(newSingleplayerGame.CheckBox_AI2.IsChecked);
                Game.multiplayer = false;
                Game.hotseat = hotseat;
                playboardTheme = (string)newSingleplayerGame.ListBox_PlayboardTheme.SelectedItem;
                LoadTheme();
                StartNewGame();
                NewSingleplayerGame_ClosedByGameStart = false;
            }
        }
        private void MenuItem_StopGame_Click(object sender, RoutedEventArgs e)
        {
            if (!Game.multiplayer)
            {
                wait.Stop();
                ResetUI();
                Game.clientCanEndTurn = false;
                Game.clientCanThrowDice = false;
            }
        }
        private void MenuItem_Hotseat_Click(object sender, RoutedEventArgs e) => StartingNewGame(true);
        private void Button_MouseMode_Click(object sender, RoutedEventArgs e) => SellModeToggle();
        private void SellModeToggle()
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
        private enum Fields { BlueField, GreenField, YellowField, RedField, NoAlpha }
        private void DrawOwner(byte field, byte status)
        {
            Regex regex = new Regex(@"Field\d+Owner");
            int FieldNumber = 0;
            string FieldOwnerSplittedName = string.Empty;
            foreach (Image img in GameCanvas.Children)
            {
                if (regex.IsMatch(img.Name))
                {
                    FieldOwnerSplittedName = img.Name.Substring(5, 2);
                    if(!int.TryParse(FieldOwnerSplittedName, out FieldNumber))
                    {
                        FieldOwnerSplittedName = img.Name.Substring(5, 1);
                        int.TryParse(FieldOwnerSplittedName, out FieldNumber);
                    }
                    if (FieldNumber == field + 1)
                    {
                        img.Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/" + MainWindow.currentThemeDir + @"/" + (Fields)status + @".png"));
                        break;
                    }
                }
            }
        }
        private enum Houses { NoAlpha, House1, House2, House3, House4, Trivago }
        private void DrawHouses(byte field, byte status)
        {
            Regex regex = new Regex(@"Field\d+");
            byte FieldNumber = 0;
            foreach (Image img in GameCanvas.Children)
            {
                if (regex.IsMatch(img.Name))
                {
                    if (FieldNumber == field)
                    {
                        img.Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/" + MainWindow.currentThemeDir + @"/" + (Houses)status + @".png"));
                        break;
                    }
                    FieldNumber++;
                }
            }
        }
        private void Field_MouseEnter(object sender, MouseEventArgs e)
        {
            Regex regex = new Regex(@"Field\d+");
            byte FieldNumber = 0;
            foreach (Image img in GameCanvas.Children)
            {
                if (regex.IsMatch(img.Name))
                {
                    if (img == sender)
                    {
                        Game.selectedField = FieldNumber;
                        OverviewRefresh();
                        break;
                    }
                    FieldNumber++;
                }
            }
        }
        private void Field_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Regex regex = new Regex(@"Field\d+");
            byte FieldNumber = 0;
            foreach (Image img in GameCanvas.Children)
            {
                if (regex.IsMatch(img.Name))
                {
                    if (img == sender)
                    {
                        BuyHouseOrSellField(FieldNumber);
                        break;
                    }
                    FieldNumber++;
                }
            }
        }
        private void FieldNoHouseBuying_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Regex regex = new Regex(@"Field\d+");
            byte FieldNumber = 0;
            foreach (Image img in GameCanvas.Children)
            {
                if (regex.IsMatch(img.Name))
                {
                    if (img == sender)
                    {
                        CantBuyHouseOrSellField(FieldNumber);
                        break;
                    }
                    FieldNumber++;
                }
            }
        }
        private void Field_CantDoShitInDetroit_MouseUp(object sender, MouseButtonEventArgs e) => CantBuyHouseNorSellThisField();
        private void Field_CantDoShitInDetroit_MouseRightButtonUp(object sender, MouseButtonEventArgs e) => CantSellHouse();
        private void Field_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Regex regex = new Regex(@"Field\d+");
            byte FieldNumber = 0;
            foreach(Image img in GameCanvas.Children)
            {
                if (regex.IsMatch(img.Name))
                {
                    if (img == sender)
                    {
                        CanSellHouse(FieldNumber);
                        break;
                    }
                    FieldNumber++;
                }
            }
        }
        private void MenuItem_ChangeMusicVolume(object sender, RoutedEventArgs e)
        {
            int index = MenuItem_Volume.Items.IndexOf(sender);
            audio.sfx.Volume = (1.1 - (double)index / 10);
            foreach(MenuItem menu in MenuItem_Volume.Items)
            {
                if(menu != sender)
                menu.IsChecked = false;
            }
            audio.music.Volume = audio.sfx.Volume / 2;
            MenuItem menuItem = sender as MenuItem;
            menuItem.IsChecked = true;
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
            Grid_Trade.Visibility = VisibilityCheck(Grid_Trade.Visibility != Visibility.Visible);
            LoadTrading();
            RefreshBoardUI();
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
            else if(e.Key == Key.LeftShift)
            {
                SellModeToggle();
            }
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }
        private void MenuItem_Click_1(object sender, RoutedEventArgs e) => SaveGame();
        private void MenuItem_Click_2(object sender, RoutedEventArgs e) => LoadGame();
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Uwaga! Zasady umieszczone na stronie mogą różnić się w zależności od wybranego motywu. Wersja beta tej gry, również może nie zawierać pewnych mechanik opisanych w instrukcji.", "Pomoc", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            System.Diagnostics.Process.Start("https://www.hasbro.com/common/instruct/00009.pdf");
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if(!GameLog_ScrollViewer.IsMouseOver)
            GameLog_ScrollViewer.ScrollToEnd();
        }
    }
}