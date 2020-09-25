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
    }
}
