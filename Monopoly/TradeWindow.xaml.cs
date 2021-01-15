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
using System.Windows.Shapes;

namespace Monopoly
{
    /// <summary>
    /// Logika interakcji dla klasy TradeWindow.xaml
    /// </summary>
    public partial class TradeWindow : Window
    {
        List<byte> Player1OwnedFields = new List<byte>();
        List<byte> Player2OwnedFields = new List<byte>();
        List<byte> Player3OwnedFields = new List<byte>();
        List<byte> Player4OwnedFields = new List<byte>();
        byte playerTrading;
        public TradeWindow()
        {
            InitializeComponent();
            CheckFieldOwners();
            LoadItems_ClientPlayer();
        }
        private void CheckFieldOwners()
        {
            for (int fieldOwnerIteration = 0; fieldOwnerIteration < MainWindow.Game.fieldOwner.Length; fieldOwnerIteration++)
            {
                switch (MainWindow.Game.fieldOwner[fieldOwnerIteration])
                {
                    case 0:
                        Player1OwnedFields.Add((byte)fieldOwnerIteration);
                        break;
                    case 1:
                        Player2OwnedFields.Add((byte)fieldOwnerIteration);
                        break;
                    case 2:
                        Player3OwnedFields.Add((byte)fieldOwnerIteration);
                        break;
                    case 3:
                        Player4OwnedFields.Add((byte)fieldOwnerIteration);
                        break;
                }
            }
        }
        private void LoadItems_ClientPlayer()
        {
            foreach(byte x in Player1OwnedFields)
            {
                FieldsComboBox_ClientPlayer.Items.Add(BoardData.fieldName[x]);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            playerTrading = 1;
            FieldsComboBox_SecondPlayer.Items.Clear();
            foreach (byte x in Player2OwnedFields)
            {
                FieldsComboBox_SecondPlayer.Items.Add(BoardData.fieldName[x]);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!List_ClientPlayer.Items.Contains(FieldsComboBox_ClientPlayer.SelectedItem))
            {
                List_ClientPlayer.Items.Add(FieldsComboBox_ClientPlayer.SelectedItem);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!List_SecondPlayer.Items.Contains(FieldsComboBox_SecondPlayer.SelectedItem))
            {
                List_SecondPlayer.Items.Add(FieldsComboBox_SecondPlayer.SelectedItem);
            }
        }

        private void Trade_Button_Click(object sender, RoutedEventArgs e)
        {
            foreach(string x in List_ClientPlayer.Items)
            {
                MainWindow.Game.fieldOwner[Array.IndexOf(BoardData.fieldName, x)] = playerTrading;
            }
            foreach (string x in List_SecondPlayer.Items)
            {
                MainWindow.Game.fieldOwner[Array.IndexOf(BoardData.fieldName, x)] = 0;
            }
        }
    }
}
