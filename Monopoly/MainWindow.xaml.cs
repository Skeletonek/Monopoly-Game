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
            RefreshBoardUI();
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

        private void MenuItem_Hotseat_Click(object sender, RoutedEventArgs e)
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
            Game.hotseat = true;
            playboardTheme = (string)newSingleplayerGame.ListBox_PlayboardTheme.SelectedItem;
            LoadTheme();
            StartNewGame();
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Uwaga! Zasady umieszczone na stronie mogą różnić się w zależności od wybranego motywu. Wersja beta tej gry, również może nie zawierać pewnych mechanik opisanych w instrukcji.", "Pomoc", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            System.Diagnostics.Process.Start("https://www.hasbro.com/common/instruct/00009.pdf");
        }
    }
}