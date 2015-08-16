using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TelMusic
{
    public partial class MainWindow : Window
    {

        bool SHOW_TASK_COMP_TIMER = false;

        string[] COMMAND_LINE_ARGS;

        List<string> LoadedFiles;

        TagLib.File[] CachedFiles;
        List<muComp> LoadedSongs;

        private TimeSpan TotalTime;

        int currentQueIndex = 0;
        List<muComp> SongQue;

        bool IsPlaylistLoaded;
        string PlaylistName;
        string PlaylistDescription;

        DispatcherTimer timerVideoTime;
        DispatcherTimer dropDownAnimation;

        muComp currentlyPlayingComp;
        TagLib.File currentlyPlayingTagLib;

        

        //Debug
        Stopwatch stopwatch;

        public MainWindow()
        {
            stopwatch = new Stopwatch();

            stopwatch.Start();
            InitializeComponent();
            COMMAND_LINE_ARGS = Environment.GetCommandLineArgs();
            var converter = new System.Windows.Media.BrushConverter();
            var brush = (Brush)converter.ConvertFromString(Properties.Settings.Default.ColorScheme);

            rect1.Fill = brush;
            rect2.Fill = brush;

            if (Properties.Settings.Default.AllowCustomWindowSize)
            {
                this.ResizeMode = System.Windows.ResizeMode.CanResize;
            }
            else
            {
                this.ResizeMode = System.Windows.ResizeMode.NoResize;
            }
            volumeSlider.Visibility = System.Windows.Visibility.Collapsed;
            volumeRect.Visibility = System.Windows.Visibility.Collapsed;

            LoadedSongs = new List<muComp>();
            SongQue = new List<muComp>();

            PlayButton.Content = FindResource("Play");

            if (COMMAND_LINE_ARGS.Length > 0)
            {
                if (COMMAND_LINE_ARGS[0] != null)
                {
                    
                }
            }
            stopwatch.Stop();
            if (SHOW_TASK_COMP_TIMER)
                Console.WriteLine("Initialization took {0}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void artistSelectionchanged(object sender, SelectionChangedEventArgs e)
        {
            stopwatch.Start();
            if (tierArtist.SelectedItem != null)
            {
                string selectedArtist = (tierArtist.SelectedItem as ListViewItem).Content.ToString();

                Console.WriteLine(selectedArtist);

                tierAlbum.Items.Clear();
                tierSongs.Items.Clear();

                List<muComp> foundMatches = LoadedSongs.FindAll(new Predicate<muComp>((m) => { if (m.artist == selectedArtist) return true; else return false; }));

                List<string> addedItems = new List<string>();
                foreach (muComp s in foundMatches)
                {
                    ListViewItem album = new ListViewItem();

                    album.Content = s.album;

                    if (!addedItems.Contains(album.Content.ToString()))
                        addedItems.Add(album.Content.ToString());
                }

                foreach (string l in addedItems)
                {
                    ListViewItem lvi = new ListViewItem();

                    lvi.Content = l;

                    tierAlbum.Items.Add(lvi);
                }

                tierAlbum.SelectedIndex = 0;
            }
            stopwatch.Stop();
            if (SHOW_TASK_COMP_TIMER)
                Console.WriteLine("Album generation took {0}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
        }

        private void albumSelectionchanged(object sender, SelectionChangedEventArgs e)
        {
            stopwatch.Start();
            if (tierAlbum.SelectedItem != null)
            {
                string selectedAlbum = (tierAlbum.SelectedItem as ListViewItem).Content.ToString();
                Console.WriteLine(selectedAlbum);

                tierSongs.Items.Clear();

                List<muComp> foundMatches = LoadedSongs.FindAll(new Predicate<muComp>((m) => { if (m.album == selectedAlbum) return true; else return false; }));

                List<string> addedItems = new List<string>();
                List<muComp> addedComps = new List<muComp>();
                foreach (muComp s in foundMatches)
                {
                    ListViewItem song = new ListViewItem();

                    song.Content = s.title;

                    if (!addedItems.Contains(song.Content.ToString()))
                    {
                        addedItems.Add(song.Content.ToString());
                        addedComps.Add(s);
                    }
                }

                for (int i = 0; i < addedItems.Count; i++)
                {
                    ListViewItem lvi = new ListViewItem();

                    lvi.Content = addedItems[i];
                    lvi.Tag = (object)addedComps[i];

                    tierSongs.Items.Add(lvi);
                }
            }

            stopwatch.Stop();
            if (SHOW_TASK_COMP_TIMER)
                Console.WriteLine("Song generation took {0}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
        }
        private void songSelectionchanged(object sender, SelectionChangedEventArgs e)
        {
            stopwatch.Start();
            if (tierSongs.SelectedItem != null)
            {


                currentlyPlayingComp = ((muComp)(tierSongs.SelectedItem as ListViewItem).Tag);
                currentlyPlayingTagLib = TagLib.File.Create(((muComp)(tierSongs.SelectedItem as ListViewItem).Tag).Tags[0].ToString());
                currentlyPlayingComp.Tags[1] = tierSongs.SelectedIndex;
                currentlyPlayingComp.Tags[2] = tierAlbum.SelectedIndex;
                currentlyPlayingComp.Tags[3] = tierArtist.SelectedIndex;

                if (!SongQue.Contains(currentlyPlayingComp))
                {
                    SongQue.Insert(0, currentlyPlayingComp);
                }

                PlayButton.Content = FindResource("Pause");



                SelectedAlbum.Content = "Album: " + currentlyPlayingTagLib.Tag.Album;

                SelectedArtist.Content = "Artist: " + currentlyPlayingComp.artist;

                if (currentlyPlayingTagLib.Tag.AlbumArtists.Length > 0)
                {
                    SelectedArtist.Content = "Artist: " + currentlyPlayingTagLib.Tag.AlbumArtists[0];
                }
                else if (currentlyPlayingTagLib.Tag.Performers.Length > 0)
                {
                    SelectedArtist.Content = "Artist: " + currentlyPlayingTagLib.Tag.Performers[0];
                }

                SelectedYear.Content = "Year: " + currentlyPlayingTagLib.Tag.Year;
                SelectedGenre.Content = "Genre: ";
                for (int i = 0; i < currentlyPlayingTagLib.Tag.Genres.Length; i++)
                {
                    if (i < currentlyPlayingTagLib.Tag.Genres.Length - 1)
                    {
                        SelectedGenre.Content += currentlyPlayingTagLib.Tag.Genres[i] + ", ";
                    }
                    else
                    {
                        SelectedGenre.Content += currentlyPlayingTagLib.Tag.Genres[i];
                    }
                }
                mediaElement1.MediaOpened += PlayCurrentSong;

                try
                {
                    selectionImage.Source = ToImage(currentlyPlayingTagLib.Tag.Pictures[0].Data.ToArray());
                }
                catch (IndexOutOfRangeException ie)
                {
                    Console.WriteLine(ie.Message);

                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri("pack://siteoforigin:,,,/Resources/blankMusic.jpg");
                    image.EndInit();

                    selectionImage.Source = image;
                }
            }
            stopwatch.Stop();
            if (SHOW_TASK_COMP_TIMER)
                Console.WriteLine("Song data generation took {0}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
        }

        void mediaElement1_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (tierSongs.SelectedIndex < tierSongs.Items.Count)
            {
                tierSongs.SelectedIndex++;

            }
            else
            {
                tierSongs.SelectedIndex = 0;
            }

            Console.WriteLine("Selected: {0} Count: {1}", tierSongs.SelectedIndex, tierSongs.Items.Count);
        }

        private void PlayCurrentSong(object sender, RoutedEventArgs e)
        {
            TotalTime = mediaElement1.NaturalDuration.TimeSpan;

            // Create a timer that will update the counters and the time slider
            timerVideoTime = new DispatcherTimer();
            timerVideoTime.Interval = TimeSpan.FromSeconds(1);
            timerVideoTime.Tick += new EventHandler(timer_Tick);
            timerVideoTime.Start();


            slider.AddHandler(MouseLeftButtonUpEvent,
                  new MouseButtonEventHandler(timeSlider_MouseLeftButtonUp),
                  true);
        }

        private void timeSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (TotalTime.TotalSeconds > 0)
            {
                mediaElement1.Position = TimeSpan.FromSeconds((slider.Value * TotalTime.TotalSeconds) / 10);
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            // Check if the movie finished calculate it's total time
            if (mediaElement1.NaturalDuration.HasTimeSpan)
            {
                if (mediaElement1.NaturalDuration.TimeSpan.TotalSeconds > 0)
                {
                    if (TotalTime.TotalSeconds > 0)
                    {
                        // Updating time slider
                        slider.Value = (mediaElement1.Position.TotalSeconds /
                                           TotalTime.TotalSeconds) * 10;

                        if ((mediaElement1.NaturalDuration - mediaElement1.Position).ToString().Split('.').Length > 0)
                        {
                            remainingTime.Content = (mediaElement1.NaturalDuration - mediaElement1.Position).ToString().Split('.')[0].Split(':')[1] + ":" + (mediaElement1.NaturalDuration - mediaElement1.Position).ToString().Split('.')[0].Split(':')[2];
                        }
                        else
                        {
                            remainingTime.Content = (mediaElement1.NaturalDuration - mediaElement1.Position).ToString().Split('.')[0];
                        }

                        //remainingTime.Content = mediaElement1.Position.;
                    }
                }
            }
        }

        public BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        public System.Drawing.Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);
            return returnImage;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            AddMusicWindow adw = new AddMusicWindow();
            adw.ShowDialog();

            LoadedFiles = adw.LoadedFiles;

            if (!string.IsNullOrEmpty(adw.PlaylistName))
            {
                IsPlaylistLoaded = true;
                PlaylistName = adw.PlaylistName;
                PlaylistDescription = adw.PlaylistDescrition;
            }

            ReloadSongs();

        }

        public TagLib.File[] Recache(int size)
        {
            TagLib.File[] result = new TagLib.File[size];
            for (int i = 0; i < CachedFiles.Length; i++)
            {
                TagLib.File file = TagLib.File.Create(LoadedFiles[i]);
                result[i] = file;
            }
            return result;
        }

        public void ReloadSongs()
        {
            stopwatch.Start();
            try
            {
                tierSongs.Items.Clear();
                tierAlbum.Items.Clear();
                tierArtist.Items.Clear();
                CachedFiles = new TagLib.File[LoadedFiles.Count];
                for (int i = 0; i < CachedFiles.Length; i++)
                {
                    TagLib.File file = TagLib.File.Create(LoadedFiles[i]);
                    CachedFiles[i] = file;

                    if (file.Tag.Album != null && (file.Tag.AlbumArtists.Length > 0 || file.Tag.Performers.Length > 0) && file.Tag.Title != null)
                    {
                        muComp comp = new muComp();
                        string album = "err";
                        string artist = "err";
                        string title = "err";

                        //if (LoadedSongs.Exists(new Predicate<muComp>((m) => { if (m.album == file.Tag.Album) return true; else return false; })))
                        title = file.Tag.Title;
                        //if (!LoadedSongs.Exists(new Predicate<muComp>((m) => { if (m.album == file.Tag.Album) return true; else return false; })))
                        album = file.Tag.Album;

                        if (file.Tag.AlbumArtists.Length > 0)
                        {
                            //if (!LoadedSongs.Exists(new Predicate<muComp>((m) => { if (m.artist == file.Tag.AlbumArtists[0]) return true; else return false; })))
                            artist = file.Tag.AlbumArtists[0];
                        }
                        else if (file.Tag.Performers.Length > 0)
                        {
                            //if (!LoadedSongs.Exists(new Predicate<muComp>((m) => { if (m.artist == file.Tag.Performers[0]) return true; else return false; })))
                            artist = file.Tag.Performers[0];
                        }

                        if (artist != "err" && album != "err" && title != "err")
                        {
                            comp = (muComp)(artist + "%" + album + "%" + title);
                            comp.Tags[0] = LoadedFiles[i];
                            //LoadedArtist.Add(comp);
                            //LoadedAlbums.Add(comp);
                            LoadedSongs.Add(comp);
                        }

                        Console.WriteLine("   Loaded {0}", LoadedFiles[i]);
                    }
                    else
                    {
                        if (Properties.Settings.Default.AllowImproperSongRegistry)
                        {
                            muComp comp = new muComp();
                            string album = "Default";
                            string artist = "Other";
                            string title = LoadedFiles[i].Split('\\')[LoadedFiles[i].Split('\\').Length - 1].Split('.')[0];

                            comp = (muComp)(artist + "%" + album + "%" + title);
                            comp.Tags[0] = LoadedFiles[i];
                            LoadedSongs.Add(comp);


                            Console.WriteLine("   Loaded invalid {0}", LoadedFiles[i]);
                        }
                        else
                        {
                            Console.WriteLine("   Skipped {0}", LoadedFiles[i]);
                        }
                    }
                }

                List<string> addedItems = new List<string>();
                foreach (muComp s in LoadedSongs)
                {
                    ListViewItem artist = new ListViewItem();

                    artist.Content = s.artist;

                    if (!addedItems.Contains(artist.Content.ToString()))
                        addedItems.Add(artist.Content.ToString());

                    Console.WriteLine("Sucessfully added {0}", s.artist);

                }

                foreach (string l in addedItems)
                {
                    ListViewItem lvi = new ListViewItem();

                    lvi.Content = l;

                    tierArtist.Items.Add(lvi);
                }

                Console.WriteLine("Enumerated artists");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

            stopwatch.Stop();
            if (SHOW_TASK_COMP_TIMER)
                Console.WriteLine("Reloading songs took {0}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
        }

        private void selectionImage_SourceUpdated(object sender, DataTransferEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            settings.ShowDialog();
            var converter = new System.Windows.Media.BrushConverter();
            var brush = (Brush)converter.ConvertFromString(Properties.Settings.Default.ColorScheme);

            rect1.Fill = brush;
            rect2.Fill = brush;

            mediaElement1.ScrubbingEnabled = Properties.Settings.Default.PauseToScrub;

            if (Properties.Settings.Default.AllowCustomWindowSize)
            {
                this.ResizeMode = System.Windows.ResizeMode.CanResize;
            }
            else
            {
                this.ResizeMode = System.Windows.ResizeMode.NoResize;
            }
        }

        private MediaState GetMediaState(MediaElement myMedia)
        {
            FieldInfo hlp = typeof(MediaElement).GetField("_helper", BindingFlags.NonPublic | BindingFlags.Instance);
            object helperObject = hlp.GetValue(myMedia);
            FieldInfo stateField = helperObject.GetType().GetField("_currentState", BindingFlags.NonPublic | BindingFlags.Instance);
            MediaState state = (MediaState)stateField.GetValue(helperObject);
            return state;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GetMediaState(mediaElement1) == MediaState.Pause)
                {
                    mediaElement1.Play();
                    PlayButton.Content = FindResource("Pause");
                }
                else if (GetMediaState(mediaElement1) == MediaState.Play)
                {
                    mediaElement1.Pause();
                    PlayButton.Content = FindResource("Play");
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message);
                Console.WriteLine(ee.StackTrace);
            }
        }

        bool showVolume = false;
        private void Volume_Click(object sender, RoutedEventArgs e)
        {
            showVolume = !showVolume;

            if (showVolume)
            {
                volumeSlider.Visibility = System.Windows.Visibility.Visible;
                volumeRect.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                volumeSlider.Visibility = System.Windows.Visibility.Collapsed;
                volumeRect.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement1.Volume = (float)(volumeSlider.Value / 10);
        }

        private void RewindButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentQueIndex < SongQue.Count)
            {
                currentQueIndex++;
            }

            if (currentQueIndex == SongQue.Count)
            {
                currentQueIndex = SongQue.Count - 1;
            }

            tierArtist.SelectedIndex = (int)SongQue[currentQueIndex].Tags[3];
            tierAlbum.SelectedIndex = (int)SongQue[currentQueIndex].Tags[2];
            tierSongs.SelectedIndex = (int)SongQue[currentQueIndex].Tags[1];
        }

        private void FastForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentQueIndex > 0)
            {
                currentQueIndex--;
            }

            tierArtist.SelectedIndex = (int)SongQue[currentQueIndex].Tags[3];
            tierAlbum.SelectedIndex = (int)SongQue[currentQueIndex].Tags[2];
            tierSongs.SelectedIndex = (int)SongQue[currentQueIndex].Tags[1];
        }

        private void tierSongs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            stopwatch.Start();
            if (tierSongs.SelectedItem != null)
            {
                mediaElement1.LoadedBehavior = MediaState.Manual;
                mediaElement1.Source = new Uri(((muComp)(tierSongs.SelectedItem as ListViewItem).Tag).Tags[0].ToString(), UriKind.Absolute);
                mediaElement1.Play();

                mediaElement1.MediaEnded += mediaElement1_MediaEnded;

                mediaElement1.SpeedRatio = 1;

                SongName.Content = "There was an error reading the song's name";

                try
                {
                    AlbumArtImage.Source = ToImage(currentlyPlayingTagLib.Tag.Pictures[0].Data.ToArray());
                }
                catch (IndexOutOfRangeException ie)
                {
                    Console.WriteLine(ie.Message);

                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri("pack://siteoforigin:,,,/Resources/blankMusic.jpg");
                    image.EndInit();

                    AlbumArtImage.Source = image;
                }

                if (string.IsNullOrEmpty(currentlyPlayingTagLib.Tag.Title))
                {
                    SongName.Content += " Error code 0";
                    if (string.IsNullOrEmpty(currentlyPlayingComp.title))
                    {
                        SongName.Content += " Error code 2";
                        if (string.IsNullOrEmpty((tierSongs.SelectedItem as ListViewItem).Content.ToString()))
                        {
                            SongName.Content += " Error code: 3";
                        }
                        else
                        {
                            SongName.Content = (tierSongs.SelectedItem as ListViewItem).Content;
                        }
                    }
                    else
                    {
                        SongName.Content = currentlyPlayingComp.title;
                    }
                }
                else
                {
                    SongName.Content = currentlyPlayingTagLib.Tag.Title;
                }
            }
            stopwatch.Stop();
            if (SHOW_TASK_COMP_TIMER)
                Console.WriteLine("Took {0}ms to start song", stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
        }

        private void savePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            stopwatch.Start();
            NewPlaylist np = new NewPlaylist();
            np.ShowDialog();

            if (np.selectedOption == "EXISTING")
            {

            NoDirectoryFound:
                if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\My Playlists\\"))
                {
                    string[] NewLines = new string[tierSongs.SelectedItems.Count];

                    for (int i = 0; i < NewLines.Length; i++)
                    {
                        NewLines[i] = "s|" + ((muComp)(tierSongs.SelectedItems[i] as ListViewItem).Tag).Tags[0].ToString();
                        Console.WriteLine(NewLines[i]);
                    }

                    File.AppendAllLines(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\My Playlists\\" + np.playlistName + ".playlist", NewLines);
                }
                else
                {
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\My Playlists\\");
                    goto NoDirectoryFound;
                }

            }
            else if (np.selectedOption == "NEW")
            {
            NoDirectoryFound:
                if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\My Playlists\\"))
                {
                    string[] NewLines = new string[tierSongs.SelectedItems.Count];

                    for (int i = 0; i < NewLines.Length; i++)
                    {
                        if (i == 0)
                        {
                            NewLines[i] = "Name|" + np.playlistName;
                        }
                        else if (i == 1)
                        {
                            NewLines[i] = "Description|" + " no desc";
                        }
                        else
                        {
                            NewLines[i] = "s|" + ((muComp)(tierSongs.SelectedItems[i] as ListViewItem).Tag).Tags[0].ToString();
                            Console.WriteLine(NewLines[i]);
                        }
                    }

                    File.WriteAllLines(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\My Playlists\\" + np.playlistName + ".playlist", NewLines);
                }
                else
                {
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\My Playlists\\");
                    goto NoDirectoryFound;
                }
            }
            stopwatch.Stop();
            if (SHOW_TASK_COMP_TIMER)
                Console.WriteLine("Playlist generation took {0}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
        }

        private void slider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Properties.Settings.Default.PauseToScrub)
            {
                mediaElement1.Pause();
            }
        }

        private void showPlaylistExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            dropDownAnimation = new DispatcherTimer();
            dropDownAnimation.Interval = TimeSpan.FromTicks(1);
            dropDownAnimation.Tick += new EventHandler(dropDownTickUP);
            dropDownAnimation.Start();
            playlists.Items.Clear();
        }

        private void showPlaylistExpander_Expanded(object sender, RoutedEventArgs e)
        {
            dropDownAnimation = new DispatcherTimer();
            dropDownAnimation.Interval = TimeSpan.FromMilliseconds(0.1);
            dropDownAnimation.Tick += new EventHandler(dropDownTickDOWN);
            dropDownAnimation.Start();

            string[] playlistFiles = Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\My Playlists\\").ToArray();

            foreach (string s in playlistFiles)
            {
                ListViewItem item = new ListViewItem();
                item.Content = s.Split('\\')[s.Split('\\').Length - 1].Split('.')[0];
                item.Tag = s;
                playlists.Items.Add(item);
            }
            Console.WriteLine("Done enumerationg playlists");
        }

        private void dropDownTickUP(object sender, EventArgs e)
        {
            if (this.Height > 640)
            {
                this.Height-=20;
            }
            else
            {
                dropDownAnimation.Stop();
            }
            Console.WriteLine("RISING " + this.Height);
        }

        private void dropDownTickDOWN(object sender, EventArgs e)
        {
            if (this.Height < 640 + 128)
            {
                this.Height+=20;
            }
            else
            {
                dropDownAnimation.Stop();
            }
            Console.WriteLine("LOWERING " + this.Height);
        }

        private void playlists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tierSongs.Items.Clear();
            tierAlbum.Items.Clear();
            tierArtist.Items.Clear();

            Console.WriteLine("Playlist index changed to " + playlists.SelectedIndex);
            string[] Lines = System.IO.File.ReadAllLines(((ListViewItem)playlists.SelectedItem).Tag.ToString());
            LoadedFiles = new List<string>();
            LoadedFiles.Clear();
            foreach (string s in Lines)
            {
                string value = s.Split('|')[1];

                switch (s.Split('|')[0])
                {
                    case "Name":
                        PlaylistName = value;
                        break;
                    case "Description":
                        PlaylistDescription = value;
                        break;
                    case "s":
                        LoadedFiles.Add(value);
                        break;
                    default:
                        Console.WriteLine("Invalid tag found in playlist file");
                        break;

                }
            }
            
            ReloadSongs();
        }
    }

    public struct muComp
    {
        public string artist;
        public string album;
        public string title;

        public object[] Tags;

        public static explicit operator muComp(string input)
        {
            //try
            //{
            muComp result = new muComp();
            result.artist = input.Split('%')[0];
            result.album = input.Split('%')[1];
            result.title = input.Split('%')[2];
            result.Tags = new object[32];
            return result;
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //    Console.WriteLine(e.StackTrace);
            //}

            //return "error:error";
        }

        public static implicit operator string(muComp input)
        {
            return input.artist + "%" + input.album;
        }
    }
}
