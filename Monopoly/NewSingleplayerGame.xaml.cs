using System.Windows;
using System.Collections.Generic;

namespace Monopoly
{
    /// <summary>
    /// Logika interakcji dla klasy NewSingleplayerGame.xaml
    /// </summary>
    public partial class NewSingleplayerGame : Window
    {
        static string currentTheme;
        public NewSingleplayerGame(List<string> ThemeBoards)
        {
            InitializeComponent();
            foreach (string x in ThemeBoards)
            {
                string[] splitedText = x.Split(';');
                ListBox_PlayboardTheme.Items.Add(splitedText[0]);
            }
        }

        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ListBox_PlayboardTheme_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            currentTheme = (string)ListBox_PlayboardTheme.SelectedItem;
        }
    }
}
