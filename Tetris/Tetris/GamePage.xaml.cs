using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Timers;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        private Timer timer;
        private Random rand;
        private bool settled = false;
        private bool shortcutsEnabled = false;
        private bool isRunning;
        public GamePage(GameState state)
        {
            InitializeComponent();
            rand = new Random();
            Canvas_mainGrid.Focus();
            gameState = state;
            this.Focus();
            isRunning = true;
            NewBlock();
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

        private delegate void UpdateBlockCallback(Block b);
        private delegate void DroppedBlockCallback();

        private void NewBlock()
        {
            settled = false;
            Block b = gameState.Factory.GetRandomBlock();
            int randomX = rand.Next(9 - b.Cells.GetLength(0));
            int originY = 18 - b.Cells.GetLength(1);
            b.OriginCoords = new int[] { randomX, originY };
            gameState.CurrentBlock = b;
            AddBlockToGrid(b);
            shortcutsEnabled = true;
            //TODO: Make dependent on level
            timer = new Timer(500);
            timer.Elapsed += BlockDown;
            timer.Start();
        }
        //This method takes a block and displays it on the game grid
        private void AddBlockToGrid(Block newBlock)
        {
            //Check for valid origin coordinates
            if(newBlock.OriginCoords.Length < 2 /*|| newBlock.OriginCoords[0] < 0 
                || newBlock.OriginCoords[1] > (17 - newBlock.Cells.GetLength(0)) ||
                newBlock.OriginCoords[1] < 0 || newBlock.OriginCoords[0] > 10*/)
            {
                return;
            }
            for(int i = 0; i < newBlock.Cells.GetLength(0); i++)
            {
                for(int j = 0; j < newBlock.Cells.GetLength(1); j++)
                {
                    if(i + newBlock.OriginCoords[0] >= 0 && i + newBlock.OriginCoords[0] < 10 && 
                        j + newBlock.OriginCoords[1] >=0 && j + newBlock.OriginCoords[1] < 18 && newBlock.Cells[i,j].IsPopulated)
                    {
                        Canvas.SetLeft(newBlock.Cells[i, j].Rect, 24 * (i + newBlock.OriginCoords[0]));
                        Canvas.SetTop(newBlock.Cells[i, j].Rect, 408 - ((j + newBlock.OriginCoords[1]) * 24));
                        Canvas_blockGrid.Children.Add(newBlock.Cells[i, j].Rect);
                    }

                }
            }
        }

        private void UpdateBlockOnGrid(Block block)
        {             
            //Check for valid origin coordinates
            if (block.OriginCoords.Length < 2)
            {
                return;
            }
            for (int i = 0; i < block.Cells.GetLength(0); i++)
            {
                for (int j = 0; j < block.Cells.GetLength(1); j++)
                {
                    if (i + block.OriginCoords[0] >= 0 && i + block.OriginCoords[0] < 10 &&
                        j + block.OriginCoords[1] >= 0 && j + block.OriginCoords[1] < 18 && block.Cells[i, j].IsPopulated)
                    {
                        Canvas.SetLeft(block.Cells[i, j].Rect, 24 * (i + block.OriginCoords[0]));
                        Canvas.SetTop(block.Cells[i, j].Rect, 408 - ((j + block.OriginCoords[1]) * 24));
                    }

                }
            }
        }

        private void RotateBlock(object sender, RoutedEventArgs e)
        {
            if(CanRotate())
            {
                gameState.CurrentBlock.RotateLeft();
                UpdateBlockOnGrid(gameState.CurrentBlock);
            }
        }

        private void MoveBlockLeft(object sender, RoutedEventArgs e)
        {
            if(CanMoveLeft())
            {
                gameState.CurrentBlock.MoveLeft();
                RemoveBlockFromGrid(gameState.CurrentBlock);
                AddBlockToGrid(gameState.CurrentBlock);
            }
        }
        private void MoveBlockRight(object sender, RoutedEventArgs e)
        {
            if(CanMoveRight())
            {
                gameState.CurrentBlock.MoveRight();
                RemoveBlockFromGrid(gameState.CurrentBlock);
                AddBlockToGrid(gameState.CurrentBlock);
            }
        }

        //Will have to test all populated cells and coordinates to make sure they're in frame
        private bool CanRotate()
        {
            Block testBlock = gameState.CurrentBlock.CreateCopy();
            testBlock.RotateLeft();
            for(int i = 0; i < testBlock.Cells.GetLength(0); i++)
            {
                for(int j = 0; j < testBlock.Cells.GetLength(1); j++)
                {
                    if(testBlock.Cells[i,j].IsPopulated && (testBlock.OriginCoords[0] + i < 0 || testBlock.OriginCoords[0] + i >= 10 ||
                        testBlock.OriginCoords[1] + j < 0 || testBlock.OriginCoords[1] + j >= 18))
                    {
                        return false;
                    }
                    else if(testBlock.Cells[i,j].IsPopulated && gameState.Grid[testBlock.OriginCoords[0] + i,testBlock.OriginCoords[1] + j].IsPopulated)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private bool CanMoveLeft()
        {
            for (int i = 0; i < gameState.CurrentBlock.Cells.GetLength(1); i++)
            {
                Cell leftMostCell = gameState.CurrentBlock.Cells[0,i];
                int column = 0;
                while(!leftMostCell.IsPopulated && column < gameState.CurrentBlock.Cells.GetLength(0) - 1)
                {
                    column++;
                    leftMostCell = gameState.CurrentBlock.Cells[column, i];
                }
                if (leftMostCell.IsPopulated && ((gameState.CurrentBlock.OriginCoords[0] + (column-1) < 0) || gameState.Grid[gameState.CurrentBlock.OriginCoords[0] + (column - 1), gameState.CurrentBlock.OriginCoords[1] + i].IsPopulated))
                    return false;
            }
            return true;
        }

        private bool CanMoveRight()
        {
            for (int i = 0; i < gameState.CurrentBlock.Cells.GetLength(1); i++)
            {
                Cell rightMostCell = gameState.CurrentBlock.Cells[gameState.CurrentBlock.Cells.GetLength(0) - 1, i];
                int column = gameState.CurrentBlock.Cells.GetLength(0) - 1;
                while (!rightMostCell.IsPopulated && column > 1)
                {
                    column--;
                    rightMostCell = gameState.CurrentBlock.Cells[column, i];
                }
                if (rightMostCell.IsPopulated && ((gameState.CurrentBlock.OriginCoords[0] + (column + 1) >= 10) || gameState.Grid[gameState.CurrentBlock.OriginCoords[0] + (column + 1), gameState.CurrentBlock.OriginCoords[1] + i].IsPopulated))
                    return false;
            }
            return true;
        }

        private bool CanMoveDown()
        {
            for (int i = 0; i < gameState.CurrentBlock.Cells.GetLength(0); i++)
            {
                Cell bottomMostCell = gameState.CurrentBlock.Cells[i, 0];
                int row = 0;
                while (!bottomMostCell.IsPopulated && row < gameState.CurrentBlock.Cells.GetLength(1)-1)
                {
                    row++;
                    bottomMostCell = gameState.CurrentBlock.Cells[i, row];
                }
                if (bottomMostCell.IsPopulated && ((gameState.CurrentBlock.OriginCoords[1] + (row - 1) < 0) || gameState.Grid[gameState.CurrentBlock.OriginCoords[0] + i, gameState.CurrentBlock.OriginCoords[1] + row - 1].IsPopulated))
                    return false;
            }
            return true;
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

        private void BlockDown(object sender, ElapsedEventArgs e)
        {
            if (CanMoveDown())
            {
                gameState.CurrentBlock.MoveDown();
                //If the settled value had been set previously but the block was able to move to where it could drop again,
                //we want to reset the settled value.
                settled = false;
                Application.Current.Dispatcher.Invoke(new UpdateBlockCallback(UpdateBlockOnGrid), new object[]{ gameState.CurrentBlock});
            }
            //If the block isn't settled, give the user one more pass of the timer
            else if(!settled)
            {
                settled = true;
            }
            else
            {
                timer.Stop();
                Application.Current.Dispatcher.Invoke(new DroppedBlockCallback(HandleDroppedBlock));
            }
        }

        private void HandleDroppedBlock()
        {
            shortcutsEnabled = false;
            for (int i = 0; i < gameState.CurrentBlock.Cells.GetLength(0); i++)
            {
                for (int j = 0; j < gameState.CurrentBlock.Cells.GetLength(1); j++)
                {
                    if (gameState.CurrentBlock.Cells[i,j].IsPopulated
                        /*gameState.CurrentBlock.OriginCoords[0] + i >= 0 && gameState.CurrentBlock.OriginCoords[0] + i < 10 &&
                        gameState.CurrentBlock.OriginCoords[1] + j >= 0 && gameState.CurrentBlock.OriginCoords[1] + j < 18*/)
                    {
                        gameState.Grid[gameState.CurrentBlock.OriginCoords[0] + i, gameState.CurrentBlock.OriginCoords[1] + j] = gameState.CurrentBlock.Cells[i, j];
                    }
                }
            }
            //TODO: Check for full row
            NewBlock();
        }
        private void LevelUp()
        {
            //TODO: Level up
        }
        private void GameOver()
        {
            //TODO: Game OVer
        }

        public void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = shortcutsEnabled;
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
        public static readonly RoutedUICommand MoveBlockLeft = new RoutedUICommand(
        "MoveBlockLeft",
        "MoveBlockLeft",
        typeof(TetrisCommands),
        new InputGestureCollection()
        {
                            //Set the shortcut to CTRL + P
                            new KeyGesture(Key.Left)
        });
        public static readonly RoutedUICommand MoveBlockRight = new RoutedUICommand(
        "MoveBlockRight",
        "MoveBlockRight",
        typeof(TetrisCommands),
        new InputGestureCollection()
        {
                                    //Set the shortcut to CTRL + P
                                    new KeyGesture(Key.Right)
        });
        //The upkey and the down key won't do anything, but I need to handle the event anyway because
        //otherwise the page will lose focus.
        public static readonly RoutedUICommand UpKey = new RoutedUICommand(
        "UpKey",
        "UpKey",
        typeof(TetrisCommands),
        new InputGestureCollection()
        {
                                    new KeyGesture(Key.Up)
        });
        public static readonly RoutedUICommand DownKey = new RoutedUICommand(
        "DownKey",
        "DownKey",
        typeof(TetrisCommands),
        new InputGestureCollection()
        {
                                    //Set the shortcut to CTRL + P
                                    new KeyGesture(Key.Down)
        });
    }
}
