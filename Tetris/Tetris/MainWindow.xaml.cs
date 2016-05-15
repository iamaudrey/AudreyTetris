using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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

namespace Tetris
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : NavigationWindow
    {
        private MediaPlayer player;
        public MainWindow()
        {
            InitializeComponent();
            player = new MediaPlayer();
            try
            {
                player.Open(new Uri("..\\..\\Content\\moon_theme.mp3", UriKind.Relative));
                player.Volume = .25;
                player.MediaEnded += new EventHandler(SongEnded);
                //player.Play();
            }
            catch(Exception e)
            {
            }
        }
        public void SongEnded(object sender, EventArgs e)
        {
            player.Position = TimeSpan.Zero;
            player.Play();
        }
    }
}
