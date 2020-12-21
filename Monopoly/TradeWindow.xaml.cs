using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Monopoly
{
    /// <summary>
    /// Logika interakcji dla klasy TradeWindow.xaml
    /// </summary>
    public partial class TradeWindow : Window
    {
        MainWindow.Game game = new MainWindow.Game();
        static byte[][] OwnedFields = new byte[4][];
        public TradeWindow()
        {
            InitializeComponent();
        }
        private void checkFieldOwners()
        {
            for(int i = 0; i < game.playerAvailable.Length; i++)
            {
                for(int j = 0; j < game.fieldOwner.Length; j++)
                {

                }
            }
        }
    }
}
