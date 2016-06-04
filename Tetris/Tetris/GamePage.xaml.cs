using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
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
        private System.Timers.Timer timer;
        private Random rand;
        private bool settled = false;
        private bool shortcutsEnabled = false;
        private bool isRunning;
        private bool canPause = false;
        private bool canResume = false;
        public GamePage(GameState state)
        {
            InitializeComponent();
            rand = new Random();
            Canvas_mainGrid.Focus();
            gameState = state;
            this.Focus();
            isRunning = true;
            gameState.NextBlock = gameState.Factory.GetRandomBlock();
            int randomX = rand.Next(9 - gameState.NextBlock.Cells.GetLength(0));
            int originY = 18 - gameState.NextBlock.Cells.GetLength(1);
            TextBlock_Level.Text = gameState.Level.ToString();
            TextBlock_CurrentRows.Text = gameState.RowsForCurrentLevel.ToString();
            TextBlock_CurrentScore.Text = gameState.Score.ToString();
            gameState.NextBlock.OriginCoords = new int[] { randomX, originY };
            NewBlock();
        }

        private void Label_exitLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            timer.Stop();
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
            gameState.CurrentBlock = gameState.NextBlock;
            foreach(Cell c in gameState.CurrentBlock.Cells)
            {
                Canvas_nextBlock.Children.Remove(c.Rect);
            }
            gameState.NextBlock = gameState.Factory.GetRandomBlock();
            int randomX = rand.Next(9 - gameState.NextBlock.Cells.GetLength(0));
            int originY = 18 - gameState.NextBlock.Cells.GetLength(1);
            gameState.NextBlock.OriginCoords = new int[] { randomX, originY };
            AddNextBlockToGrid(gameState.NextBlock);
            AddBlockToGrid(gameState.CurrentBlock);
            if(!SpaceAlreadyOccupied())
            {
                shortcutsEnabled = true;
                //TODO: Make dependent on level
                timer = new System.Timers.Timer(500 * (Math.Pow(.75, gameState.Level - 1)));
                timer.Elapsed += BlockDown;
                timer.Start();
            }
            else
            {
                GameOver();
            }
        }
        //This method takes a block and displays it on the game grid
        private void AddBlockToGrid(Block newBlock)
        {
            //Check for valid origin coordinates
            if(newBlock.OriginCoords.Length < 2)
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

        private void AddNextBlockToGrid(Block newBlock)
        {
            for (int i = 0; i < newBlock.Cells.GetLength(0); i++)
            {
                for (int j = newBlock.Cells.GetLength(1) - 1; j >= 0; j--)
                {
                    if (newBlock.Cells[i, j].IsPopulated)
                    {
                        Canvas.SetLeft(newBlock.Cells[i, j].Rect, (24 * i) + 3);
                        Canvas.SetTop(newBlock.Cells[i, j].Rect, (24 * (newBlock.Cells.GetLength(1) - j - 1)) + 3);
                        Canvas_nextBlock.Children.Add(newBlock.Cells[i, j].Rect);
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

        private void PauseGame(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            shortcutsEnabled = false;
            canPause = false;
            canResume = true;
        }
        private void ResumeGame(object sender, RoutedEventArgs e)
        {
            timer.Start();
            shortcutsEnabled = true;
            canPause = true;
            canResume = false;
        }

        private void DebugGameState(object sender, RoutedEventArgs e)
        {
            string x = gameState.GetType().ToString();
            var y = isRunning;
            var z = timer;
            Console.WriteLine(x);
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

        //This method determines whether or not the current block collides with existing blocks on the game grid
        private bool SpaceAlreadyOccupied()
        {
            for(int i = 0; i < gameState.CurrentBlock.Cells.GetLength(0); i++)
            {
                for(int j = 0; j < gameState.CurrentBlock.Cells.GetLength(1); j++)
                {
                    if(gameState.CurrentBlock.Cells[i,j].IsPopulated && gameState.Grid[gameState.CurrentBlock.OriginCoords[0] + i,
                        gameState.CurrentBlock.OriginCoords[1] + j].IsPopulated)
                    {
                        return true;
                    }
                }
            }
            return false;

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
        private void DropBlock(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            shortcutsEnabled = false;
            timer = new System.Timers.Timer(10);
            timer.Elapsed += BlockDown;
            timer.Start();
        }

        private void HandleDroppedBlock()
        {
            shortcutsEnabled = false;
            for (int i = 0; i < gameState.CurrentBlock.Cells.GetLength(0); i++)
            {
                for (int j = 0; j < gameState.CurrentBlock.Cells.GetLength(1); j++)
                {
                    if (gameState.CurrentBlock.Cells[i,j].IsPopulated)
                    {
                        gameState.Grid[gameState.CurrentBlock.OriginCoords[0] + i, gameState.CurrentBlock.OriginCoords[1] + j] = gameState.CurrentBlock.Cells[i, j];
                    }
                }
            }
            List<int> fullRows = new List<int>();
            for (int i = 0; i < gameState.Grid.GetLength(1); i++)
            {
                bool rowFull = true;
                for(int j = 0; j < gameState.Grid.GetLength(0); j++)
                {
                    if(!gameState.Grid[j,i].IsPopulated)
                    {
                        rowFull = false;
                    }
                }
                if(rowFull)
                {
                    fullRows.Add(i);
                }
            }
            foreach(int i in fullRows)
            {
                for(int j = 0; j < gameState.Grid.GetLength(0); j++)
                {
                    Canvas_blockGrid.Children.Remove(gameState.Grid[j, i].Rect);
                    gameState.Grid[j, i].IsPopulated = false;
                    gameState.Grid[j, i].Rect.InvalidateVisual();
                }
            }
            Canvas_blockGrid.InvalidateVisual();
            Thread.Sleep(500);
            int rowOffset = 0;
            for(int i = 0; i < fullRows.Count; i++)
            {
                int numberOfRows = 1;
                while(fullRows.Contains(fullRows.ElementAt(i) + numberOfRows))
                {
                    numberOfRows++;
                }
                DropRows(fullRows.ElementAt(i) - rowOffset, numberOfRows);
                i += (numberOfRows - 1);
                rowOffset += numberOfRows;
            }
            gameState.RowsForCurrentLevel += fullRows.Count;
            if(fullRows.Count > 0)
            {
                gameState.Score += (fullRows.Count * 100 * gameState.Level);
                if (fullRows.Count > 1)
                {
                    gameState.Score += ((fullRows.Count - 1) * 50 * gameState.Level);
                }
                TextBlock_CurrentScore.Text = gameState.Score.ToString();
            }
            if(gameState.RowsForCurrentLevel >= 10)
            {
                LevelUp();
            }
            else
            {
                TextBlock_CurrentRows.Text = gameState.RowsForCurrentLevel.ToString();
                NewBlock();
            } 
        }
        private void DropRows(int startRowIndex, int numberOfRows)
        {
            for(int i = 0; i < numberOfRows; i++)
            {
                int bottomIndex = startRowIndex + numberOfRows - i;
                for(int row = bottomIndex; row < gameState.Grid.GetLength(1); row++)
                {
                    for(int column = 0; column < gameState.Grid.GetLength(0); column++)
                    {
                        gameState.Grid[column, row - 1] = gameState.Grid[column, row];
                        Canvas.SetTop(gameState.Grid[column, row - 1].Rect, Canvas.GetTop(gameState.Grid[column, row - 1].Rect) + 24);
                    }
                }
                for(int column = 0; column < gameState.Grid.GetLength(0); column++)
                {
                    gameState.Grid[column, gameState.Grid.GetLength(1) - 1] = new Cell("#FFFFFF", false);
                }
                foreach(UIElement e in Canvas_blockGrid.Children)
                {
                    e.InvalidateVisual();
                }
                Thread.Sleep(500);
            }
        }
        private void LevelUp()
        {
            gameState.Level++;
            gameState.RowsForCurrentLevel = 0;
            TextBlock_Level.Text = gameState.Level.ToString();
            TextBlock_CurrentRows.Text = gameState.RowsForCurrentLevel.ToString();
            NewBlock();
        }
        private void GameOver()
        {
            shortcutsEnabled = false;
            TextBlock gameOverBlock = new TextBlock();
            gameOverBlock.Text = "Game Over";
            gameOverBlock.Width = 300;
            gameOverBlock.Height = 250;
            gameOverBlock.Background = Brushes.Blue;
            gameOverBlock.FontSize = 20;
            Canvas.SetTop(gameOverBlock, 100);
            Canvas.SetLeft(gameOverBlock, 50);
            Canvas_mainGrid.Children.Add(gameOverBlock);
            //TODO: Game OVer
        }

        public void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = shortcutsEnabled;
        }
        public void CanPause(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = canPause;
        }
        public void CanResume(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = canResume;
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
                        new KeyGesture(Key.Space)
            });
        public static readonly RoutedUICommand RotateBlock = new RoutedUICommand(
            "RotateBlock",
            "RotateBlock",
            typeof(TetrisCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Tab)
            });
        public static readonly RoutedUICommand MoveBlockLeft = new RoutedUICommand(
            "MoveBlockLeft",
            "MoveBlockLeft",
            typeof(TetrisCommands),
            new InputGestureCollection()
            {
                                new KeyGesture(Key.Left)
            });
        public static readonly RoutedUICommand MoveBlockRight = new RoutedUICommand(
            "MoveBlockRight",
            "MoveBlockRight",
            typeof(TetrisCommands),
            new InputGestureCollection()
            {
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
                                        new KeyGesture(Key.Down)
            });
        public static readonly RoutedUICommand PauseGame = new RoutedUICommand(
        "PauseGame",
        "PauseGame",
        typeof(TetrisCommands),
        new InputGestureCollection()
            {
                                        new KeyGesture(Key.P, ModifierKeys.Control)
            });
        public static readonly RoutedUICommand ResumeGame = new RoutedUICommand(
        "ResumeGame",
        "ResumeGame",
        typeof(TetrisCommands),
        new InputGestureCollection()
            {
                                        new KeyGesture(Key.G, ModifierKeys.Control)
            });
        public static readonly RoutedUICommand DebugGame = new RoutedUICommand(
        "DebugGame",
        "DebugGame",
        typeof(TetrisCommands),
        new InputGestureCollection()
            {
                                        new KeyGesture(Key.Escape)
            });
    }
}
