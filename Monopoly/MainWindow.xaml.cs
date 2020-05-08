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

namespace Monopoly
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StreamReader clientStreamReader;
        private StreamWriter clientStreamWriter;
        Random rng = new Random();
        public class Game
        {
            public byte player1location=0;
            public byte player2location=0;
            public byte player3location=0;
            public byte player4location=0;
            public byte dice1;
            public byte dice2;
        }

        BoardLocations boardLocations = new BoardLocations();
        Game game = new Game();
        public MainWindow()
        {
            InitializeComponent();
        }

        // SERVER CODE
        // //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private bool ConnectToServer()
        {
            //connect to server at given port
            try
            {
                TcpClient tcpClient = new TcpClient("localhost", 4444);
                ConnectionStatus.Header = "Connected";
                //get a network stream from server
                NetworkStream clientSockStream = tcpClient.GetStream();
                clientStreamReader = new StreamReader(clientSockStream);
                clientStreamWriter = new StreamWriter(clientSockStream);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return false;
            }

            return true;
        }

        private void btnConnectToServer_Click(object sender, RoutedEventArgs e)
        {
            //connect to server
            if (!ConnectToServer())
                ConnectionStatus.Header = "Unable to connect to server";
        }

        private void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //send message to server
                clientStreamWriter.WriteLine("Hello!");
                clientStreamWriter.Flush();
                ConnectionStatus.Header = "SERVER: " + clientStreamReader.ReadLine();
            }
            catch (Exception se)
            {
                ConnectionStatus.Header = se.StackTrace;
            }
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
                if (game.player1location >= 40) { game.player1location = 0; }
                Canvas.SetLeft(Player1, boardLocations.playerlocation(true, game.player1location));
                Canvas.SetTop(Player1, boardLocations.playerlocation(false, game.player1location));
            }
        }
    }
}
