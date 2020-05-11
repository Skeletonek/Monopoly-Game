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
using EO.WebBrowser;
using EO.Base;
using EO.WebEngine;
using EO.Wpf;

namespace Monopoly
{
    /// <summary>
    /// Logika interakcji dla klasy Spotify.xaml
    /// </summary>
    public partial class Spotify : Window
    {
        public Spotify()
        {
            InitializeComponent();
            EO.WebEngine.BrowserOptions options = new EO.WebEngine.BrowserOptions();
            options.EnableWebSecurity = false;
            options.AllowJavaScript = true;
            options.AllowPlugins = true;
            EO.WebEngine.EngineOptions.Default.SetDefaultBrowserOptions(options);
        }
    }
}
