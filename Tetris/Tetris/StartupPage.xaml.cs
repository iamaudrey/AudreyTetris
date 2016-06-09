/*
StartupPage.xaml.cs
Audrey Henry
This is the logic behind the main menu of the game.
*/
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Tetris
{
    /// <summary>
    /// Interaction logic for StartupPage.xaml
    /// </summary>
    public partial class StartupPage : Page
    {
        //Contains a reference to the media player
        private MediaPlayer player;
        public StartupPage()
        {
            InitializeComponent();
            //Determine whether or not to enable the LoadGame option
            if(!File.Exists("savegame"))
            {
                Label_LoadGame.IsEnabled = false;
                MenuItem_File_LoadGame.IsEnabled = false;
                Label_LoadGame.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B3B3B"));
            }
            //Attempt to initialize the Media Player object
            player = new MediaPlayer();
            try
            {
                player.Open(new Uri("..\\..\\Content\\moon_theme.mp3", UriKind.Relative));
                player.Volume = .25;
                player.MediaEnded += new EventHandler(SongEnded);
                player.Play();
            }
            catch (Exception)
            {
            }
        }
        //Constructor with existing mediaplayer
        public StartupPage(MediaPlayer newPlayer)
        {
            InitializeComponent();
            if (!File.Exists("savegame"))
            {
                Label_LoadGame.IsEnabled = false;
                MenuItem_File_LoadGame.IsEnabled = false;
                Label_LoadGame.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B3B3B"));
            }
            if (newPlayer != null)
            {
                player = newPlayer;
                MenuItem_Options_Music.IsChecked = !player.IsMuted;
            }
            else
            {
                player = new MediaPlayer();
                try
                {
                    player.Open(new Uri("..\\..\\Content\\moon_theme.mp3", UriKind.Relative));
                    player.Volume = .25;
                    player.MediaEnded += new EventHandler(SongEnded);
                    player.Play();
                }
                catch (Exception)
                {
                }
            } 
        }

        //Make sure that the song repeats!!! 
        public void SongEnded(object sender, EventArgs e)
        {
            player.Position = TimeSpan.Zero;
            player.Play();
        }

        //Quit menu option
        private void Label_exitLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        //Fancy effects for menu items
        private void Label_exitLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            Label_exitLabel.Foreground = new SolidColorBrush(Color.FromRgb(68, 131, 68));
        }

        private void Label_exitLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            Label_exitLabel.Foreground = new SolidColorBrush(Color.FromRgb(24, 64, 24));
        }

        private void Label_newGameLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            Label_newGameLabel.Foreground = new SolidColorBrush(Color.FromRgb(68, 131, 68));
        }

        private void Label_newGameLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            Label_newGameLabel.Foreground = new SolidColorBrush(Color.FromRgb(24, 64, 24));
        }

        private void Label_newGameLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.NavigationService.Navigate(new GamePage(new GameState(), player));
        }

        private void Label_LoadGame_MouseEnter(object sender, MouseEventArgs e)
        {
            if(File.Exists("savegame"))
            {
                Label_LoadGame.Foreground = new SolidColorBrush(Color.FromRgb(68, 131, 68));
            }      
        }

        private void Label_LoadGame_MouseLeave(object sender, MouseEventArgs e)
        {
            Label_LoadGame.Foreground = new SolidColorBrush(Color.FromRgb(24, 64, 24));
        }
        
        //This method loads the game from a save file
        private void Label_LoadGame_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(File.Exists("savegame"))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream("savegame", FileMode.Open, FileAccess.Read, FileShare.None);
                GameState result = (GameState)formatter.Deserialize(stream);
                stream.Close();
                foreach(Cell c in result.Grid)
                {
                    c.Rect = new Rectangle();
                    c.Rect.Stroke = Brushes.AntiqueWhite;
                    c.Rect.StrokeThickness = 2;
                    c.Rect.Height = 24;
                    c.Rect.Width = 24;
                    c.Rect.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(c.Color));
                }
                result.Factory = new BlockFactory();
                NavigationService.Navigate(new GamePage(result, player));
            }
        }

        private void MenuItem_File_NewGame_Click(object sender, RoutedEventArgs e)
        {
            Label_newGameLabel_MouseLeftButtonUp(sender, null);
        }

        private void MenuItem_File_Quit_Click(object sender, RoutedEventArgs e)
        {
            Label_exitLabel_MouseLeftButtonUp(sender, null);
        }

        private void MenuItem_File_LoadGame_Click(object sender, RoutedEventArgs e)
        {
            Label_LoadGame_MouseLeftButtonUp(sender, null);
        }

        //This method toggles the music mute
        private void MenuItem_Options_Music_Click(object sender, RoutedEventArgs e)
        {
            if(player != null)
            {
                if (MenuItem_Options_Music.IsChecked)
                {
                    player.IsMuted = false;
                }
                else
                {
                    player.IsMuted = true;
                }
            }
        }

        //This method launches the how to play window
        private void MenuItem_Help_HowToPlay_Click(object sender, RoutedEventArgs e)
        {
            HowToPlayWindow howtoplay = new HowToPlayWindow();
            howtoplay.ShowDialog();
        }

        //This method launches the about window
        private void MenuItem_Help_About_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.ShowDialog();
        }
    }
}
