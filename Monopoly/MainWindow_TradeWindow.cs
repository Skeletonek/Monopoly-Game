using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Monopoly
{
    public partial class MainWindow : Window
    {
        List<byte> Player1OwnedFields = new List<byte>();
        List<byte> Player2OwnedFields = new List<byte>();
        List<byte> Player3OwnedFields = new List<byte>();
        List<byte> Player4OwnedFields = new List<byte>();
        string[] MoneyTraded = new string[2];
        byte playerTrading;

        private void LoadTrading()
        {
            CheckFieldOwners();
            LoadItems_ClientPlayer();
            GroupBox_TradeLeft.Header = Game.playername[Game.clientplayer].ToString();
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
            foreach (byte x in Player1OwnedFields)
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
            GroupBox_TradeRight.Header = Game.playername[1].ToString();
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
            GroupBox_TradeRight.Header = Game.playername[2].ToString();
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
            GroupBox_TradeRight.Header = Game.playername[3].ToString();
        }
        private void Button_ClientPlayer_AddDistrict_Click(object sender, RoutedEventArgs e)
        {
            if (!List_ClientPlayer.Items.Contains(FieldsComboBox_ClientPlayer.SelectedItem))
            {
                List_ClientPlayer.Items.Add(FieldsComboBox_ClientPlayer.SelectedItem);
            }
        }
        private void Button_SecondPlayer_AddDistrict_Click(object sender, RoutedEventArgs e)
        {
            if (!List_SecondPlayer.Items.Contains(FieldsComboBox_SecondPlayer.SelectedItem))
            {
                List_SecondPlayer.Items.Add(FieldsComboBox_SecondPlayer.SelectedItem);
            }
        }
        private void MoneySlider_ClientPlayer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(List_ClientPlayer.Items.Contains(MoneyTraded[0]))
            {
                List_ClientPlayer.Items.Remove(MoneyTraded[0]);
            }
            MoneyTraded[0] = Convert.ToInt32(MoneySlider_ClientPlayer.Value).ToString() + " $";
            MoneyTextBox_ClientPlayer.Text = MoneyTraded[0];
            List_ClientPlayer.Items.Add(MoneyTraded[0]);
        }
        private void MoneySlider_SecondPlayer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (List_ClientPlayer.Items.Contains(MoneyTraded[1]))
            {
                List_ClientPlayer.Items.Remove(MoneyTraded[1]);
            }
            MoneyTraded[1] = Convert.ToInt32(MoneySlider_SecondPlayer.Value).ToString() + " $";
            MoneyTextBox_SecondPlayer.Text = MoneyTraded[1];
            List_SecondPlayer.Items.Add(MoneyTraded[1]);
        }
        private void Trade_Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (string x in List_ClientPlayer.Items)
            {
                Game.fieldOwner[Array.IndexOf(BoardData.fieldName, x)] = playerTrading;
            }
            foreach (string x in List_SecondPlayer.Items)
            {
                Game.fieldOwner[Array.IndexOf(BoardData.fieldName, x)] = 0;
            }
            Grid_Trade.Visibility = Visibility.Hidden;
        }
        private void Reject_Trade_Click(object sender, RoutedEventArgs e)
        {
            Grid_Trade.Visibility = Visibility.Hidden;
        }
        private void Change_Trade_Click(object sender, RoutedEventArgs e)
        {
            //TODO
        }
        private void FieldsComboBox_ClientPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Game.selectedField = (byte)Array.IndexOf(BoardData.fieldName, FieldsComboBox_ClientPlayer.SelectedItem);
            OverviewRefresh();
        }
        private void FieldsComboBox_SecondPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Game.selectedField = (byte)Array.IndexOf(BoardData.fieldName, FieldsComboBox_SecondPlayer.SelectedItem);
            OverviewRefresh();
        }
    }
}
