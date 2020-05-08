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

namespace Monopoly_Server
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StreamWriter serverStreamWriter;
        private StreamReader serverStreamReader;
        public MainWindow()
        {

        }

        private bool StartServer()
        {
            //create server's tcp listener for incoming connection
            TcpListener tcpServerListener = new TcpListener(4444);
            tcpServerListener.Start();        //start server
            label.Content="Server Started";
            //block tcplistener to accept incoming connection
            Socket serverSocket = tcpServerListener.AcceptSocket();

            try
            {
                if (serverSocket.Connected)
                {
                    label.Content = "Client connected";
                    //open network stream on accepted socket
                    NetworkStream serverSockStream =
                        new NetworkStream(serverSocket);
                    serverStreamWriter =
                        new StreamWriter(serverSockStream);
                    serverStreamReader =
                        new StreamReader(serverSockStream);
                }
            }
            catch (Exception e)
            {
                label.Content = e.StackTrace;
                return false;
            }

            return true;
        }

        private void btnStartServer_Click(object sender, RoutedEventArgs e)
        {
            //start server
            if (!StartServer())
                label.Content = "Unable to start server";

            //sending n receiving msgs
            while (true)
            {
                label.Content = "CLIENT: " + serverStreamReader.ReadLine();
                serverStreamWriter.WriteLine("Hi!");
                serverStreamWriter.Flush();
            }
        }
    }
}