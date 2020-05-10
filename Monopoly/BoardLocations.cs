using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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

    class BoardData
    {
        public string[] fieldName = new string[40];
        public BitmapImage[] fieldIcon = new BitmapImage[40];
        public int[] fieldPrice = new int[40];
        public int[] fieldNoSetRent = new int[40];
        public int[] field1Rent = new int[40];
        public int[] field2Rent = new int[40];
        public int[] field3Rent = new int[40];
        public int[] field4Rent = new int[40];
        public int[] fieldHRent = new int[40];
        public byte[] fieldSet1 = new byte[40];
        public byte[] fieldSet2 = new byte[40];
        public bool[] fieldChance = new bool[40];
        public bool[] fieldCommChest = new bool[40];
        public bool[] fieldTax = new bool[40];
        public int[] fieldTaxCost = new int[40];

        public void gameDataWriter()
        {
            for (int i = 0; i < 40; i++)
            {
                switch(i)
                {
                    case 0:
                        fieldName[i] = "Pole startu";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldStart.jpg", UriKind.Relative));
                        fieldPrice[i] = 0;
                        fieldNoSetRent[i] = 0;
                        field1Rent[i] = 0;
                        field2Rent[i] = 0;
                        field3Rent[i] = 0;
                        field4Rent[i] = 0;
                        fieldHRent[i] = 0;
                        fieldSet1[i] = 0;
                        fieldSet2[i] = 0;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 1:
                        fieldName[i] = "Mediterranean Avenue";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldBronze1.jpg", UriKind.Relative));
                        fieldPrice[i] = 60;
                        fieldNoSetRent[i] = 2;
                        field1Rent[i] = 10;
                        field2Rent[i] = 30;
                        field3Rent[i] = 90;
                        field4Rent[i] = 160;
                        fieldHRent[i] = 250;
                        fieldSet1[i] = 3;
                        fieldSet2[i] = 0;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 2:
                        fieldName[i] = "Kasa społeczna";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldCommChest.jpg", UriKind.Relative));
                        fieldPrice[i] = 0;
                        fieldNoSetRent[i] = 0;
                        field1Rent[i] = 0;
                        field2Rent[i] = 0;
                        field3Rent[i] = 0;
                        field4Rent[i] = 0;
                        fieldHRent[i] = 0;
                        fieldSet1[i] = 0;
                        fieldSet2[i] = 0;
                        fieldChance[i] = false;
                        fieldCommChest[i] = true;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 3:
                        fieldName[i] = "Baltic Avenue";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldBronze2.jpg", UriKind.Relative));
                        fieldPrice[i] = 60;
                        fieldNoSetRent[i] = 4;
                        field1Rent[i] = 20;
                        field2Rent[i] = 60;
                        field3Rent[i] = 180;
                        field4Rent[i] = 320;
                        fieldHRent[i] = 450;
                        fieldSet1[i] = 1;
                        fieldSet2[i] = 0;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 4:
                        fieldName[i] = "Podatek dochodowy";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldTax1.jpg", UriKind.Relative));
                        fieldPrice[i] = 200;
                        fieldNoSetRent[i] = 0;
                        field1Rent[i] = 0;
                        field2Rent[i] = 0;
                        field3Rent[i] = 0;
                        field4Rent[i] = 0;
                        fieldHRent[i] = 0;
                        fieldSet1[i] = 0;
                        fieldSet2[i] = 0;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldTax[i] = true;
                        fieldTaxCost[i] = 200;
                        break;
                }
            }
        }
    }
}
