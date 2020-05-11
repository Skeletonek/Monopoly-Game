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
        int[] playerLocationX_DB = new int[41]
            {
            559,
            507,
            455,
            403,
            351,
            299,
            247,
            195,
            143,
            91,
            2,
            5,
            5,
            5,
            5,
            5,
            5,
            5,
            5,
            5,
            5,
            91,
            143,
            195,
            247,
            299,
            351,
            403,
            455,
            507,
            580,
            580,
            580,
            580,
            580,
            580,
            580,
            580,
            580,
            580,
            30
            };
        int[] playerLocationY_DB = new int[41]
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
            560,
            511,
            459,
            407,
            355,
            303,
            251,
            199,
            147,
            95,
            10,
            10,
            10,
            10,
            10,
            10,
            10,
            10,
            10,
            10,
            10,
            90,
            142,
            194,
            246,
            298,
            350,
            402,
            454,
            506,
            561
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

                    case 16:
                        fieldName[i] = "St. James Place";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldOrange1.jpg", UriKind.Relative));
                        fieldPrice[i] = 180;
                        fieldNoSetRent[i] = 14;
                        field1Rent[i] = 70;
                        field2Rent[i] = 200;
                        field3Rent[i] = 550;
                        field4Rent[i] = 750;
                        fieldHRent[i] = 950;
                        fieldSet1[i] = 18;
                        fieldSet2[i] = 19;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 17:
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

                    case 18:
                        fieldName[i] = "Tennesse Avenue";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldOrange2.jpg", UriKind.Relative));
                        fieldPrice[i] = 180;
                        fieldNoSetRent[i] = 14;
                        field1Rent[i] = 70;
                        field2Rent[i] = 200;
                        field3Rent[i] = 550;
                        field4Rent[i] = 750;
                        fieldHRent[i] = 950;
                        fieldSet1[i] = 16;
                        fieldSet2[i] = 19;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 19:
                        fieldName[i] = "New York Avenue";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldOrange3.jpg", UriKind.Relative));
                        fieldPrice[i] = 200;
                        fieldNoSetRent[i] = 16;
                        field1Rent[i] = 80;
                        field2Rent[i] = 220;
                        field3Rent[i] = 600;
                        field4Rent[i] = 800;
                        fieldHRent[i] = 1000;
                        fieldSet1[i] = 16;
                        fieldSet2[i] = 18;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 20:
                        fieldName[i] = "Bezpłatny parking";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldParkingLot.jpg", UriKind.Relative));
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

                    case 21:
                        fieldName[i] = "Kentucky Avenue";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldRed1.jpg", UriKind.Relative));
                        fieldPrice[i] = 220;
                        fieldNoSetRent[i] = 18;
                        field1Rent[i] = 90;
                        field2Rent[i] = 250;
                        field3Rent[i] = 700;
                        field4Rent[i] = 875;
                        fieldHRent[i] = 1050;
                        fieldSet1[i] = 23;
                        fieldSet2[i] = 24;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 22:
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

                    case 23:
                        fieldName[i] = "Indiana Avenue";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldRed2.jpg", UriKind.Relative));
                        fieldPrice[i] = 220;
                        fieldNoSetRent[i] = 18;
                        field1Rent[i] = 90;
                        field2Rent[i] = 250;
                        field3Rent[i] = 700;
                        field4Rent[i] = 875;
                        fieldHRent[i] = 1050;
                        fieldSet1[i] = 21;
                        fieldSet2[i] = 24;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 24:
                        fieldName[i] = "Illinois Avenue";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldRed3.jpg", UriKind.Relative));
                        fieldPrice[i] = 240;
                        fieldNoSetRent[i] = 20;
                        field1Rent[i] = 100;
                        field2Rent[i] = 300;
                        field3Rent[i] = 750;
                        field4Rent[i] = 925;
                        fieldHRent[i] = 1100;
                        fieldSet1[i] = 21;
                        fieldSet2[i] = 23;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 25:
                        fieldName[i] = "Dworzec B. & O.";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldRailroad3.jpg", UriKind.Relative));
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

                    case 26:
                        fieldName[i] = "Atlantic Avenue";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldYellow1.jpg", UriKind.Relative));
                        fieldPrice[i] = 260;
                        fieldNoSetRent[i] = 22;
                        field1Rent[i] = 110;
                        field2Rent[i] = 330;
                        field3Rent[i] = 800;
                        field4Rent[i] = 975;
                        fieldHRent[i] = 1150;
                        fieldSet1[i] = 27;
                        fieldSet2[i] = 29;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 27:
                        fieldName[i] = "Ventnor Avenue";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldYellow2.jpg", UriKind.Relative));
                        fieldPrice[i] = 260;
                        fieldNoSetRent[i] = 22;
                        field1Rent[i] = 110;
                        field2Rent[i] = 330;
                        field3Rent[i] = 800;
                        field4Rent[i] = 975;
                        fieldHRent[i] = 1150;
                        fieldSet1[i] = 26;
                        fieldSet2[i] = 29;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 28:
                        fieldName[i] = "Wodociągi";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldWaterworks.jpg", UriKind.Relative));
                        fieldPrice[i] = 150;
                        fieldNoSetRent[i] = 0;
                        field1Rent[i] = 0;
                        field2Rent[i] = 0;
                        field3Rent[i] = 0;
                        field4Rent[i] = 0;
                        fieldHRent[i] = 0;
                        fieldSet1[i] = 12;
                        fieldSet2[i] = 0;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = true;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 29:
                        fieldName[i] = "Marvin Gardens";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldYellow3.jpg", UriKind.Relative));
                        fieldPrice[i] = 280;
                        fieldNoSetRent[i] = 24;
                        field1Rent[i] = 120;
                        field2Rent[i] = 360;
                        field3Rent[i] = 850;
                        field4Rent[i] = 1025;
                        fieldHRent[i] = 1200;
                        fieldSet1[i] = 26;
                        fieldSet2[i] = 27;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 30:
                        fieldName[i] = "Idź do więzienia!";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldGoToJail.jpg", UriKind.Relative));
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

                    case 31:
                        fieldName[i] = "Pacific Avenue";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldGreen1.jpg", UriKind.Relative));
                        fieldPrice[i] = 300;
                        fieldNoSetRent[i] = 26;
                        field1Rent[i] = 130;
                        field2Rent[i] = 390;
                        field3Rent[i] = 900;
                        field4Rent[i] = 1100;
                        fieldHRent[i] = 1275;
                        fieldSet1[i] = 32;
                        fieldSet2[i] = 34;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 32:
                        fieldName[i] = "North Carolina Avenue";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldGreen2.jpg", UriKind.Relative));
                        fieldPrice[i] = 300;
                        fieldNoSetRent[i] = 26;
                        field1Rent[i] = 130;
                        field2Rent[i] = 390;
                        field3Rent[i] = 900;
                        field4Rent[i] = 1100;
                        fieldHRent[i] = 1275;
                        fieldSet1[i] = 31;
                        fieldSet2[i] = 34;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 33:
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

                    case 34:
                        fieldName[i] = "Pennsylvania Avenue";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldGreen3.jpg", UriKind.Relative));
                        fieldPrice[i] = 320;
                        fieldNoSetRent[i] = 28;
                        field1Rent[i] = 150;
                        field2Rent[i] = 450;
                        field3Rent[i] = 1000;
                        field4Rent[i] = 1200;
                        fieldHRent[i] = 1400;
                        fieldSet1[i] = 31;
                        fieldSet2[i] = 32;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 35:
                        fieldName[i] = "Dworzec ShortLine";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldRailroad4.jpg", UriKind.Relative));
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

                    case 36:
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

                    case 37:
                        fieldName[i] = "Park Place";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldBlue1.jpg", UriKind.Relative));
                        fieldPrice[i] = 350;
                        fieldNoSetRent[i] = 35;
                        field1Rent[i] = 175;
                        field2Rent[i] = 500;
                        field3Rent[i] = 1100;
                        field4Rent[i] = 1300;
                        fieldHRent[i] = 1500;
                        fieldSet1[i] = 39;
                        fieldSet2[i] = 0;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
                        fieldExtra[i] = false;
                        fieldTax[i] = false;
                        fieldTaxCost[i] = 0;
                        break;

                    case 38:
                        fieldName[i] = "Podatek luksusowy";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldTax2.jpg", UriKind.Relative));
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
                        fieldExtra[i] = false;
                        fieldTax[i] = true;
                        fieldTaxCost[i] = 100;
                        break;

                    case 39:
                        fieldName[i] = "Boardwalk";
                        fieldIcon[i] = new BitmapImage(new Uri(@"\Resources\FieldBlue2.jpg", UriKind.Relative));
                        fieldPrice[i] = 400;
                        fieldNoSetRent[i] = 50;
                        field1Rent[i] = 200;
                        field2Rent[i] = 600;
                        field3Rent[i] = 1400;
                        field4Rent[i] = 1700;
                        fieldHRent[i] = 2000;
                        fieldSet1[i] = 37;
                        fieldSet2[i] = 0;
                        fieldChance[i] = false;
                        fieldCommChest[i] = false;
                        fieldRailroad[i] = false;
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
