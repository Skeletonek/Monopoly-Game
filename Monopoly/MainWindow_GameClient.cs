using System;
using System.Linq;
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
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Game.dangerzone)
            {
                Game.clientCanThrowDice = false;
                RefreshDiceUI();
                ThrowDice();
            }
            else
            {
                Bankrupt();
                RefreshUI();
            }
        }

        private void EndTurn_Click(object sender, RoutedEventArgs e)
        {
            if (!Game.dangerzone)
            {
                Game.clientCanEndTurn = false;
                RefreshDiceUI();
                if(Game.hotseat)
                {
                    switch(Game.clientplayer)
                    {
                        case 0:
                            Hotseat_ChangeClientPlayer(1);
                            break;

                        case 1:
                            if (!Game.playerAI[2])
                            {
                                Hotseat_ChangeClientPlayer(2);
                            }
                            else
                            {
                                Hotseat_ChangeClientPlayer(0);
                            }                              
                            break;

                        case 2:
                            if (!Game.playerAI[3])
                            {
                                Hotseat_ChangeClientPlayer(3);
                            }
                            else
                            {
                                Hotseat_ChangeClientPlayer(0);
                            }
                            break;

                        case 3:
                            Hotseat_ChangeClientPlayer(0);
                            break;
                    }
                }
                EndTurn();
            }
            else
            {
                FieldCheck();
            }
        }
        private void Hotseat_ChangeClientPlayer(sbyte player)
        {
            Game.clientplayer = player;
            foreach(Label label in Grid_GroupBox_Stats.Children.OfType<Label>())
            {
                label.FontWeight = FontWeights.Normal;
                if(label.Name == string.Concat("Label_Player",player + 1,"Name"))
                {
                    label.FontWeight = FontWeights.Bold;
                }
                if(label.Name == "Label_Player1Cash") //Not quite satisfied with that method but this should do
                {
                    break;
                }
            }
        }
        private void DangerZone()
        {
            Game.clientCanEndTurn = true;
            Game.clientCanThrowDice = true;
            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = Color.FromArgb(255, 255, 100, 100);
            this.Background = brush;
            Button_EndTurn.Background = brush;
            Button_EndTurn.Content = "Zapłać";
            Button_EndTurn.IsEnabled = true;
            Button_ThrowDice.Background = brush;
            Button_ThrowDice.Content = "Ogłoś bankructwo";
            Button_ThrowDice.IsEnabled = true;
            Game.sellmode = true;
            audio.playSFX("incorrect");
            if (Game.turn == Game.clientplayer)
                MessageBox.Show("Znajdujesz się w strefie zagrożenia! Sprzedaj budynki lub ulice aby móc zapłacić. Jeżeli nie możesz zrobić nic więcej, ogłoś swoje bankructwo", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            Game.dangerzone = true;
            RefreshDiceUI();

        }
        private void LeaveDangerZone()
        {
            Game.clientCanThrowDice = false;
            if (!Game.playerBankrupt[Game.clientplayer])
            Game.clientCanEndTurn = true;
            else
            Game.clientCanEndTurn = false;
            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = Color.FromArgb(255, 221, 221, 221);
            Button_EndTurn.Background = brush;
            Button_EndTurn.Content = "Zakończ turę";
            Button_EndTurn.IsEnabled = true;
            Button_ThrowDice.Background = brush;
            Button_ThrowDice.Content = "Rzuć koścmi";
            Button_ThrowDice.IsEnabled = false;
            brush.Color = Color.FromArgb(255, 255, 255, 255);
            this.Background = brush;
            Game.sellmode = false;
            Game.dangerzone = false;
            RefreshDiceUI();
        }
        private void Jump()
        {
            Canvas.SetLeft(Player1, boardLocations.playerlocation(true, Game.playerlocation[0]));
            Canvas.SetTop(Player1, boardLocations.playerlocation(false, Game.playerlocation[0]));
            if (Game.playerlocation[1] != 10)
            {
                Canvas.SetLeft(Player2, boardLocations.playerlocation(true, Game.playerlocation[1]) + 22);
                Canvas.SetTop(Player2, boardLocations.playerlocation(false, Game.playerlocation[1]));
            }
            else
            {
                Canvas.SetLeft(Player2, boardLocations.playerlocation(true, Game.playerlocation[1]) + 22);
                Canvas.SetTop(Player2, boardLocations.playerlocation(false, Game.playerlocation[1]) + 55);
            }
            Canvas.SetLeft(Player3, boardLocations.playerlocation(true, Game.playerlocation[2]));
            Canvas.SetTop(Player3, boardLocations.playerlocation(false, Game.playerlocation[2]) + 22);
            if (Game.playerlocation[3] != 10)
            {
                Canvas.SetLeft(Player4, boardLocations.playerlocation(true, Game.playerlocation[3]) + 22);
                Canvas.SetTop(Player4, boardLocations.playerlocation(false, Game.playerlocation[3]) + 22);
            }
            else
            {
                Canvas.SetLeft(Player4, boardLocations.playerlocation(true, Game.playerlocation[3]) + 44);
                Canvas.SetTop(Player4, boardLocations.playerlocation(false, Game.playerlocation[3]) + 55);
            }
        }
        private void CallClientAction(byte ActionCode, byte AdditionalCode, int rent = 0)
        {
            byte currentPlayerLocation = 0;
            MessageBoxResult result = MessageBoxResult.OK;
            switch (ActionCode)
            {
                case 10: //Chance
                    byte chanceCard = AdditionalCode;
                    MessageBox.Show(BoardData.chanceText[chanceCard], "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (!DoChanceCard(chanceCard))
                    {
                        MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                        Game.dangerzone = true;
                        DangerZone();
                    }
                    else
                    {
                        if (Game.dangerzone == true)
                        {
                            LeaveDangerZone();
                        }
                        GameLog.Text += Game.playername[Game.turn] + " otrzymuje kartę szansy: " + BoardData.chanceText[chanceCard] + "!" + Environment.NewLine + Environment.NewLine;
                        if (Game.multiplayer)
                        {
                            SendGameLog(Game.playername[Game.turn] + " otrzymuje kartę szansy: " + BoardData.chanceText[chanceCard] + "!" + Environment.NewLine + Environment.NewLine);
                        }
                    }
                    break;

                case 20: //Community Chest
                    byte commChestCard = AdditionalCode;
                    MessageBox.Show(BoardData.commChestText[commChestCard], "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (!DoCommChestCard(commChestCard))
                    {
                        MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                        Game.dangerzone = true;
                        DangerZone();
                    }
                    else
                    {
                        if (Game.dangerzone == true)
                        {
                            LeaveDangerZone();
                        }
                        GameLog.Text += Game.playername[Game.turn] + " otrzymuje kartę kasy społecznej: " + BoardData.commChestText[commChestCard] + "!" + Environment.NewLine + Environment.NewLine;
                        if (Game.multiplayer)
                        {
                            SendGameLog(Game.playername[Game.turn] + " otrzymuje kartę kasy społecznej: " + BoardData.commChestText[commChestCard] + "!" + Environment.NewLine + Environment.NewLine);
                        }
                    }
                    break;

                case 30: //Buy railroad
                    currentPlayerLocation = AdditionalCode;
                    result = MessageBox.Show("Czy chcesz kupić " + BoardData.fieldName[currentPlayerLocation] + "?", "Monopoly", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            if (!BuyField(currentPlayerLocation))
                            {
                                MessageBox.Show("Nie stać Cię na ten dworzec!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                Game.playerRailroadOwned[Game.turn]++;
                                GameLog.Text += Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                                if (Game.multiplayer)
                                {
                                    SendGameLog(Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine);
                                }
                            }
                            break;

                        case MessageBoxResult.No:
                            break;
                    }
                    break;

                case 31: //Pay for staying on a railroad
                    currentPlayerLocation = AdditionalCode;
                    MessageBox.Show("Stanąłeś na dworcu gracza " + Game.fieldOwner[currentPlayerLocation] + ". Musisz mu zapłacić: " + rent, "Monopoly", MessageBoxButton.OK, MessageBoxImage.Warning);
                    if (!PayRent(currentPlayerLocation, rent))
                    {
                        MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                        Game.dangerzone = true;
                        DangerZone();
                    }
                    else
                    {
                        if (Game.dangerzone == true)
                        {
                            LeaveDangerZone();
                        }
                        GameLog.Text += Game.playername[Game.turn] + " płaci " + rent + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                        if (Game.multiplayer)
                        {
                            SendGameLog(Game.playername[Game.turn] + " płaci " + rent + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine);
                        }
                    }
                    break;

                case 40: //Pay tax
                    currentPlayerLocation = AdditionalCode;
                    MessageBox.Show("Musisz zapłacić podatek w wysokości " + BoardData.fieldTaxCost[currentPlayerLocation] + "$.", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    if (!PayTax(currentPlayerLocation))
                    {
                        MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                        Game.dangerzone = true;
                        DangerZone();
                    }
                    else
                    {
                        if (Game.dangerzone == true)
                        {
                            LeaveDangerZone();
                        }
                        GameLog.Text += Game.playername[Game.turn] + " płaci podatek w wysokości " + BoardData.fieldTaxCost[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                        if (Game.multiplayer)
                        {
                            SendGameLog(Game.playername[Game.turn] + " płaci podatek w wysokości " + BoardData.fieldTaxCost[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine);
                        }
                    }
                    break;

                case 50: //Buy street
                    currentPlayerLocation = AdditionalCode;
                    result = MessageBox.Show("Czy chcesz kupić ulicę " + BoardData.fieldName[currentPlayerLocation] + "?", "Monopoly", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            if (!BuyField(currentPlayerLocation))
                            {
                                MessageBox.Show("Nie stać Cię na tą ulicę!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                GameLog.Text += Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                                if (Game.multiplayer)
                                {
                                    SendGameLog(Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine);
                                }
                                if (Game.fieldOwner[currentPlayerLocation] != 4 && Game.fieldOwner[currentPlayerLocation] == Game.fieldOwner[BoardData.fieldSet1[currentPlayerLocation]] && (Game.fieldOwner[currentPlayerLocation] == Game.fieldOwner[BoardData.fieldSet2[currentPlayerLocation]] || BoardData.fieldSet2[currentPlayerLocation] == 0))
                                {
                                    MessageBox.Show("Od teraz możesz kupować domy w tej dzielnicy! Aby kupić, kliknij na dane pole lewym przyciskiem myszy przed zakończeniem tury", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                            break;
                    }
                    break;

                case 51: //Pay for street
                    currentPlayerLocation = AdditionalCode;
                    MessageBox.Show("Stanąłeś na dzielnicy gracz " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + ". Musisz mu zapłacić: " + rent, "Monopoly", MessageBoxButton.OK, MessageBoxImage.Warning);
                    if (!PayRent(currentPlayerLocation, rent))
                    {
                        MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                        Game.dangerzone = true;
                        DangerZone();
                    }
                    else
                    {
                        if (Game.dangerzone == true)
                        {
                            LeaveDangerZone();
                        }
                        GameLog.Text += Game.playername[Game.turn] + " płaci " + rent + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                        if (Game.multiplayer)
                        {
                            SendGameLog(Game.playername[Game.turn] + " płaci " + rent + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine);
                        }
                    }
                    break;

                case 60: //Buy electric company / waterworks
                    currentPlayerLocation = AdditionalCode;
                    result = MessageBox.Show("Czy chcesz kupić " + BoardData.fieldName[currentPlayerLocation], "Monopoly", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            if (!BuyField(currentPlayerLocation))
                            {
                                MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                GameLog.Text += Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                                if (Game.multiplayer)
                                {
                                    SendGameLog(Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine);
                                }
                            }
                            break;

                        case MessageBoxResult.No:
                            break;
                    }
                    break;

                case 61: //Pay for electric company / waterworks
                    currentPlayerLocation = AdditionalCode;
                    int calculatedMoney = rent;
                    MessageBox.Show("Stanąłeś na " + BoardData.fieldName[currentPlayerLocation] + " gracza " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + ". Musisz mu zapłacić: " + calculatedMoney, "Monopoly", MessageBoxButton.OK, MessageBoxImage.Warning);
                    if (!PayExtraFieldMultiplier(calculatedMoney, currentPlayerLocation))
                    {
                        MessageBox.Show("Nie stać Cię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
                        Game.dangerzone = true;
                        DangerZone();
                    }
                    else
                    {
                        if (Game.dangerzone == true)
                        {
                            LeaveDangerZone();
                        }
                        GameLog.Text += Game.playername[Game.turn] + " płaci " + calculatedMoney + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                        if (Game.multiplayer)
                        {
                            SendGameLog(Game.playername[Game.turn] + " płaci " + calculatedMoney + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine);
                        }
                    }
                    break;
            }
        }
    }
}
