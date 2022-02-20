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
        byte PlayerTradeTarget;
        TradePlayer[] PlayerTrade;
        List<sbyte> PlayersForTrade;
        string[] MoneyTraded = new string[32];

        private void LoadTrading()
        {
            PlayerTrade = new TradePlayer[4]
            {
                new TradePlayer(0),
                new TradePlayer(1),
                new TradePlayer(2),
                new TradePlayer(3)
            };
            PlayersForTrade = new List<sbyte>() { 0, 1, 2, 3 };
            //CheckFieldOwners();
            LoadItems_ClientPlayer();
            GroupBox_TradeLeft.Header = Game.playername[Game.clientplayer].ToString();
            PlayersForTrade.Remove(Game.clientplayer);
        }
        private void LoadItems_ClientPlayer()
        {
            FieldsComboBox_ClientPlayer.Items.Clear();
            foreach (byte x in PlayerTrade[Game.turn].OwnedFields)
            {
                FieldsComboBox_ClientPlayer.Items.Add(BoardData.fieldName[x]);
            }
            MoneySlider_ClientPlayer.Maximum = PlayerTrade[Game.turn].Cash;
        }
        private void MenuItem_Player_Click(object sender, RoutedEventArgs e)
        {
            PlayerTradeTarget = 1;
            FieldsComboBox_SecondPlayer.Items.Clear();
            foreach (byte x in PlayerTrade[PlayersForTrade[0]].OwnedFields)
            {
                FieldsComboBox_SecondPlayer.Items.Add(BoardData.fieldName[x]);
            }
            MoneySlider_SecondPlayer.Maximum = PlayerTrade[PlayersForTrade[0]].Cash;
            GroupBox_TradeRight.Header = PlayerTrade[PlayersForTrade[0]].ToString();
        }
        private void MenuItem_Player_Click_1(object sender, RoutedEventArgs e)
        {
            PlayerTradeTarget = 2;
            FieldsComboBox_SecondPlayer.Items.Clear();
            foreach (byte x in PlayerTrade[PlayersForTrade[1]].OwnedFields)
            {
                FieldsComboBox_SecondPlayer.Items.Add(BoardData.fieldName[x]);
            }
            MoneySlider_SecondPlayer.Maximum = PlayerTrade[PlayersForTrade[1]].Cash;
            GroupBox_TradeRight.Header = PlayerTrade[PlayersForTrade[1]].ToString();
        }
        private void MenuItem_Player_Click_2(object sender, RoutedEventArgs e)
        {
            PlayerTradeTarget = 3;
            FieldsComboBox_SecondPlayer.Items.Clear();
            foreach (byte x in PlayerTrade[PlayersForTrade[2]].OwnedFields)
            {
                FieldsComboBox_SecondPlayer.Items.Add(BoardData.fieldName[x]);
            }
            MoneySlider_SecondPlayer.Maximum = PlayerTrade[PlayersForTrade[2]].Cash;
            GroupBox_TradeRight.Header = PlayerTrade[PlayersForTrade[2]].ToString();
        }
        private void AcceptTradeOffer()
        {
            //Giving districts to each other
            foreach (string x in List_ClientPlayer.Items)
            {
                try
                {
                    Game.fieldOwner[Array.IndexOf(BoardData.fieldName, x)] = PlayerTradeTarget; //This is not ideal.
                }
                catch(IndexOutOfRangeException e)
                {

                }
            }
            foreach (string x in List_SecondPlayer.Items)
            {
                try
                {
                    Game.fieldOwner[Array.IndexOf(BoardData.fieldName, x)] = Game.turn;
                }
                catch(IndexOutOfRangeException e)
                {

                }
            }
            //Giving money to each other
            Game.playercash[Game.turn] += (int)MoneySlider_SecondPlayer.Value + -(int)MoneySlider_ClientPlayer.Value;
            Game.playercash[PlayerTradeTarget] += (int)MoneySlider_ClientPlayer.Value + -(int)MoneySlider_SecondPlayer.Value;
            Grid_Trade.Visibility = Visibility.Hidden;
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
            if(List_ClientPlayer.Items.Contains(MoneyTraded[Game.turn]))
            {
                List_ClientPlayer.Items.Remove(MoneyTraded[Game.turn]);
            }
            MoneyTraded[Game.clientplayer] = Convert.ToInt32(MoneySlider_ClientPlayer.Value).ToString() + " $";
            MoneyTextBox_ClientPlayer.Text = MoneyTraded[Game.turn];
            List_ClientPlayer.Items.Add(MoneyTraded[Game.turn]);
        }
        private void MoneySlider_SecondPlayer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (List_SecondPlayer.Items.Contains(MoneyTraded[PlayerTradeTarget]))
            {
                List_SecondPlayer.Items.Remove(MoneyTraded[PlayerTradeTarget]);
            }
            MoneyTraded[PlayerTradeTarget] = Convert.ToInt32(MoneySlider_SecondPlayer.Value).ToString() + " $";
            MoneyTextBox_SecondPlayer.Text = MoneyTraded[PlayerTradeTarget];
            List_SecondPlayer.Items.Add(MoneyTraded[PlayerTradeTarget]);
        }
        private void Trade_Button_Click(object sender, RoutedEventArgs e)
        {
            AcceptTradeOffer();
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
        private void LockOfferButtons()
        {
            Reject_Trade_Button.IsEnabled = false;
            Change_Trade_Button.IsEnabled = false;
            Accept_Trade_Button.IsEnabled = false;
        }
        private void LockTradeSettings()
        {

        }
    }
}
