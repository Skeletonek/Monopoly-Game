using System;
using System.Windows;

namespace Monopoly
{
    /// <summary>
    /// Logika interakcji dla klasy CheatWindow.xaml
    /// </summary>
    public partial class CheatWindow : Window
    {
        public CheatWindow()
        {
            InitializeComponent();
        }

        private void ButtonText_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.cheat = Convert.ToInt32(TextBoxCheat.Text);
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.cheat_allowTradeWindow = true;
            this.Close();
        }

        private void ButtonText_Click_Money(object sender, RoutedEventArgs e)
        {
            Game.playercash[Game.clientplayer] += Convert.ToInt32(TextBoxCheat_Money.Text);
            this.Close();
        }
    }
}
