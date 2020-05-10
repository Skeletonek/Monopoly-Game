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
        public bool[] fieldRailroad = new bool[40];
        public bool[] fieldExtra = new bool[40];
        public bool[] fieldTax = new bool[40];
        public int[] fieldTaxCost = new int[40];
        public string[] chanceText = new string[10];
        public byte[] chanceAction = new byte[10];
        public int[] chanceXValue = new int[10];
        public string[] commChestText = new string[10];
        public byte[] commChestAction = new byte[10];
        public int[] commChestXValue = new int[10];

        // chanceAction
        // 0 - Player earns x money from bank
        // 1 - Player have to pay x money to bank
        // 2 - Player have to move to the exact field
        // 3 - Player have to move x fields
        // 4 - Player earns x money from other players
        // 5 - Player have to renovate his buildings
        // 6 - Player is arrested 
        // 7 - Player have to pay x or choose other card
        // 8 - Player have a free exit-prison card

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
                        fieldRailroad[i] = false;
                        fieldExtra[i] = true;
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
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
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
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
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
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
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
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
                        fieldTax[i] = true;
                        fieldTaxCost[i] = 200;
                        break;

                    case 5:
                        fieldName[i] = "Dworzec Reading";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldRailroad1.jpg", UriKind.Relative));
                        fieldPrice[i] = 200;
                        fieldNoSetRent[i] = 0;
                        field1Rent[i] = 25;
                        field2Rent[i] = 50;
                        field3Rent[i] = 100;
                        field4Rent[i] = 200;
                        fieldHRent[i] = 0;
                        fieldSet1[i] = 0;
                        fieldSet2[i] = 0;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = true;
                        fieldExtra[i] = false;
                        fieldTax[i] = true;
                        fieldTaxCost[i] = 200;
                        break;

                    case 6:
                        fieldName[i] = "Oriental Avenue";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldLightblue1.jpg", UriKind.Relative));
                        fieldPrice[i] = 100;
                        fieldNoSetRent[i] = 6;
                        field1Rent[i] = 30;
                        field2Rent[i] = 90;
                        field3Rent[i] = 270;
                        field4Rent[i] = 400;
                        fieldHRent[i] = 550;
                        fieldSet1[i] = 8;
                        fieldSet2[i] = 9;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = true;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 7:
                        fieldName[i] = "Szansa";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldChance.jpg", UriKind.Relative));
                        fieldPrice[i] = 0;
                        fieldNoSetRent[i] = 0;
                        field1Rent[i] = 0;
                        field2Rent[i] = 0;
                        field3Rent[i] = 0;
                        field4Rent[i] = 0;
                        fieldHRent[i] = 0;
                        fieldSet1[i] = 0;
                        fieldSet2[i] = 0;
                        fieldChance[i] = true;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 8:
                        fieldName[i] = "Vermont Avenue";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldLightblue2.jpg", UriKind.Relative));
                        fieldPrice[i] = 100;
                        fieldNoSetRent[i] = 6;
                        field1Rent[i] = 30;
                        field2Rent[i] = 90;
                        field3Rent[i] = 270;
                        field4Rent[i] = 400;
                        fieldHRent[i] = 550;
                        fieldSet1[i] = 6;
                        fieldSet2[i] = 9;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 9:
                        fieldName[i] = "Connecticut Avenue";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldLightblue3.jpg", UriKind.Relative));
                        fieldPrice[i] = 120;
                        fieldNoSetRent[i] = 8;
                        field1Rent[i] = 40;
                        field2Rent[i] = 100;
                        field3Rent[i] = 300;
                        field4Rent[i] = 450;
                        fieldHRent[i] = 600;
                        fieldSet1[i] = 8;
                        fieldSet2[i] = 9;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 10:
                        fieldName[i] = "Więzienie";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldPrison.jpg", UriKind.Relative));
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
                        fieldRailroad[i] = false;
                        fieldExtra[i] = true;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 11:
                        fieldName[i] = "St. Charles Place";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldMagenta1.jpg", UriKind.Relative));
                        fieldPrice[i] = 140;
                        fieldNoSetRent[i] = 10;
                        field1Rent[i] = 50;
                        field2Rent[i] = 150;
                        field3Rent[i] = 450;
                        field4Rent[i] = 625;
                        fieldHRent[i] = 750;
                        fieldSet1[i] = 13;
                        fieldSet2[i] = 14;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 12:
                        fieldName[i] = "Elektrownia";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldElectric.jpg", UriKind.Relative));
                        fieldPrice[i] = 150;
                        fieldNoSetRent[i] = 0;
                        field1Rent[i] = 0;
                        field2Rent[i] = 0;
                        field3Rent[i] = 0;
                        field4Rent[i] = 0;
                        fieldHRent[i] = 0;
                        fieldSet1[i] = 29;
                        fieldSet2[i] = 0;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = true;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 13:
                        fieldName[i] = "States Avenue";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldMagenta2.jpg", UriKind.Relative));
                        fieldPrice[i] = 140;
                        fieldNoSetRent[i] = 10;
                        field1Rent[i] = 50;
                        field2Rent[i] = 150;
                        field3Rent[i] = 450;
                        field4Rent[i] = 625;
                        fieldHRent[i] = 750;
                        fieldSet1[i] = 11;
                        fieldSet2[i] = 14;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 14:
                        fieldName[i] = "Virginia Avenue";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldMagenta3.jpg", UriKind.Relative));
                        fieldPrice[i] = 160;
                        fieldNoSetRent[i] = 12;
                        field1Rent[i] = 60;
                        field2Rent[i] = 180;
                        field3Rent[i] = 500;
                        field4Rent[i] = 700;
                        fieldHRent[i] = 900;
                        fieldSet1[i] = 11;
                        fieldSet2[i] = 13;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 15:
                        fieldName[i] = "Dworzec Pennsylvania";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldRailroad2.jpg", UriKind.Relative));
                        fieldPrice[i] = 200;
                        fieldNoSetRent[i] = 0;
                        field1Rent[i] = 25;
                        field2Rent[i] = 50;
                        field3Rent[i] = 100;
                        field4Rent[i] = 200;
                        fieldHRent[i] = 0;
                        fieldSet1[i] = 0;
                        fieldSet2[i] = 0;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = true;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;
                }
            }

            chanceText[0] = "Zapłać 50 zł";
            chanceAction[0] = 1;
            chanceXValue[0] = 50;

            commChestText[0] = "Pobierz 50 zł";
            commChestAction[0] = 0;
            commChestXValue[0] = 50;
        }
    }
}
