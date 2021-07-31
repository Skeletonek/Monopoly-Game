using System;
using System.Windows;
using System.Windows.Media;

namespace Monopoly
{
    public partial class MainWindow : Window
    {

        // Game CODE
        // //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void StartNewGame()
        {
            if (Game.playerAvailable[0])
            {
                Player1.Visibility = Visibility.Visible;
                Player1_Icon.Visibility = Visibility.Visible;
                Label_Player1Cash.Visibility = Visibility.Visible;
                Label_Player1Name.Visibility = Visibility.Visible;
                Label_Player1Name.Content = Game.playername[0];
                Game.playerBankruptNeededToWin++;
            }
            if (Game.playerAvailable[1])
            {
                Player2.Visibility = Visibility.Visible;
                Player2_Icon.Visibility = Visibility.Visible;
                Label_Player2Cash.Visibility = Visibility.Visible;
                Label_Player2Name.Visibility = Visibility.Visible;
                Label_Player2Name.Content = Game.playername[1];
                Game.playerBankruptNeededToWin++;
            }
            if (Game.playerAvailable[2])
            {
                Player3.Visibility = Visibility.Visible;
                Player3_Icon.Visibility = Visibility.Visible;
                Label_Player3Cash.Visibility = Visibility.Visible;
                Label_Player3Name.Visibility = Visibility.Visible;
                Label_Player3Name.Content = Game.playername[2];
                Game.playerBankruptNeededToWin++;
            }
            if (Game.playerAvailable[3])
            {
                Player4.Visibility = Visibility.Visible;
                Player4_Icon.Visibility = Visibility.Visible;
                Label_Player4Cash.Visibility = Visibility.Visible;
                Label_Player4Name.Visibility = Visibility.Visible;
                Label_Player4Name.Content = Game.playername[3];
                Game.playerBankruptNeededToWin++;
            }
            if (!connectedToServer)
            {
                Game.clientplayer = 0;
                Label_Player1Name.FontWeight = FontWeights.Bold;
                Button_ThrowDice.IsEnabled = true;
            }
            Game.playercash[0] = 1500;
            Game.playercash[1] = 1500;
            Game.playercash[2] = 1500;
            Game.playercash[3] = 1500;
            Game.playerlocation[0] = 0;
            Game.playerlocation[1] = 0;
            Game.playerlocation[2] = 0;
            Game.playerlocation[3] = 0;
            for (int i = 0; i < 41; i++)
            {
                Game.fieldOwner[i] = 4;
                Game.fieldHouse[i] = 0;
                Game.fieldPlayers[i] = 0;
            }
            Game.fieldPlayers[0] = 4;
            BoardData.gameDataWriter();
            //GameCanvas.Children.RemoveRange(44, 100);
            GameLog.Text = "";
            audio.music.Stop();
        }
        private void TurnCheck()
        {
            if (Game.playerAvailable[3])
            {
                if (Game.turn > 3)
                {
                    Game.turn = 0;
                }
            }
            else if (Game.playerAvailable[2])
            {
                if (Game.turn > 2)
                {
                    Game.turn = 0;
                }
            }
            else
            {
                if (Game.turn > 1)
                {
                    Game.turn = 0;
                }
            }
        }

        private void EndTurn()
        {
            Button_EndTurn.IsEnabled = false;
            Button_Trade.IsEnabled = false;
            TurnCheck();
            if (Game.turn == 0 && Game.playerArrestedTurns[Game.turn] == 0)
            {
                if (Game.playerBankrupt[Game.turn] == false)
                    EnableMove();
                else
                    Game.turn++;
            }
            else if (Game.turn == 0 && Game.playerArrestedTurns[Game.turn] != 0)
            {
                DisableMove();
            }
            if (Game.turn == 1 && Game.playerArrestedTurns[Game.turn] == 0)
            {
                if (Game.playerBankrupt[Game.turn] == false)
                    EnableMove();
                else
                {
                    Game.turn++;
                    TurnCheck();
                }
            }
            else if (Game.turn == 1 && Game.playerArrestedTurns[Game.turn] != 0)
            {
                DisableMove();
            }
            if (Game.turn == 2 && Game.playerArrestedTurns[Game.turn] == 0)
            {
                if (Game.playerBankrupt[Game.turn] == false)
                    EnableMove();
                else
                {
                    Game.turn++;
                    TurnCheck();
                }
            }
            else if (Game.turn == 2 && Game.playerArrestedTurns[Game.turn] != 0)
            {
                DisableMove();
            }
            if (Game.turn == 3 && Game.playerArrestedTurns[Game.turn] == 0)
            {
                if (Game.playerBankrupt[Game.turn] == false)
                    EnableMove();
                else
                {
                    Game.turn++;
                    TurnCheck();
                    EndTurn();
                }
            }
            else if (Game.turn == 3 && Game.playerArrestedTurns[Game.turn] != 0)
            {
                DisableMove();
            }
        }

