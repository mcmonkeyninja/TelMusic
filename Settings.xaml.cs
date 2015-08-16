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

namespace TelMusic
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {

            this.Owner = App.Current.MainWindow;
            InitializeComponent();

            customSize.IsChecked = Properties.Settings.Default.AllowCustomWindowSize;
            importInvalidSongs.IsChecked = Properties.Settings.Default.AllowImproperSongRegistry;
            pauseToScrub.IsChecked = Properties.Settings.Default.PauseToScrub;
            ColorSelection.SelectedIndex = Properties.Settings.Default.ColorSchemeIndex;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                colorRect.Fill = (ColorSelection.SelectedItem as Label).Background;
                Properties.Settings.Default.ColorSchemeIndex = ColorSelection.SelectedIndex;
                Properties.Settings.Default.ColorScheme = colorRect.Fill.ToString();
                Properties.Settings.Default.Save();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void windowsSizeChecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AllowCustomWindowSize = customSize.IsChecked.Value;
        }

        private void invalidSongChecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AllowImproperSongRegistry = importInvalidSongs.IsChecked.Value;
        }

        private void pauseToScrubChecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.PauseToScrub = pauseToScrub.IsChecked.Value;
        }
    }
}
