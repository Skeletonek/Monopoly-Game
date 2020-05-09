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
using System.Threading;
using System.Windows.Threading;
using SimpleTCP;

namespace Monopoly
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SimpleTcpClient client;
        Random rng = new Random();
        bool connectedToServer = false;
        DispatcherTimer wait = new DispatcherTimer();
        byte diceScore;
        public class Game
        {
            public byte player1location = 0;
            public byte player2location = 0;
            public byte player3location = 0;
            public byte player4location = 0;
            public int player1cash = 1500;
            public int player2cash = 1500;
            public int player3cash = 1500;
            public int player4cash = 1500;
            public byte clientplayer = 1;
            public byte turn = 0;
            public byte dice1;
            public byte dice2;
            public byte selectedField = 0;
            public int currentFieldPrice = 0;
            public bool currentFieldForSale = false;
            public byte[] fieldHouse = new byte[40];
            public string[] fieldOwner = new string[40];
        }

        BoardLocations boardLocations = new BoardLocations();
        BoardData boardData = new BoardData();
        Game game = new Game();
        public MainWindow()
        {
            for(int i = 0; i<40; i++)
            {
                game.fieldOwner[i] = "Mr. Nobody";
                game.fieldHouse[i] = 0;
            }
            boardData.gameDataWriter();
            client = new SimpleTcpClient();
            client.StringEncoder = Encoding.UTF8;
            client.DataReceived += Client_DataReceived;
            wait.Interval = TimeSpan.FromMilliseconds(300);
            wait.Tick += Wait_Tick;
            InitializeComponent();
        }

        // SERVER CODE
        // //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void Client_DataReceived(object sender, Message e)
        {
            ConnectionStatus.Header = e.MessageString;
        }

        private void btnConnectToServer_Click(object sender, RoutedEventArgs e)
        {
            client.Connect("127.0.0.1", 4444);
            connectedToServer = true;
        }

        private void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            //client.
        }

        // GAME CODE
        // //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Space)
            {
                Canvas.SetLeft(Player1, boardLocations.playerlocation(true, game.player1location));
                Canvas.SetTop(Player1, boardLocations.playerlocation(false, game.player1location));
                game.player1location++;
                if (game.player1location >= 40) { game.player1location = 0; }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button_ThrowDice.IsEnabled = false;
            ThrowDiceAndMove();
            // Connection Test
            if (connectedToServer == true)
            { client.WriteLineAndGetReply("Test", TimeSpan.FromSeconds(3)); }
        }

        private void EndTurn_Click(object sender, RoutedEventArgs e)
        {
            EndTurn();
        }
        private void EndTurn()
        {
            if (game.turn == 0)
            {
                Button_EndTurn.IsEnabled = false;
                game.turn = 1;
                ThrowDiceAndMove();
            }
            else if (game.turn == 1)
            {
                game.turn = 0;
                Button_ThrowDice.IsEnabled = true;
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
            wait.Start();
        }

        private void Wait_Tick(object sender, EventArgs e)
        {
            if (diceScore > 0)
            {
                switch (game.turn)
                {
                    case 0:
                        game.player1location++;
                        if (game.player1location >= 40)
                        { game.player1location = 0; }
                        Canvas.SetLeft(Player1, boardLocations.playerlocation(true, game.player1location));
                        Canvas.SetTop(Player1, boardLocations.playerlocation(false, game.player1location));
                        break;

                    case 1:
                        game.player2location++;
                        if (game.player2location >= 40)
                        { game.player2location = 0; }
                        Canvas.SetLeft(Player2, boardLocations.playerlocation(true, game.player2location));
                        Canvas.SetTop(Player2, boardLocations.playerlocation(false, game.player2location));
                        break;
                }
                diceScore--;
            }
            else
            {
                wait.Stop();
                if(game.turn != 0)
                {
                    EndTurn();
                }
                else
                {
                    Button_EndTurn.IsEnabled = true;
                }
            }
        }

        private void FieldCheck()
        {
            byte currentPlayerLocation = 0;
            int currentPlayerBalance = 0;
            if(game.turn == 0)
            { currentPlayerLocation = game.player1location; }
            else if (game.turn == 1)
            { currentPlayerLocation = game.player2location; }
            switch(currentPlayerLocation)
            {
                case 0:
                    currentPlayerBalance += 200;
                    break;
                case 1:
                    if (game.fieldOwner[currentPlayerLocation] == "Mr. Nobody")
                    {
                        game.currentFieldForSale = true;
                        game.currentFieldPrice = 60;
                    }
                    else
                    {

                    }
                    break;

            }
        }
        private void OverviewRefresh()
        {
            Overview_Picture.Source = boardData.fieldIcon[game.selectedField];
            Overview_Name.Content = boardData.fieldName[game.selectedField];
            Overview_Price.Content = boardData.fieldPrice[game.selectedField] + " $";
            Overview_Houses.Content = game.fieldHouse[game.selectedField];
            Overview_Owner.Content = game.fieldOwner[game.selectedField];
            Overview_1Rent.Content = boardData.field1Rent[game.selectedField] + " $";
            Overview_2Rent.Content = boardData.field2Rent[game.selectedField] + " $";
            Overview_3Rent.Content = boardData.field3Rent[game.selectedField] + " $";
            Overview_4Rent.Content = boardData.field4Rent[game.selectedField] + " $";
            Overview_HRent.Content = boardData.fieldHRent[game.selectedField] + " $";
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
    }
}
