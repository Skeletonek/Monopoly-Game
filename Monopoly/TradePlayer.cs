using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    class TradePlayer
    {
        public byte ID;
        public List<byte> OwnedFields;
        public int Cash;
        //public bool ExitPrison_Chance; //<----{
        //public bool ExitPrison_Community; //<-{ Unused as there is no ExitPrison cards currently

        public TradePlayer(byte ID)
        {
            this.ID = ID;
            OwnedFields = new List<byte>();
            Cash = Game.playercash[this.ID];
        }

        public string ShowCashInString()
        {
            return Cash + " $";
        }
    }
}
