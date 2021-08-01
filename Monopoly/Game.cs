using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    static public class Game
    {
        static public bool multiplayer = false;
        static public string[] playername = new string[] { "Gracz 1", "Gracz 2", "Gracz 3", "Gracz 4", "Mr. Nobody" };
        static public byte[] playerlocation = new byte[] { 0, 0, 0, 0 };
        static public int[] playercash = new int[] { 1500, 1500, 1500, 1500 };
        static public byte[] playerRailroadOwned = new byte[] { 0, 0, 0, 0 };
        static public byte[] playerArrestedTurns = new byte[] { 0, 0, 0, 0 };
        static public bool[] playerAvailable = new bool[] { false, false, false, false };
        static public bool[] playerBankrupt = new bool[] { false, false, false, false };
        static public byte playerBankruptNeededToWin = 0;
        static public byte clientplayer = 0;
        static public bool clientCanThrowDice = false;
        static public bool clientCanEndTurn = false;
        static public byte turn = 0;
        static public byte faze = 0;
        static public byte dice1;
        static public byte dice2;
        static public byte selectedField = 0;
        static public int currentFieldPrice = 0;
        static public bool currentFieldForSale = false;
        static public byte[] fieldHouse = new byte[41];
        static public byte[] fieldOwner = new byte[41];
        static public byte[] fieldPlayers = new byte[41];
        static public int taxmoney = 0;
        static public bool sellmode = false;
        static public bool dangerzone = false;
    }
}
