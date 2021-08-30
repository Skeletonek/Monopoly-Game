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
                Label_Player1Name.Content = Game.playername[0];
                Game.playerBankruptNeededToWin++;
            }
            if (Game.playerAvailable[1])
            {
                Label_Player2Name.Content = Game.playername[1];
                Game.playerBankruptNeededToWin++;
            }
            if (Game.playerAvailable[2])
            {
                Label_Player3Name.Content = Game.playername[2];
                Game.playerBankruptNeededToWin++;
            }
            if (Game.playerAvailable[3])
            {
                Label_Player4Name.Content = Game.playername[3];
                Game.playerBankruptNeededToWin++;
            }
            if (!connectedToServer)
            {
                Game.clientplayer = 0;
                Label_Player1Name.FontWeight = FontWeights.Bold;
                Game.clientCanThrowDice = true;
            }
            for(int i=0; i < 4; i++)
            {
                Game.playercash[i] = 1500;
                Game.playerlocation[i] = 0;
            }
            for (int i = 0; i < 41; i++)
            {
                Game.fieldOwner[i] = 4;
                Game.fieldHouse[i] = 0;
                Game.fieldPlayers[i] = 0;
            }
            Game.fieldPlayers[0] = 4;
            BoardData.gameDataWriter();
            GameLog.Text = "";
            audio.music.Stop();
            RefreshUI();
        }

        private void CallClient()
        {
            switch (FazeCoordinator())
            {
                case 1:
                    Game.clientCanThrowDice = true;
                    Game.clientCanEndTurn = false;
                    RefreshDiceUI();
                    break;

                case 2:
                    Game.clientCanThrowDice = false;
                    Game.clientCanEndTurn = true;
                    RefreshDiceUI();
                    break;

            }
        }
        private void CallAI()
        {
            switch (FazeCoordinator())
            {
                case 1:
                    ThrowDice();
                    break;

                case 2:
                    EndTurn();
                    break;

            }
        }
        private void GameCoordinator()
        {
            if (Game.turn == Game.clientplayer)
            {
                if (Game.faze == 0)
                {
                    if (!Game.hotseat)
                        GameLog.Text += "TWOJA TURA!" + Environment.NewLine + Environment.NewLine;
                    else
                        GameLog.Text += "TURA GRACZA " + Game.playername[Game.turn] + "!" + Environment.NewLine + Environment.NewLine;
                    audio.playSFX("correct");
                }
                CallClient();
            }
            else if (!Game.multiplayer)
            {
                CallAI();
            }
        }
        private sbyte FazeCoordinator()
        {
            switch (Game.faze)
            {
                case 0:
                    if (Game.playerArrestedTurns[Game.turn] == 0)
                    {
                        return 1; //Can Throw Dice
                    }
                    else
                    {
                        Game.playerArrestedTurns[Game.turn]--;
                        if (Game.playerArrestedTurns[Game.turn] == 0)
                        {
                            Game.playerlocation[Game.turn] = 10;
                            RefreshBoardUI();
                        }
                        return 2; //Can do something else
                    }

                case 1:
                    return 2; //Can do something else

                case 2:
                    return 3; //End (Unused)

                default:
                    return -1; //Error
            }
        }
        public void EndTurn()
        {
            do
            {
                Game.turn++;
                TurnCheck();
            } while (Game.playerBankrupt[Game.turn]);
            Game.faze = 0;
            GameCoordinator();
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
        public void ThrowDice()
        {
            Game.dice1 = Convert.ToByte(rng.Next(1, 7));
            Game.dice2 = Convert.ToByte(rng.Next(1, 7));
            diceScore = Convert.ToByte(Game.dice1 + Game.dice2);
            RefreshDiceUI();
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
                if (Game.playerlocation[Game.turn] < 40)
                {
                    Game.fieldPlayers[Game.playerlocation[Game.turn] - 1]--;
                    Game.fieldPlayers[Game.playerlocation[Game.turn]]++; //This is crashing multiplayer after exiting prison. (Probably fixed)
                }
                else
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
                Game.faze++;
                GameCoordinator();
                if (Game.multiplayer)
                    SendData();
            }
        }

        private void FieldCheck()
        {
            byte currentPlayerLocation = Game.playerlocation[Game.turn];
            int rent = BoardData.fieldNoSetRent[currentPlayerLocation];
            if (BoardData.fieldChance[currentPlayerLocation])
            {
                byte chanceCard = Convert.ToByte(rng.Next(0, BoardData.chanceCards));
                if (Game.turn == Game.clientplayer)
                {
                    CallClientAction(10, chanceCard);
                }
                else
                {
                    if (!Game.multiplayer)
                    {
                        if (DoChanceCard(chanceCard))
                        {
                            GameLog.Text += Game.playername[Game.turn] + " otrzymuje kartę szansy: " + BoardData.chanceText[chanceCard] + "!" + Environment.NewLine + Environment.NewLine;
                        }
                        else
                        {
                            MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                            Bankrupt();
                        }
                    }
                }

            }
            else if (BoardData.fieldCommChest[currentPlayerLocation] == true)
            {
                byte commChestCard = Convert.ToByte(rng.Next(0, BoardData.commChestCards));
                if (Game.turn == Game.clientplayer)
                {
                    CallClientAction(20, commChestCard);
                }
                else
                {
                    if (!Game.multiplayer)
                    {
                        if (DoCommChestCard(commChestCard))
                        {
                            GameLog.Text += Game.playername[Game.turn] + " otrzymuje kartę kasy społecznej: " + BoardData.commChestText[commChestCard] + "!" + Environment.NewLine + Environment.NewLine;
                        }
                        else
                        {
                            MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                            Bankrupt();
                        }
                    }
                }
            }
            else if (BoardData.fieldRailroad[currentPlayerLocation])
            {
                if (Game.fieldOwner[currentPlayerLocation] == 4)
                {
                    if (Game.turn == Game.clientplayer)
                    {
                        CallClientAction(30, currentPlayerLocation);
                    }
                    else
                    {
                        if (!Game.multiplayer)
                        {
                            if (BuyField(currentPlayerLocation))
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
                        CallClientAction(31, currentPlayerLocation, rent);
                    }
                    else
                    {
                        if (!Game.multiplayer)
                        {
                            if (!PayRent(currentPlayerLocation, rent))
                            {
                                MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                                Bankrupt();
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
                    CallClientAction(40, currentPlayerLocation);
                }
                else
                {
                    if (!Game.multiplayer)
                    {
                        if (PayTax(currentPlayerLocation))
                        {
                            GameLog.Text += Game.playername[Game.turn] + " płaci podatek w wysokości " + BoardData.fieldTaxCost[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                        }
                        else
                        {
                            MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                            Bankrupt();
                        }
                    }
                }
            }
            else if (BoardData.fieldExtra[currentPlayerLocation])
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
                        Game.playercash[Game.turn] += Game.taxmoney;
                        PlayerStatusRefresh();
                        GameLog.Text += Game.playername[Game.turn] + " odwiedza bezpłatny parking." + Environment.NewLine + Environment.NewLine;
                        if (Game.multiplayer)
                        {
                            SendGameLog(Game.playername[Game.turn] + " odwiedza bezpłatny parking." + Environment.NewLine + Environment.NewLine);
                        }
                        if (Game.turn == Game.clientplayer)
                        {
                            MessageBox.Show("Odwiedzasz bezpłatny parking.", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
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
                                CallClientAction(60, currentPlayerLocation);
                            }
                            else
                            {
                                if (!Game.multiplayer)
                                {
                                    if (BuyField(currentPlayerLocation))
                                    {
                                        GameLog.Text += Game.playername[Game.turn] + " kupuje " + BoardData.fieldName[currentPlayerLocation] + "!" + Environment.NewLine + Environment.NewLine;
                                    }
                                }
                            }
                        }
                        else if (Game.fieldOwner[currentPlayerLocation] != Game.turn)
                        {
                            int calculatedMoney = CalculateExtraFieldMultiplier(currentPlayerLocation);
                            if (Game.turn == Game.clientplayer)
                            {
                                CallClientAction(61, currentPlayerLocation, calculatedMoney);
                            }
                            else
                            {
                                if (!Game.multiplayer)
                                {
                                    if (!PayExtraFieldMultiplier(calculatedMoney, currentPlayerLocation))
                                    {
                                        MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                                        Bankrupt();
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
                        goto case 12;
                }
            }
            else // For normal estates
            {
                if (Game.fieldOwner[currentPlayerLocation] == 4)
                {
                    if (Game.turn == Game.clientplayer)
                    {
                        CallClientAction(50, currentPlayerLocation);
                    }
                    else
                    {
                        if (!Game.multiplayer)
                        {
                            if (BuyField(currentPlayerLocation))
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
                        CallClientAction(51, currentPlayerLocation, rent);
                    }
                    else
                    {
                        if (!Game.multiplayer)
                        {
                            if (!PayRent(currentPlayerLocation, rent))
                            {
                                MessageBox.Show("Przeciwnik bankrutuje!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                                Bankrupt();
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
        private void Bankrupt()
        {
            GameLog.Text += Game.playername[Game.turn] + " OGŁASZA BANKRUCTWO!" + Environment.NewLine + Environment.NewLine;
            if (Game.multiplayer)
            {
                SendGameLog(Game.playername[Game.turn] + " OGŁASZA BANKRUCTWO!" + Environment.NewLine + Environment.NewLine);
            }
            Game.playerBankrupt[Game.turn] = true;
            Game.fieldPlayers[Game.playerlocation[Game.turn]]--;
            for (int i = 1; i < 40; i++)
            {
                if (Game.fieldOwner[i] == Game.turn)
                {
                    Game.fieldOwner[i] = 4;
                    if (Game.fieldHouse[i] > 0)
                    {
                        Game.fieldHouse[i] = 0;
                    }
                }
            }
            Game.playerBankruptNeededToWin--;
            if (Game.turn == Game.clientplayer)
            {
                LeaveDangerZone();
                RefreshDiceUI();
            }
            RefreshUI();
            if (Game.playerBankruptNeededToWin <= 1)
            {
                MessageBox.Show("Koniec gry!", "Monopoly", MessageBoxButton.OK, MessageBoxImage.Information);
                if (Game.multiplayer)
                {
                    client.Disconnect();
                }
                ResetUI();
            }
            else
            {
                EndTurn();
            }
        }
        private bool BuyField(byte currentPlayerLocation)
        {
            if (Game.playercash[Game.turn] >= BoardData.fieldPrice[currentPlayerLocation])
            {
                Game.playercash[Game.turn] -= BoardData.fieldPrice[currentPlayerLocation];
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

        private bool PayRent(byte currentPlayerLocation, int rent)
        {
            if (Game.playercash[Game.turn] >= rent)
            {
                Game.playercash[Game.turn] -= rent;
                Game.playercash[Game.fieldOwner[currentPlayerLocation]] = Game.playercash[Game.fieldOwner[currentPlayerLocation]] + rent;
                audio.playSFX("money");
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool PayTax(byte currentPlayerLocation)
        {
            if (Game.playercash[Game.turn] >= BoardData.fieldTaxCost[currentPlayerLocation])
            {
                Game.playercash[Game.turn] = Game.playercash[Game.turn] - BoardData.fieldTaxCost[currentPlayerLocation];
                Game.taxmoney += BoardData.fieldTaxCost[currentPlayerLocation];
                audio.playSFX("money");
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool DoChanceCard(byte chanceCard)
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
                    Game.playercash[Game.turn] -= BoardData.chanceXValue[chanceCard];
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

        private bool DoCommChestCard(byte commChestCard)
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
                    Game.playercash[Game.turn] -= BoardData.commChestXValue[commChestCard];
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
        private int CalculateExtraFieldMultiplier(byte currentPlayerLocation)
        {
            if (Game.fieldOwner[BoardData.fieldSet1[currentPlayerLocation]] != Game.fieldOwner[currentPlayerLocation])
            {
                return (Game.dice1 + Game.dice2) * 4;
            }
            else
            {
                return (Game.dice1 + Game.dice2) * 10;
            }
        }

        private bool PayExtraFieldMultiplier(int calculatedMoney, byte currentPlayerLocation)
        {
            if (Game.playercash[Game.turn] >= calculatedMoney)
            {
                Game.playercash[Game.turn] -= calculatedMoney;
                Game.playercash[Game.fieldOwner[currentPlayerLocation]] = Game.playercash[Game.fieldOwner[currentPlayerLocation]] + calculatedMoney;
                audio.playSFX("money");
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool BuyHouse(byte selectedField)
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
                            Game.playercash[Game.turn] -= 50;
                            BuyHouse2(selectedField);
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
                            Game.playercash[Game.turn] -= 100;
                            BuyHouse2(selectedField);
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
                            Game.playercash[Game.turn] -= 150;
                            BuyHouse2(selectedField);
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
                            Game.playercash[Game.turn] -= 200;
                            BuyHouse2(selectedField);
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
        private void BuyHouse2(byte selectedField)
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
        public bool SellHouse(byte selectedField)
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
                        Game.playercash[Game.turn] += 25;
                        SellHouse2(selectedField);
                        return true;
                    }
                    return false;
                }
                else if (selectedField > 10 && selectedField < 20)
                {
                    if (Game.fieldHouse[selectedField] > 0)
                    {
                        Game.playercash[Game.turn] += 50;
                        SellHouse2(selectedField);
                        return true;
                    }
                    return false;
                }
                else if (selectedField > 20 && selectedField < 30)
                {
                    if (Game.fieldHouse[selectedField] > 0)
                    {
                        Game.playercash[Game.turn] += 75;
                        SellHouse2(selectedField);
                        return true;
                    }
                    return false;
                }
                else if (selectedField > 30 && selectedField < 40)
                {
                    if (Game.fieldHouse[selectedField] > 0)
                    {
                        Game.playercash[Game.turn] += 100;
                        SellHouse2(selectedField);
                        return true;
                    }
                    return false;
                }
                return false;
            }
            return false;
        }

        private void SellHouse2(byte selectedField)
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
        public bool SellField(byte selectedField)
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
