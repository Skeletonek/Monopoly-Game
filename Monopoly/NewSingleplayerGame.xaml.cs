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
        public NewSingleplayerGame(List<string> ThemeBoards, bool hotseat)
        {
            InitializeComponent();
            foreach (string x in ThemeBoards)
            {
                string[] splitedText = x.Split(';');
                ListBox_PlayboardTheme.Items.Add(splitedText[0]);
            }
            if(!hotseat)
            {
                CheckBox_AI1.IsChecked = true;
                CheckBox_AI1.IsEnabled = false;
                CheckBox_AI2.IsChecked = true;
                CheckBox_AI2.IsEnabled = false;
            }
        }

        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NewSingleplayerGame_ClosedByGameStart = true;
            this.Close();
        }

        private void ListBox_PlayboardTheme_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            currentTheme = (string)ListBox_PlayboardTheme.SelectedItem;
        }
    }
}
