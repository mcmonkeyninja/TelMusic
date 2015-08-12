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
    /// Interaction logic for NewPlaylist.xaml
    /// </summary>
    public partial class NewPlaylist : Window
    {
        public string selectedOption;
        public string playlistName;

        public NewPlaylist()
        {
            InitializeComponent();
            label1.Visibility = Visibility.Collapsed;
            textBox.Visibility = Visibility.Collapsed;
        }

        private void existingPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            selectedOption = "EXISTING";
            existingPlaylistButton.Visibility = Visibility.Collapsed;
            label1.Visibility = Visibility.Visible;
            textBox.Visibility = Visibility.Visible;
        }

        private void newPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            selectedOption = "NEW";
            existingPlaylistButton.Visibility = Visibility.Collapsed;
            label1.Visibility = Visibility.Visible;
            textBox.Visibility = Visibility.Visible;
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(textBox.Text) && !string.IsNullOrWhiteSpace(textBox.Text))
                {
                    playlistName = textBox.Text;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Please enter a valid name");
                }
            }
        }
    }
}