        private void bankrupt()
        {
            GameLog.Text += Game.playername[Game.turn] + " OGŁASZA BANKRUCTWO!" + Environment.NewLine + Environment.NewLine;
            if (Game.multiplayer)
            {
                SendGameLog(Game.playername[Game.turn] + " OGŁASZA BANKRUCTWO!" + Environment.NewLine + Environment.NewLine);
            }
            if (Game.turn == Game.clientplayer)
            {
                Button_ThrowDice.IsEnabled = false;
                Button_EndTurn.IsEnabled = false;
            }
            Game.playerBankrupt[Game.turn] = true;
            Game.fieldPlayers[Game.playerlocation[Game.turn]]--;
            switch (Game.turn)
            {
                case 0:
                    Player1.Visibility = Visibility.Hidden;
                    Player1_Icon.Visibility = Visibility.Hidden;
                    break;

                case 1:
                    Player2.Visibility = Visibility.Hidden;
                    Player2_Icon.Visibility = Visibility.Hidden;
                    break;

                case 2:
                    Player3.Visibility = Visibility.Hidden;
                    Player3_Icon.Visibility = Visibility.Hidden;
                    break;

                case 3:
                    Player4.Visibility = Visibility.Hidden;
                    Player4_Icon.Visibility = Visibility.Hidden;
                    break;
            }
            Game.playerBankruptNeededToWin--;
            LeaveDangerZone();
            if (Game.playerBankruptNeededToWin <= 1)
            {
                MessageBox.Show("Koniec gry!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                if (Game.multiplayer)
                {
                    client.Disconnect();
                }
                this.Close();
            }
            EndTurn();

        }
        private void DangerZone()
        {
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
            Button_MouseMode.Content = "Tryb sprzedawania ulic";
            audio.playSFX("incorrect");
            if (Game.turn == Game.clientplayer)
                MessageBox.Show("Znajdujesz się w strefie zagrożenia! Sprzedaj budynki lub ulice aby móc zapłacić. Jeżeli nie możesz zrobić nic więcej, ogłoś swoje bankructwo", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
            Game.dangerzone = true;

        }
        private void LeaveDangerZone()
        {
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
            Button_MouseMode.Content = "Tryb budowania domów";
            Game.dangerzone = false;
        }
        private void EnableMove()
        {
            if (Game.clientplayer == Game.turn)
            {
                Game.sellmode = false;
                Button_MouseMode.Content = "Tryb budowania domów";
                Button_ThrowDice.IsEnabled = true;
                //if(!Game.multiplayer)
                //Button_Trade.IsEnabled = true;
                GameLog.Text += "TWOJA TURA!" + Environment.NewLine + Environment.NewLine;
                audio.playSFX("correct");
            }
            else
            {
                if (!Game.multiplayer)
                {
                    ThrowDiceAndMove();
                }
            }
            if (Game.multiplayer)
                SendData();
        }

        private void DisableMove()
        {
            Game.playerArrestedTurns[Game.turn]--;
            if (Game.multiplayer)
                SendData();
            if (Game.clientplayer == Game.turn)
            {
                Button_EndTurn.IsEnabled = true;
                if (Game.playerArrestedTurns[Game.turn] == 0)
                {
                    Game.playerlocation[Game.turn] = 10;
                }
            }
            else
            {
                if (!Game.multiplayer)
                {
                    if (Game.playerArrestedTurns[Game.turn] == 0)
                    {
                        Game.playerlocation[Game.turn] = 10;
                    }
                    Game.turn++;
                    EndTurn();
                }
            }
        }

        private void ThrowDiceAndMove()
        {
            Game.dice1 = Convert.ToByte(rng.Next(1, 7));
            Game.dice2 = Convert.ToByte(rng.Next(1, 7));
            DiceShow(Game.dice1, Game.dice2);
            diceScore = Convert.ToByte(Game.dice1 + Game.dice2);
            DiceScore.Content = diceScore;
            if (Game.multiplayer)
            {
                SendData();
            }
            wait.Start();
        }

        private void JumpingAnimation_Tick(object sender, EventArgs e)
        {
            if (diceScore > 0)
            {
                Game.playerlocation[Game.turn]++;
                Game.fieldPlayers[Game.playerlocation[Game.turn] - 1]--;
                Game.fieldPlayers[Game.playerlocation[Game.turn]]++;
                if (Game.playerlocation[Game.turn] >= 40)
                {
                    Game.fieldPlayers[40]--;
                    Game.fieldPlayers[0]++;
                    Game.playerlocation[Game.turn] = 0;
                    Game.playercash[Game.turn] = Game.playercash[Game.turn] + 200;
                    GameLog.Text += Game.playername[Game.turn] + " otrzymuje 200$ za przejście przez start!" + Environment.NewLine + Environment.NewLine;
                    if (Game.multiplayer)
                    {
                        SendGameLog(Game.playername[Game.turn] + " otrzymuje 200$ za przejście przez start!" + Environment.NewLine + Environment.NewLine);
                    }
                    PlayerStatusRefresh();
                }
                Jump();
                diceScore--;
                if (Game.multiplayer)
                    SendMoveData();
            }
            else
            {
                wait.Stop();
                Game.selectedField = Game.playerlocation[Game.turn];
                OverviewRefresh();
                FieldCheck();
                if (Game.turn != 0 && !Game.multiplayer)
                {
                    Game.turn++;
                    if (Game.turn > 3)
                    {
                        Game.turn = 0;
                    }
                    EndTurn();
                }
                else
                {
                    Button_EndTurn.IsEnabled = true;
                }
                if (Game.multiplayer)
                    SendData();
            }
        }

        private void FieldCheck()
        {
            byte currentPlayerLocation = Game.playerlocation[Game.turn];
            int rent = BoardData.fieldNoSetRent[currentPlayerLocation];
            if (BoardData.fieldChance[currentPlayerLocation] == true)
            {
                byte chanceCard = Convert.ToByte(rng.Next(0, 5));
                if (Game.turn == Game.clientplayer)
                {
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
                }
                else
                {
                    if (!Game.multiplayer)
                    {
                        if (doChanceCard(chanceCard))
                        {
                            GameLog.Text += Game.playername[Game.turn] + " otrzymuje kartę szansy: " + BoardData.chanceText[chanceCard] + "!" + Environment.NewLine + Environment.NewLine;
                        }
                        else
                        {
                            MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                            bankrupt();
                        }
                    }
                }

            }
            else if (BoardData.fieldCommChest[currentPlayerLocation] == true)
            {
                byte commChestCard = Convert.ToByte(rng.Next(0, 11));
                if (Game.turn == Game.clientplayer)
                {
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
                }
                else
                {
                    if (!Game.multiplayer)
                    {
                        if (doCommChestCard(commChestCard))
                        {
                            GameLog.Text += Game.playername[Game.turn] + " otrzymuje kartę kasy społecznej: " + BoardData.commChestText[commChestCard] + "!" + Environment.NewLine + Environment.NewLine;
                        }
                        else
                        {
                            MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                            bankrupt();
                        }
                    }
                }
            }
            else if (BoardData.fieldRailroad[currentPlayerLocation] == true)
            {
                if (Game.fieldOwner[currentPlayerLocation] == 4)
                {
                    if (Game.turn == Game.clientplayer)
                    {
                        MessageBoxResult result = MessageBox.Show("Czy chcesz kupić " + BoardData.fieldName[currentPlayerLocation] + "?", "Monopoly", MessageBoxButton.YesNo, MessageBoxImage.Question);
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
                    }
                    else
                    {
                        if (!Game.multiplayer)
                        {
                            if (buyField(currentPlayerLocation))
                            {
                                Game.playerRailroadOwned[Game.turn]++;
                                GameLog.Text += Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;

                            }
                        }
                    }
                }
                else if (Game.fieldOwner[currentPlayerLocation] != Game.turn)
                {
                    if (Game.playerRailroadOwned[Game.fieldOwner[currentPlayerLocation]] == 1)
                    {
                        rent = BoardData.field1Rent[currentPlayerLocation];
                    }
                    else if (Game.playerRailroadOwned[Game.fieldOwner[currentPlayerLocation]] == 2)
                    {
                        rent = BoardData.field2Rent[currentPlayerLocation];
                    }
                    else if (Game.playerRailroadOwned[Game.fieldOwner[currentPlayerLocation]] == 3)
                    {
                        rent = BoardData.field3Rent[currentPlayerLocation];
                    }
                    else if (Game.playerRailroadOwned[Game.fieldOwner[currentPlayerLocation]] >= 4)
                    {
                        rent = BoardData.field4Rent[currentPlayerLocation];
                    }
                    if (Game.turn == Game.clientplayer)
                    {
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
                    }
                    else
                    {
                        if (!Game.multiplayer)
                        {
                            if (!payRent(currentPlayerLocation, rent))
                            {
                                MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                                bankrupt();
                            }
                            else
                            {
                                GameLog.Text += Game.playername[Game.turn] + " płaci " + rent + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                            }
                        }
                    }
                }
            }
            else if (BoardData.fieldTax[currentPlayerLocation] == true)
            {
                if (Game.turn == Game.clientplayer)
                {
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
                }
                else
                {
                    if (!Game.multiplayer)
                    {
                        if (payTax(currentPlayerLocation))
                        {
                            GameLog.Text += Game.playername[Game.turn] + " płaci podatek w wysokości " + BoardData.fieldTaxCost[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                        }
                        else
                        {
                            MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                            bankrupt();
                        }
                    }
                }
            }
            else if (BoardData.fieldExtra[currentPlayerLocation] == true)
            {
                switch (currentPlayerLocation)
                {
                    case 10: //Prison
                        GameLog.Text += Game.playername[Game.turn] + " odwiedza więźniów! Jaki miły z niego człowiek!" + Environment.NewLine + Environment.NewLine;
                        if (Game.multiplayer)
                        {
                            SendGameLog(Game.playername[Game.turn] + " odwiedza więźniów! Jaki miły z niego człowiek!" + Environment.NewLine + Environment.NewLine);
                        }
                        break;

                    case 20: //Parking Lot
                        Game.playercash[Game.turn] = Game.playercash[Game.turn] + Game.taxmoney;
                        PlayerStatusRefresh();
                        GameLog.Text += Game.playername[Game.turn] + " zdobywa " + Game.taxmoney + "$!" + Environment.NewLine + Environment.NewLine;
                        if (Game.multiplayer)
                        {
                            SendGameLog(Game.playername[Game.turn] + " zdobywa " + Game.taxmoney + "$!" + Environment.NewLine + Environment.NewLine);
                        }
                        if (Game.turn == Game.clientplayer)
                        {
                            MessageBox.Show("Zdobywasz " + Game.taxmoney + "$!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        Game.taxmoney = 0;
                        break;

                    case 30: //Go To Jail
                        if (Game.turn == Game.clientplayer)
                        {
                            MessageBox.Show("Idziesz do więzienia!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        Game.playerlocation[Game.turn] = 40;
                        Game.playerArrestedTurns[Game.turn] = 2;
                        Jump();
                        GameLog.Text += Game.playername[Game.turn] + " zostaje aresztowany!" + Environment.NewLine + Environment.NewLine;
                        if (Game.multiplayer)
                        {
                            SendGameLog(Game.playername[Game.turn] + " zostaje aresztowany!" + Environment.NewLine + Environment.NewLine);
                        }
                        break;

                    case 12: //Electric Company
                        if (Game.fieldOwner[currentPlayerLocation] == 4)
                        {
                            if (Game.turn == Game.clientplayer)
                            {
                                MessageBoxResult result = MessageBox.Show("Czy chcesz kupić elektrownię?", "Monopoly", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                switch (result)
                                {
                                    case MessageBoxResult.Yes:
                                        if (!buyField(currentPlayerLocation))
                                        {
                                            MessageBox.Show("Nie stać Cię na elektrownię!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
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
                            }
                            else
                            {
                                if (!Game.multiplayer)
                                {
                                    if (buyField(currentPlayerLocation))
                                    {
                                        GameLog.Text += Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                                    }
                                }
                            }
                        }
                        else if (Game.fieldOwner[currentPlayerLocation] != Game.turn)
                        {
                            int calculatedMoney = calculateExtraFieldMultiplier(currentPlayerLocation);
                            if (Game.turn == Game.clientplayer)
                            {
                                MessageBox.Show("Stanąłeś na elektrowni gracza " + Game.fieldOwner[currentPlayerLocation] + ". Musisz mu zapłacić: " + calculatedMoney, "Monopoly", MessageBoxButton.OK, MessageBoxImage.Warning);
                                if (!payExtraFieldMultiplier(calculatedMoney, currentPlayerLocation))
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
                            }
                            else
                            {
                                if (!Game.multiplayer)
                                {
                                    if (!payExtraFieldMultiplier(calculatedMoney, currentPlayerLocation))
                                    {
                                        MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                                        bankrupt();
                                    }
                                    else
                                    {
                                        GameLog.Text += Game.playername[Game.turn] + " płaci " + calculatedMoney + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                                    }
                                }
                            }
                        }
                        break;

                    case 28: //Waterworks
                        if (Game.fieldOwner[currentPlayerLocation] == 4)
                        {
                            if (Game.turn == Game.clientplayer)
                            {
                                MessageBoxResult result = MessageBox.Show("Czy chcesz kupić wodociągi?", "Monopoly", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                switch (result)
                                {
                                    case MessageBoxResult.Yes:
                                        if (!buyField(currentPlayerLocation))
                                        {
                                            MessageBox.Show("Nie stać Cię na wodociągi!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Error);
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
                            }
                            else
                            {
                                if (!Game.multiplayer)
                                {
                                    if (buyField(currentPlayerLocation))
                                    {
                                        GameLog.Text += Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                                    }
                                }
                            }
                        }
                        else if (Game.fieldOwner[currentPlayerLocation] != Game.turn)
                        {
                            int calculatedMoney = calculateExtraFieldMultiplier(currentPlayerLocation);
                            if (Game.turn == Game.clientplayer)
                            {
                                MessageBox.Show("Stanąłeś na wodociągach gracza " + Game.fieldOwner[currentPlayerLocation] + ". Musisz mu zapłacić: " + calculatedMoney, "Monopoly", MessageBoxButton.OK, MessageBoxImage.Warning);
                                if (!payExtraFieldMultiplier(calculatedMoney, currentPlayerLocation))
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
                            }
                            else
                            {
                                if (!Game.multiplayer)
                                {
                                    if (!payExtraFieldMultiplier(calculatedMoney, currentPlayerLocation))
                                    {
                                        MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                                        bankrupt();
                                    }
                                    else
                                    {
                                        GameLog.Text += Game.playername[Game.turn] + " płaci " + calculatedMoney + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                                    }
                                }
                            }
                        }
                        break;
                }
            }
            else // For normal estates
            {
                if (Game.fieldOwner[currentPlayerLocation] == 4)
                {
                    if (Game.turn == Game.clientplayer)
                    {
                        MessageBoxResult result = MessageBox.Show("Czy chcesz kupić ulicę " + BoardData.fieldName[currentPlayerLocation] + "?", "Monopoly", MessageBoxButton.YesNo, MessageBoxImage.Question);
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

                            case MessageBoxResult.No:
                                break;
                        }
                    }
                    else
                    {
                        if (!Game.multiplayer)
                        {
                            if (buyField(currentPlayerLocation))
                            {
                                GameLog.Text += Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                            }
                        }
                    }
                }
                else if (Game.fieldOwner[currentPlayerLocation] != Game.turn)
                {
                    bool hasSet = false;
                    if (BoardData.fieldSet2[currentPlayerLocation] == 0)
                    {
                        if (Game.fieldOwner[currentPlayerLocation] == Game.turn && Game.fieldOwner[currentPlayerLocation] != 4 && Game.fieldOwner[currentPlayerLocation] == Game.fieldOwner[BoardData.fieldSet1[currentPlayerLocation]])
                        {
                            hasSet = true;
                        }
                    }
                    else
                    {
                        if (Game.fieldOwner[currentPlayerLocation] == Game.turn && Game.fieldOwner[currentPlayerLocation] != 4 && Game.fieldOwner[currentPlayerLocation] == Game.fieldOwner[BoardData.fieldSet1[currentPlayerLocation]] && Game.fieldOwner[currentPlayerLocation] == Game.fieldOwner[BoardData.fieldSet2[currentPlayerLocation]])
                        {
                            hasSet = true;
                        }
                    }
                    if (hasSet)
                    {
                        rent = BoardData.fieldNoSetRent[currentPlayerLocation] * 2;
                    }
                    if (Game.fieldHouse[currentPlayerLocation] == 1)
                    {
                        rent = BoardData.field1Rent[currentPlayerLocation];
                    }
                    else if (Game.fieldHouse[currentPlayerLocation] == 2)
                    {
                        rent = BoardData.field2Rent[currentPlayerLocation];
                    }
                    else if (Game.fieldHouse[currentPlayerLocation] == 3)
                    {
                        rent = BoardData.field3Rent[currentPlayerLocation];
                    }
                    else if (Game.fieldHouse[currentPlayerLocation] == 4)
                    {
                        rent = BoardData.field4Rent[currentPlayerLocation];
                    }
                    else if (Game.fieldHouse[currentPlayerLocation] >= 5)
                    {
                        rent = BoardData.fieldHRent[currentPlayerLocation];
                    }
                    if (Game.turn == Game.clientplayer)
                    {
                        MessageBox.Show("Stanąłeś na dzielnicy gracz " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + ". Musisz mu zapłacić: " + rent, "Monopoly", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    }
                    else
                    {
                        if (!Game.multiplayer)
                        {
                            if (!payRent(currentPlayerLocation, rent))
                            {
                                MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                                bankrupt();
                            }
                            else
                            {
                                GameLog.Text += Game.playername[Game.turn] + " płaci " + rent + "$ graczowi " + Game.playername[Game.fieldOwner[currentPlayerLocation]] + "!" + Environment.NewLine + Environment.NewLine;
                            }
                        }
                    }
                }
            }
            PlayerStatusRefresh();
            if (Game.multiplayer)
                SendData();
        }
        private bool buyField(byte currentPlayerLocation)
        {
            if (Game.playercash[Game.turn] >= BoardData.fieldPrice[currentPlayerLocation])
            {
                Game.playercash[Game.turn] = Game.playercash[Game.turn] - BoardData.fieldPrice[currentPlayerLocation];
                Game.fieldOwner[currentPlayerLocation] = Game.turn;
                DrawOwner(currentPlayerLocation, Game.turn);
                audio.playSFX("money");
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool payRent(byte currentPlayerLocation, int rent)
        {
            if (Game.playercash[Game.turn] >= rent)
            {
                Game.playercash[Game.turn] = Game.playercash[Game.turn] - rent;
                Game.playercash[Game.fieldOwner[currentPlayerLocation]] = Game.playercash[Game.fieldOwner[currentPlayerLocation]] + rent;
                audio.playSFX("money");
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool payTax(byte currentPlayerLocation)
        {
            if (Game.playercash[Game.turn] >= BoardData.fieldTaxCost[currentPlayerLocation])
            {
                Game.playercash[Game.turn] = Game.playercash[Game.turn] - BoardData.fieldTaxCost[currentPlayerLocation];
                Game.taxmoney = Game.taxmoney + BoardData.fieldTaxCost[currentPlayerLocation];
                audio.playSFX("money");
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool doChanceCard(byte chanceCard)
        {
            if (BoardData.chanceAction[chanceCard] == 0)
            {
                // No function here currently
                return true;
            }
            if (BoardData.chanceAction[chanceCard] == 1)
            {
                if (Game.playercash[Game.turn] >= BoardData.chanceXValue[chanceCard])
                {
                    Game.playercash[Game.turn] = Game.playercash[Game.turn] - BoardData.chanceXValue[chanceCard];
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                MessageBox.Show("Błąd związany z kartą szansy! Dalsza gra może zawierać błędy! Zrestartuj aplikację i zgłoś błąd twórcy!", "Ups...", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new System.Exception("Błąd związany z kartą szansy! Dalsza gra może zawierać błędy! Zrestartuj aplikację i zgłoś błąd twórcy!");
            }
        }

        private bool doCommChestCard(byte commChestCard)
        {
            if (BoardData.commChestAction[commChestCard] == 0)
            {
                Game.playercash[Game.turn] = Game.playercash[Game.turn] + BoardData.commChestXValue[commChestCard];
                return true;
            }
            if (BoardData.commChestAction[commChestCard] == 1)
            {
                if (Game.playercash[Game.turn] >= BoardData.commChestXValue[commChestCard])
                {
                    Game.playercash[Game.turn] = Game.playercash[Game.turn] - BoardData.commChestXValue[commChestCard];
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                MessageBox.Show("Błąd związany z kartą kasy społecznej! Dalsza gra może zawierać błędy! Zrestartuj aplikację i zgłoś błąd twórcy!", "Ups...", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new System.InvalidOperationException("Błąd związany z kartą kasy społecznej! Dalsza gra może zawierać błędy! Zrestartuj aplikację i zgłoś błąd twórcy!");
            }
        }
        private int calculateExtraFieldMultiplier(byte currentPlayerLocation)
        {
            if (Game.fieldOwner[BoardData.fieldSet1[currentPlayerLocation]] != Game.turn)
            {
                return Game.dice1 + Game.dice2 * 4;
            }
            else
            {
                return Game.dice1 + Game.dice2 * 10;
            }
        }

        private bool payExtraFieldMultiplier(int calculatedMoney, byte currentPlayerLocation)
        {
            if (Game.playercash[Game.turn] >= calculatedMoney)
            {
                Game.playercash[Game.turn] = Game.playercash[Game.turn] - calculatedMoney;
                Game.playercash[Game.fieldOwner[currentPlayerLocation]] = Game.playercash[Game.fieldOwner[currentPlayerLocation]] + calculatedMoney;
                audio.playSFX("money");
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool buyHouse(byte selectedField)
        {
            bool hasSet = false;
            if (BoardData.fieldSet2[selectedField] == 0)
            {
                if (Game.fieldOwner[selectedField] == Game.turn && Game.fieldOwner[selectedField] != 4 && Game.fieldOwner[selectedField] == Game.fieldOwner[BoardData.fieldSet1[selectedField]])
                {
                    hasSet = true;
                }
            }
            else
            {
                if (Game.fieldOwner[selectedField] == Game.turn && Game.fieldOwner[selectedField] != 4 && Game.fieldOwner[selectedField] == Game.fieldOwner[BoardData.fieldSet1[selectedField]] && Game.fieldOwner[selectedField] == Game.fieldOwner[BoardData.fieldSet2[selectedField]])
                {
                    hasSet = true;
                }
            }
            if (hasSet == true)
            {
                if (selectedField > 0 && selectedField < 9)
                {
                    if (Game.fieldHouse[selectedField] < 5)
                    {
                        if (Game.playercash[Game.turn] >= 50)
                        {
                            Game.playercash[Game.turn] = Game.playercash[Game.turn] - 50;
                            buyHouse2(selectedField);
                            return true;
                        }
                        return false;
                    }
                    return false;
                }
                else if (selectedField > 10 && selectedField < 20)
                {
                    if (Game.fieldHouse[selectedField] < 5)
                    {
                        if (Game.playercash[Game.turn] >= 100)
                        {
                            Game.playercash[Game.turn] = Game.playercash[Game.turn] - 100;
                            buyHouse2(selectedField);
                            return true;
                        }
                        return false;
                    }
                    return false;
                }
                else if (selectedField > 20 && selectedField < 30)
                {
                    if (Game.fieldHouse[selectedField] < 5)
                    {
                        if (Game.playercash[Game.turn] >= 150)
                        {
                            Game.playercash[Game.turn] = Game.playercash[Game.turn] - 150;
                            buyHouse2(selectedField);
                            return true;
                        }
                        return false;
                    }
                    return false;
                }
                else if (selectedField > 30 && selectedField < 40)
                {
                    if (Game.fieldHouse[selectedField] < 5)
                    {
                        if (Game.playercash[Game.turn] >= 200)
                        {
                            Game.playercash[Game.turn] = Game.playercash[Game.turn] - 200;
                            buyHouse2(selectedField);
                            return true;
                        }
                        return false;
                    }
                    return false;
                }
                return false;
            }
            return false;
        }
        private void buyHouse2(byte selectedField)
        {
            Game.fieldHouse[selectedField]++;
            audio.playSFX("build");
            GameLog.Text += Game.playername[Game.clientplayer] + " kupuje budynek w dzielnicy " + BoardData.fieldName[Game.selectedField] + Environment.NewLine + Environment.NewLine;
            if (Game.multiplayer)
            {
                SendGameLog(Game.playername[Game.clientplayer] + " kupuje budynek w dzielnicy " + BoardData.fieldName[Game.selectedField] + Environment.NewLine + Environment.NewLine);
            }
            switch (Game.fieldHouse[selectedField])
            {
                case 1:
                    DrawHouses(selectedField, 1);
                    break;

                case 2:
                    DrawHouses(selectedField, 2);
                    break;

                case 3:
                    DrawHouses(selectedField, 3);
                    break;

                case 4:
                    DrawHouses(selectedField, 4);
                    break;

                case 5:
                    DrawHouses(selectedField, 5);
                    break;

                default:
                    MessageBox.Show("Wystąpił błąd podczas wywołania instrukcji DrawHouses!", "Ups...", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw new InvalidOperationException("Wystąpił błąd podczas wywołania instrukcji DrawHouses!");
            }
            OverviewRefresh();
            PlayerStatusRefresh();
        }
        private bool sellHouse(byte selectedField)
        {
            bool hasSet = false;
            if (BoardData.fieldSet2[selectedField] == 0)
            {
                if (Game.fieldOwner[selectedField] == Game.turn && Game.fieldOwner[selectedField] != 4 && Game.fieldOwner[selectedField] == Game.fieldOwner[BoardData.fieldSet1[selectedField]])
                {
                    hasSet = true;
                }
            }
            else
            {
                if (Game.fieldOwner[selectedField] == Game.turn && Game.fieldOwner[selectedField] != 4 && Game.fieldOwner[selectedField] == Game.fieldOwner[BoardData.fieldSet1[selectedField]] && Game.fieldOwner[selectedField] == Game.fieldOwner[BoardData.fieldSet2[selectedField]])
                {
                    hasSet = true;
                }
            }
            if (hasSet == true)
            {
                if (selectedField > 0 && selectedField < 9)
                {
                    if (Game.fieldHouse[selectedField] > 0)
                    {
                        Game.playercash[Game.turn] = Game.playercash[Game.turn] + 25;
                        sellHouse2(selectedField);
                        return true;
                    }
                    return false;
                }
                else if (selectedField > 10 && selectedField < 20)
                {
                    if (Game.fieldHouse[selectedField] > 0)
                    {
                        Game.playercash[Game.turn] = Game.playercash[Game.turn] + 50;
                        sellHouse2(selectedField);
                        return true;
                    }
                    return false;
                }
                else if (selectedField > 20 && selectedField < 30)
                {
                    if (Game.fieldHouse[selectedField] > 0)
                    {
                        Game.playercash[Game.turn] = Game.playercash[Game.turn] + 75;
                        sellHouse2(selectedField);
                        return true;
                    }
                    return false;
                }
                else if (selectedField > 30 && selectedField < 40)
                {
                    if (Game.fieldHouse[selectedField] > 0)
                    {
                        Game.playercash[Game.turn] = Game.playercash[Game.turn] + 100;
                        sellHouse2(selectedField);
                        return true;
                    }
                    return false;
                }
                return false;
            }
            return false;
        }

        private void sellHouse2(byte selectedField)
        {
            Game.fieldHouse[selectedField]--;
            audio.playSFX("destroy");
            GameLog.Text += Game.playername[Game.clientplayer] + " sprzedaje budynek w dzielnicy " + BoardData.fieldName[Game.selectedField] + Environment.NewLine + Environment.NewLine;
            if (Game.multiplayer)
            {
                SendGameLog(Game.playername[Game.clientplayer] + " sprzedaje budynek w dzielnicy " + BoardData.fieldName[Game.selectedField] + Environment.NewLine + Environment.NewLine);
            }
            switch (Game.fieldHouse[selectedField])
            {
                case 0:
                    DrawHouses(selectedField, 0);
                    break;

                case 1:
                    DrawHouses(selectedField, 1);
                    break;

                case 2:
                    DrawHouses(selectedField, 2);
                    break;

                case 3:
                    DrawHouses(selectedField, 3);
                    break;

                case 4:
                    DrawHouses(selectedField, 4);
                    break;

                default:
                    MessageBox.Show("Wystąpił błąd podczas wywołania instrukcji DrawHouses!", "Ups...", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw new InvalidOperationException("Wystąpił błąd podczas wywołania instrukcji DrawHouses!");
            }
            OverviewRefresh();
            PlayerStatusRefresh();
        }
        private bool sellField(byte selectedField)
        {
            if (Game.fieldHouse[selectedField] == 0 && Game.fieldOwner[selectedField] == Game.turn)
            {
                Game.playercash[Game.fieldOwner[selectedField]] = Game.playercash[Game.fieldOwner[selectedField]] + (BoardData.fieldPrice[selectedField] / 2);
                Game.fieldOwner[selectedField] = 4;
                OverviewRefresh();
                PlayerStatusRefresh();
                DrawOwner(selectedField, Game.turn);
                return true;
            }
            return false;
        }
    }
}
