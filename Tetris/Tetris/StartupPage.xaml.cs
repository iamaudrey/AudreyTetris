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

namespace Tetris
{
    /// <summary>
    /// Interaction logic for StartupPage.xaml
    /// </summary>
    public partial class StartupPage : Page
    {
        public StartupPage()
        {
            InitializeComponent();
        }

        private void Label_exitLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

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
            this.NavigationService.Navigate(new GamePage());
        }

    }
}
