using System.Windows;

namespace Monopoly
{
    /// <summary>
    /// Logika interakcji dla klasy NewSingleplayerGame.xaml
    /// </summary>
    public partial class NewSingleplayerGame : Window
    {
        public NewSingleplayerGame()
        {
            InitializeComponent();
        }

        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
