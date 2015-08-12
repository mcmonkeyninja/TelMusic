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
    /// Interaction logic for AddMusicWindow.xaml
    /// </summary>
    public partial class AddMusicWindow : Window
    {
        public List<string> LoadedFiles;
        public string PlaylistName;
        public string PlaylistDescrition;

        public AddMusicWindow()
        {
            InitializeComponent();
        }

        private void openFiles_ButtonClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            openFileDialog.Filter = "Music Files|*.mp3|All Files (*.*)|*.*";


            openFileDialog.Multiselect = true;

            System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();



            // Process input if the user clicked OK.
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                LoadedFiles = new List<string>(openFileDialog.FileNames);

                // Open the selected file to read.
                //System.IO.Stream fileStream = openFileDialog.OpenFile();

                //Can read file here

                //fileStream.Close();
            }
            this.Close();
        }

        private void openPlaylist_ButtonClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            openFileDialog.Filter = "Playlist Files|*.playlist|All Files (*.*)|*.*";

            openFileDialog.Multiselect = false;

            System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();

            // Process input if the user clicked OK.
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Open the selected file to read.
                System.IO.Stream fileStream = openFileDialog.OpenFile();

                //Can read file here
                string[] Lines = System.IO.File.ReadAllLines(openFileDialog.FileName);

                foreach (string s in Lines)
                {
                    string value = s.Split(':')[1];

                    switch (s.Split(':')[0])
                    {
                        case "Name":
                            PlaylistName = value;
                            break;
                        case "Description":
                            PlaylistDescrition = value;
                            break;
                        case "s":
                            LoadedFiles.Add(value);
                            break;
                        default:
                            Console.WriteLine("Invalid tag found in playlist file");
                            break;

                    }
                }


                fileStream.Close();
            }

            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("I said I wasn't implemented you bastard!");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button_Click(sender, e);
        }

    }
}
