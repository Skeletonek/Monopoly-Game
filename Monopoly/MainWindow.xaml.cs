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
        public class Game
        {
            public byte player1location = 0;
            public byte player2location = 0;
            public byte player3location = 0;
            public byte player4location = 0;
            public byte dice1;
            public byte dice2;
            public byte selectedfield = 0;
            public byte[] fieldHouse = new byte[40];
            public string[] fieldOwner = new string[40];
        }

        BoardLocations boardLocations = new BoardLocations();
        Game game = new Game();
        public MainWindow()
        {
            client = new SimpleTcpClient();
            client.StringEncoder = Encoding.UTF8;
            client.DataReceived += Client_DataReceived;
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
            game.dice1 = Convert.ToByte(rng.Next(1, 7));
            game.dice2 = Convert.ToByte(rng.Next(1, 7));
            Dice1.Content = game.dice1;
            Dice2.Content = game.dice2;
            DiceScore.Content = game.dice1 + game.dice2;
            for (int i = 0; i < game.dice1 + game.dice2; i++)
            {
                game.player1location++;
                if (game.player1location >= 40) 
                { game.player1location = 0; }
                Canvas.SetLeft(Player1, boardLocations.playerlocation(true, game.player1location));
                Canvas.SetTop(Player1, boardLocations.playerlocation(false, game.player1location));
            }
            if (connectedToServer == true)
            { client.WriteLineAndGetReply("Test", TimeSpan.FromSeconds(3)); }


        }

        private void OverviewRefresh()
        {
            switch(game.selectedfield)
            {
                case 0:
                    Overview_Picture.Source = new BitmapImage(new Uri(@"\Resources\FieldStart.jpg", UriKind.Relative));
                    Overview_Name.Content = "Pole startu";
                    Overview_Price.Content = "- $";
                    Overview_Houses.Content = "-";
                    Overview_Owner.Content = "-";
                    break;
            }
        }
        private void Field1_MouseEnter(object sender, MouseEventArgs e)
        {
            game.selectedfield = 0;
            OverviewRefresh();
        }
    }
}
