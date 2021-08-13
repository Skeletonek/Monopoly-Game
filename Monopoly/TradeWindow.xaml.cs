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
            GroupBox_TradeLeft.Name = Game.playername[Game.clientplayer];
        }
        private void CheckFieldOwners()
        {
            for (int fieldOwnerIteration = 0; fieldOwnerIteration < Game.fieldOwner.Length; fieldOwnerIteration++)
            {
                switch (Game.fieldOwner[fieldOwnerIteration])
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
            MoneySlider_ClientPlayer.Maximum = Game.playercash[0];
        }
        private void MenuItem_Player_Click(object sender, RoutedEventArgs e)
        {
            playerTrading = 1;
            FieldsComboBox_SecondPlayer.Items.Clear();
            foreach (byte x in Player2OwnedFields)
            {
                FieldsComboBox_SecondPlayer.Items.Add(BoardData.fieldName[x]);
            }
            MoneySlider_SecondPlayer.Maximum = Game.playercash[1];
            GroupBox_TradeRight.Name = Game.playername[1];
        }
        private void MenuItem_Player_Click_1(object sender, RoutedEventArgs e)
        {
            playerTrading = 2;
            FieldsComboBox_SecondPlayer.Items.Clear();
            foreach (byte x in Player3OwnedFields)
            {
                FieldsComboBox_SecondPlayer.Items.Add(BoardData.fieldName[x]);
            }
            MoneySlider_SecondPlayer.Maximum = Game.playercash[2];
            GroupBox_TradeRight.Name = Game.playername[2];
        }
        private void MenuItem_Player_Click_2(object sender, RoutedEventArgs e)
        {
            playerTrading = 3;
            FieldsComboBox_SecondPlayer.Items.Clear();
            foreach (byte x in Player4OwnedFields)
            {
                FieldsComboBox_SecondPlayer.Items.Add(BoardData.fieldName[x]);
            }
            MoneySlider_SecondPlayer.Maximum = Game.playercash[3];
            GroupBox_TradeRight.Name = Game.playername[3];
        }
        private void Button_AddDistrict_Click(object sender, RoutedEventArgs e)
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
        private void MoneySlider_ClientPlayer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MoneyTextBox_ClientPlayer.Text = Convert.ToInt32(MoneySlider_ClientPlayer.Value).ToString();
        }
        private void MoneySlider_SecondPlayer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MoneyTextBox_SecondPlayer.Text = Convert.ToInt32(MoneySlider_SecondPlayer.Value).ToString();
        }
        private void Trade_Button_Click(object sender, RoutedEventArgs e)
        {
            foreach(string x in List_ClientPlayer.Items)
            {
                Game.fieldOwner[Array.IndexOf(BoardData.fieldName, x)] = playerTrading;
            }
            foreach (string x in List_SecondPlayer.Items)
            {
                Game.fieldOwner[Array.IndexOf(BoardData.fieldName, x)] = 0;
            }
        }
    }
}
