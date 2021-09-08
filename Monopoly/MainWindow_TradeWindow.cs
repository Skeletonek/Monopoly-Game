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
        string[] MoneyTraded = new string[2];
        byte PlayerTradeTarget;
        TradePlayer Player1Trade = new TradePlayer(0);
        TradePlayer Player2Trade = new TradePlayer(1);
        TradePlayer Player3Trade = new TradePlayer(2);
        TradePlayer Player4Trade = new TradePlayer(3);

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
                        Player1Trade.OwnedFields.Add((byte)fieldOwnerIteration);
                        break;
                    case 1:
                        Player2Trade.OwnedFields.Add((byte)fieldOwnerIteration);
                        break;
                    case 2:
                        Player3Trade.OwnedFields.Add((byte)fieldOwnerIteration);
                        break;
                    case 3:
                        Player4Trade.OwnedFields.Add((byte)fieldOwnerIteration);
                        break;
                }
            }
        }
        private TradePlayer currentTurnPlayer()
        {
            switch(Game.turn)
            {
                case 0:
                    return Player1Trade;
                case 1:
                    return Player2Trade;
                case 2:
                    return Player3Trade;
                case 3:
                    return Player4Trade;
                default:
                    return null;
            }
        }
        private void LoadItems_ClientPlayer()
        {
            foreach (byte x in currentTurnPlayer().OwnedFields)
            {
                FieldsComboBox_ClientPlayer.Items.Add(BoardData.fieldName[x]);
            }
            MoneySlider_ClientPlayer.Maximum = currentTurnPlayer().Cash;
        }
        private void AcceptTradeOffer()
        {
            //Giving districts to each other
            foreach (string x in List_ClientPlayer.Items)
            {
                Game.fieldOwner[Array.IndexOf(BoardData.fieldName, x)] = PlayerTradeTarget; //This is not ideal.
            }
            foreach (string x in List_SecondPlayer.Items)
            {
                Game.fieldOwner[Array.IndexOf(BoardData.fieldName, x)] = Game.turn;
            }
            //Giving money to each other
            Game.playercash[Game.turn] += -int.Parse(MoneyTraded[0].Substring(0, MoneyTraded[0].Length-2)) + int.Parse(MoneyTraded[1].Substring(0, MoneyTraded[1].Length - 2));
            Game.playercash[PlayerTradeTarget] += int.Parse(MoneyTraded[0].Substring(0, MoneyTraded[0].Length - 2)) + -int.Parse(MoneyTraded[1].Substring(0, MoneyTraded[1].Length - 2));
            Grid_Trade.Visibility = Visibility.Hidden;
        }
        private void MenuItem_Player_Click(object sender, RoutedEventArgs e)
        {
            PlayerTradeTarget = 1;
            FieldsComboBox_SecondPlayer.Items.Clear();
            foreach (byte x in Player2Trade.OwnedFields)
            {
                FieldsComboBox_SecondPlayer.Items.Add(BoardData.fieldName[x]);
            }
            MoneySlider_SecondPlayer.Maximum = Player2Trade.Cash;
            GroupBox_TradeRight.Header = Game.playername[1].ToString();
        }
        private void MenuItem_Player_Click_1(object sender, RoutedEventArgs e)
        {
            PlayerTradeTarget = 2;
            FieldsComboBox_SecondPlayer.Items.Clear();
            foreach (byte x in Player3Trade.OwnedFields)
            {
                FieldsComboBox_SecondPlayer.Items.Add(BoardData.fieldName[x]);
            }
            MoneySlider_SecondPlayer.Maximum = Game.playercash[2];
            GroupBox_TradeRight.Header = Game.playername[2].ToString();
        }
        private void MenuItem_Player_Click_2(object sender, RoutedEventArgs e)
        {
            PlayerTradeTarget = 3;
            FieldsComboBox_SecondPlayer.Items.Clear();
            foreach (byte x in Player4Trade.OwnedFields)
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
            if(List_ClientPlayer.Items.Contains(MoneyTraded[Game.turn]))
            {
                List_ClientPlayer.Items.Remove(MoneyTraded[Game.turn]);
            }
            MoneyTraded[0] = Convert.ToInt32(MoneySlider_ClientPlayer.Value).ToString() + " $";
            MoneyTextBox_ClientPlayer.Text = MoneyTraded[Game.turn];
            List_ClientPlayer.Items.Add(MoneyTraded[Game.turn]);
        }
        private void MoneySlider_SecondPlayer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (List_ClientPlayer.Items.Contains(MoneyTraded[PlayerTradeTarget]))
            {
                List_ClientPlayer.Items.Remove(MoneyTraded[PlayerTradeTarget]);
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
