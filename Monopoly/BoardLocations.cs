using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    class BoardLocations
    {
        int[] playerLocationX_DB = new int[40]
            {
            562,
            510,
            458,
            406,
            354,
            302,
            250,
            198,
            146,
            94,
            42,
            42,
            42,
            42,
            42,
            42,
            42,
            42,
            42,
            42,
            42,
            94,
            146,
            198,
            250,
            302,
            354,
            406,
            458,
            510,
            562,
            562,
            562,
            562,
            562,
            562,
            562,
            562,
            562,
            562,
            };
        int[] playerLocationY_DB = new int[40]
        {
            583,
            583,
            583,
            583,
            583,
            583,
            583,
            583,
            583,
            583,
            583,
            511,
            459,
            407,
            355,
            303,
            251,
            199,
            147,
            95,
            43,
            43,
            43,
            43,
            43,
            43,
            43,
            43,
            43,
            43,
            43,
            95,
            147,
            199,
            251,
            303,
            355,
            407,
            459,
            511,
        };

        public int playerlocation(bool x, int pos)
        {
            if(x==true)
            {
                return playerLocationX_DB[pos];
            }
            else
            {
                return playerLocationY_DB[pos];
            }
        }
        
    }
}
