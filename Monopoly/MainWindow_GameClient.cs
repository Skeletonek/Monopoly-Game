using System;
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
                            Game.clientplayer++;
                            break;

                        case 1:
                            Game.clientplayer--;
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
        //private void OldJump()
        //{
        //    int xcord = 0;
        //    int ycord = 0;
        //    if (Game.fieldPlayers[Game.playerlocation[Game.turn]] <= 1)
        //    {
        //        xcord = boardLocations.playerlocation(true, Game.playerlocation[Game.turn]);
        //        ycord = boardLocations.playerlocation(false, Game.playerlocation[Game.turn]);
        //    }
        //    else if (Game.fieldPlayers[Game.playerlocation[Game.turn]] == 2)
        //    {
        //        xcord = boardLocations.playerlocation(true, Game.playerlocation[Game.turn]) + 22;
        //        ycord = boardLocations.playerlocation(false, Game.playerlocation[Game.turn]);
        //    }
        //    else if (Game.fieldPlayers[Game.playerlocation[Game.turn]] == 3)
        //    {
        //        xcord = boardLocations.playerlocation(true, Game.playerlocation[Game.turn]);
        //        ycord = boardLocations.playerlocation(false, Game.playerlocation[Game.turn]) + 22;
        //    }
        //    else if (Game.fieldPlayers[Game.playerlocation[Game.turn]] >= 4)
        //    {
        //        xcord = boardLocations.playerlocation(true, Game.playerlocation[Game.turn]) + 22;
        //        ycord = boardLocations.playerlocation(false, Game.playerlocation[Game.turn]) + 22;
        //    }
        //    switch (Game.turn)
        //    {
        //        case 0:
        //            Canvas.SetLeft(Player1, xcord);
        //            Canvas.SetTop(Player1, ycord);
        //            break;

        //        case 1:
        //            Canvas.SetLeft(Player2, xcord);
        //            Canvas.SetTop(Player2, ycord);
        //            break;

        //        case 2:
        //            Canvas.SetLeft(Player3, xcord);
        //            Canvas.SetTop(Player3, ycord);
        //            break;

        //        case 3:
        //            Canvas.SetLeft(Player4, xcord);
        //            Canvas.SetTop(Player4, ycord);
        //            break;
        //    }
        //}
        private void CallClientAction(byte ActionCode, byte AdditionalCode, int rent = 0)
        {
            byte currentPlayerLocation = 0;
            MessageBoxResult result = MessageBoxResult.OK;
            switch (ActionCode)
            {
                case 1:
                    byte chanceCard = AdditionalCode;
                    MessageBox.Show(BoardData.chanceText[chanceCard], "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (!doChanceCard(chanceCard))
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

                case 2:
                    byte commChestCard = AdditionalCode;
                    MessageBox.Show(BoardData.commChestText[commChestCard], "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (!doCommChestCard(commChestCard))
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

                case 3:
                    currentPlayerLocation = AdditionalCode;
                    result = MessageBox.Show("Czy chcesz kupić " + BoardData.fieldName[currentPlayerLocation] + "?", "Monopoly", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            if (!buyField(currentPlayerLocation))
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

                case 4:
                    currentPlayerLocation = AdditionalCode;
                    MessageBox.Show("Stanąłeś na dworcu gracza " + Game.fieldOwner[currentPlayerLocation] + ". Musisz mu zapłacić: " + rent, "Monopoly", MessageBoxButton.OK, MessageBoxImage.Warning);
                    if (!payRent(currentPlayerLocation, rent))
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

                case 5:
                    currentPlayerLocation = AdditionalCode;
                    MessageBox.Show("Musisz zapłacić podatek w wysokości " + BoardData.fieldTaxCost[currentPlayerLocation] + "$.", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    if (!payTax(currentPlayerLocation))
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

                case 6:
                    currentPlayerLocation = AdditionalCode;
                    result = MessageBox.Show("Czy chcesz kupić ulicę " + BoardData.fieldName[currentPlayerLocation] + "?", "Monopoly", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            if (!buyField(currentPlayerLocation))
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
                                    MessageBox.Show("Od teraz możesz kupować domy w tej dzielnicy! Aby kupić, kliknij na dane pole lewym przyciskiem myszy przed zakończeniem tury", "Monopoly", MessageBoxButton.YesNo, MessageBoxImage.Information);
                                }
                            }
                            break;
                    }
                    break;

                 //Not Completed Yet
            }
        }
    }
}
