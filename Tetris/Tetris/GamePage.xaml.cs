using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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
    /// Interaction logic for GamePage.xaml
    /// </summary>
    public partial class GamePage : Page
    {
        private GameState gameState;
        public GamePage(GameState state)
        {
            InitializeComponent();
            Canvas_mainGrid.Focus();
            gameState = state;
            Block b = gameState.Factory.GetRandomBlock();
            b.OriginCoords = new int[]{2, 4};
            gameState.CurrentBlock = b;
            AddBlockToGrid(b);
            this.Focus();
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

        //This method takes a block and displays it on the game grid
        private void AddBlockToGrid(Block newBlock)
        {
            //Check for valid origin coordinates
            if(newBlock.OriginCoords.Length < 2 || newBlock.OriginCoords[0] < 0 
                || newBlock.OriginCoords[1] > (17 - newBlock.Cells.GetLength(0)) ||
                newBlock.OriginCoords[1] < 0 || newBlock.OriginCoords[0] > 10)
            {
                return;
            }
            for(int i = 0; i < newBlock.Cells.GetLength(0); i++)
            {
                for(int j = 0; j < newBlock.Cells.GetLength(1); j++)
                {
                    if(newBlock.Cells[i,j].IsPopulated)
                    {
                        Canvas.SetLeft(newBlock.Cells[i, j].Rect, 24 * (i + newBlock.OriginCoords[0]));
                        Canvas.SetTop(newBlock.Cells[i, j].Rect, 408 - ((j + newBlock.OriginCoords[1]) * 24));
                        Canvas_blockGrid.Children.Add(newBlock.Cells[i, j].Rect);
                    }

                }
            }
        }

        private void RotateBlock(object sender, RoutedEventArgs e)
        {
            gameState.CurrentBlock = gameState.CurrentBlock.RotateLeft();
            RemoveBlockFromGrid(gameState.CurrentBlock);
            AddBlockToGrid(gameState.CurrentBlock);
        }

        private void RemoveBlockFromGrid(Block block)
        {
            for(int i = 0; i < block.Cells.GetLength(0); i++)
            {
                for(int j = 0; j < block.Cells.GetLength(1); j++)
                {
                    if(block.Cells[i,j].IsPopulated)
                    {
                        Canvas_blockGrid.Children.Remove(block.Cells[i, j].Rect);
                    }
                }
            }
        }

        private void Canvas_mainGrid_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Space)
            {
                gameState.CurrentBlock = gameState.CurrentBlock.RotateLeft();
                RemoveBlockFromGrid(gameState.CurrentBlock);
                AddBlockToGrid(gameState.CurrentBlock);
            }
        }

    }

    public class TetrisCommands
    {
        public TetrisCommands() { }
        //This command will Drop the block down (eventually)
        public static readonly RoutedUICommand DropBlock = new RoutedUICommand(
        "DropBlock",
        "DropBlock",
        typeof(TetrisCommands),
        new InputGestureCollection()
        {
                    //Set the shortcut to CTRL + P
                    new KeyGesture(Key.Space)
        });
    }
}
