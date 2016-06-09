/*GamePage.xaml.cs
Audrey Henry
This is where most of the game logic takes place. This class contains the main game loop, and
all of the instructions for drawing blocks on the screen.*/

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
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Tetris
{
    /// <summary>
    /// Interaction logic for GamePage.xaml
    /// </summary>
    public partial class GamePage : Page
    {
        //The game page has its own GameState object, which keeps track of score, level, etc.
        private GameState gameState;
        //The timer object controls the main game loop
        private System.Timers.Timer timer;
        private Random rand;
        //The settled variable determines whether the block at the bottom of the screen has "settled"
        //When a block hits the bottom of the screen, we give it one more interval of time to shift before
        //it's cemented in place.
        private bool settled = false;
        //This variable helps keep track of whether or not to allow keyboard shortcuts
        private bool shortcutsEnabled = false;
        private bool isRunning;
        private int highScore = 0;
        private bool canPause = false;
        private bool canResume = false;
        //This class contains a reference to the MediaPlayer, so we can control it
        private MediaPlayer player;
        //Constructor
        public GamePage(GameState state, MediaPlayer newPlayer)
        {
            InitializeComponent();
            player = newPlayer;
            rand = new Random();
            Canvas_mainGrid.Focus();
            gameState = state;
            Focus();
            isRunning = true;
            canPause = true;
            //If there is a previous high score recorded, read it from the file and display it on the screen.
            InitializeHighScore();
            if(player.IsMuted)
            {
                MenuItem_Options_Music.IsChecked = false;
            }
            if(!File.Exists("savegame"))
            {
                MenuItem_File_LoadGame.IsEnabled = false;
            }
            //Initialize the first block
            gameState.NextBlock = gameState.Factory.GetRandomBlock();
            int randomX = rand.Next(9 - gameState.NextBlock.Cells.GetLength(0));
            int originY = 18 - gameState.NextBlock.Cells.GetLength(1);
            TextBlock_Level.Text = gameState.Level.ToString();
            TextBlock_CurrentRows.Text = gameState.RowsForCurrentLevel.ToString();
            TextBlock_CurrentScore.Text = gameState.Score.ToString();
            MenuItem_File_LoadGame.IsEnabled = false;
            MenuItem_File_SaveGame.IsEnabled = false;
            gameState.NextBlock.OriginCoords = new int[] { randomX, originY };
            //If the gamestate already had cells in the grid, make sure to redraw them
            for(int i = 0; i < gameState.Grid.GetLength(1); i++)
            {
                for(int j = 0; j < gameState.Grid.GetLength(0); j++)
                {
                    if (gameState.Grid[j,i].IsPopulated)
                    {
                        Canvas.SetLeft(gameState.Grid[j, i].Rect, 24 * j);
                        Canvas.SetTop(gameState.Grid[j, i].Rect, 408 - (i * 24));
                        Canvas_blockGrid.Children.Add(gameState.Grid[j, i].Rect);
                    }
                }

            }
            //Start the game loop with a new block
            NewBlock();
        }

        //Handle user clicking "Exit"
        private void Label_exitLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            timer.Stop();
            WriteHighScore();
            Window.GetWindow(this).Close();
        }

        //Special effects for labels
        private void Label_exitLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            Label_exitLabel.Foreground = new SolidColorBrush(Color.FromRgb(68, 131, 68));
        }

        private void Label_exitLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            Label_exitLabel.Foreground = new SolidColorBrush(Color.FromRgb(24, 64, 24));
        }


        private void Label_SaveGame_MouseEnter(object sender, MouseEventArgs e)
        {
            Label_SaveGame.Foreground = new SolidColorBrush(Color.FromRgb(68, 131, 68));
        }

        private void Label_SaveGame_MouseLeave(object sender, MouseEventArgs e)
        {
            Label_SaveGame.Foreground = new SolidColorBrush(Color.FromRgb(24, 64, 24));
        }

        private void Label_MainMenu_MouseEnter(object sender, MouseEventArgs e)
        {
            Label_MainMenu.Foreground = new SolidColorBrush(Color.FromRgb(68, 131, 68));
        }

        private void Label_MainMenu_MouseLeave(object sender, MouseEventArgs e)
        {
            Label_MainMenu.Foreground = new SolidColorBrush(Color.FromRgb(24, 64, 24));
        }

        //Delegates to help manage block movement
        private delegate void UpdateBlockCallback(Block b);
        private delegate void DroppedBlockCallback();

        //This method makes the next block the current block, and sets it down the path to the bottom
        private void NewBlock()
        {
            settled = false;
            gameState.CurrentBlock = gameState.NextBlock;
            //Remove the block from the "next block" section
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
            //If there's room to drop, initialize the timer
            if(!SpaceAlreadyOccupied())
            {
                shortcutsEnabled = true;
                timer = new System.Timers.Timer(500 * (Math.Pow(.75, gameState.Level - 1)));
                timer.Elapsed += BlockDown;
                timer.Start();
            }
            //Otherwise, it's game over
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

        //This method draws the "next" block to the next block canvas.
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
        //The following three methods move/rotate the block on the screen. These are caused by user interaction
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
            MenuItem_File_LoadGame.IsEnabled = true;
            MenuItem_File_SaveGame.IsEnabled = true;
            canPause = false;
            canResume = true;
            Border_Paused.Visibility = Visibility.Visible;
        }

        private void ResumeGame(object sender, RoutedEventArgs e)
        {
            Border_Paused.Visibility = Visibility.Hidden;
            MenuItem_File_LoadGame.IsEnabled = false;
            MenuItem_File_SaveGame.IsEnabled = false;
            timer.Start();
            shortcutsEnabled = true;
            canPause = true;
            canResume = false;
        }

        //Super secret cheat
        private void LevelUpCheat(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            shortcutsEnabled = false;
            //Remove the currently falling blocks, to be replaced with new ones.
            foreach(Cell c in gameState.CurrentBlock.Cells)
            {
                Canvas_blockGrid.Children.Remove(c.Rect);
            }
            foreach(Cell c in gameState.NextBlock.Cells)
            {
                Canvas_nextBlock.Children.Remove(c.Rect);
            }
            LevelUp();
        }

        //This method determines whether or not we can rotate the currently falling block by testing against the borders of the playing field
        //and the currently settled blocks.
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
        //Like the above function, but tests whether there are any obstacles to the left
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

        //See above, for moving the object right.
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

        //This method determines whether the block can move any further down
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

        //This method clears the block's rectangle objects from the main canvas.
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

        //This method is called for every timer interval. It's pretty much the game loop.
        private void BlockDown(object sender, ElapsedEventArgs e)
        {
            if (CanMoveDown())
            {
                gameState.CurrentBlock.MoveDown();
                //If the settled value had been set previously but the block was able to move to where it could drop again,
                //we want to reset the settled value.
                settled = false;
                try
                {
                    Application.Current.Dispatcher.Invoke(new UpdateBlockCallback(UpdateBlockOnGrid), new object[] { gameState.CurrentBlock });
                }
                catch (Exception)
                {
                    //Do nothing, exceptions generally occur when users try to exit by clicking the 'x'
                }
            }
            //If the block isn't settled, give the user one more pass of the timer
            else if(!settled)
            {
                settled = true;
            }
            //If the block is settled, call the HandleDroppedBlock method
            else
            {
                timer.Stop();
                Application.Current.Dispatcher.Invoke(new DroppedBlockCallback(HandleDroppedBlock));
            }
        }
        //This method drops the block as far as it can go.
        private void DropBlock(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            shortcutsEnabled = false;
            timer = new System.Timers.Timer(10);
            timer.Elapsed += BlockDown;
            timer.Start();
        }

        //This method is called when a block settles at the bottom. It adds the block cells to the game grid and determines whether or not
        //any rows were filled.
        private void HandleDroppedBlock()
        {
            shortcutsEnabled = false;
            //Add the block cells to the game grid
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
            //Loop through the game grid, storing the indexes of full rows
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
            //For each full row, remove the corresponding rectangles from the canvas
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
            //Give the user a moment to pause
            Thread.Sleep(500);
            int rowOffset = 0;
            //This loop drops all of the remaining rows after the full ones were removed
            for(int i = 0; i < fullRows.Count; i++)
            {
                int numberOfRows = 1;
                //Determine how many rows in the current chunk we need to drop
                while(fullRows.Contains(fullRows.ElementAt(i) + numberOfRows))
                {
                    numberOfRows++;
                }
                //Drop all of the rows above the given empty rows
                DropRows(fullRows.ElementAt(i) - rowOffset, numberOfRows);
                //Account for multiple rows in our iterator
                i += (numberOfRows - 1);
                rowOffset += numberOfRows;
            }
            //Update the statistics
            gameState.RowsForCurrentLevel += fullRows.Count;
            //Calculate the score
            if(fullRows.Count > 0)
            {
                gameState.Score += (fullRows.Count * 100 * gameState.Level);
                if (fullRows.Count > 1)
                {
                    gameState.Score += ((fullRows.Count - 1) * 50 * gameState.Level);
                }
                TextBlock_CurrentScore.Text = gameState.Score.ToString();
            }
            //If we hit more than 10 rows for the level, level up
            if(gameState.RowsForCurrentLevel >= 10)
            {
                LevelUp();
            }
            //Otherwise, continue on
            else
            {
                TextBlock_CurrentRows.Text = gameState.RowsForCurrentLevel.ToString();
                NewBlock();
            } 
        }

        //This method is given an index of an empty rows and the number of empty rows above that index. Its purpose
        //is to drop down all of the non-empty rows above the empty ones. 
        private void DropRows(int startRowIndex, int numberOfRows)
        {
            //Loop that executes for every row dropped
            for(int i = 0; i < numberOfRows; i++)
            {
                //Determine the bottom index of the rows that we'll be dropping
                int bottomIndex = startRowIndex + numberOfRows - i;
                //Loop through rows
                for(int row = bottomIndex; row < gameState.Grid.GetLength(1); row++)
                {
                    //Loop through each column in the row, dropping each one down a notch
                    for(int column = 0; column < gameState.Grid.GetLength(0); column++)
                    {
                        gameState.Grid[column, row - 1] = gameState.Grid[column, row];
                        Canvas.SetTop(gameState.Grid[column, row - 1].Rect, Canvas.GetTop(gameState.Grid[column, row - 1].Rect) + 24);
                    }
                }
                //Since we've dropped all of the non-empty rows down one, fill the topmost row with empty cells.
                for(int column = 0; column < gameState.Grid.GetLength(0); column++)
                {
                    gameState.Grid[column, gameState.Grid.GetLength(1) - 1] = new Cell("#FFFFFF", false);
                }
                foreach(UIElement e in Canvas_blockGrid.Children)
                {
                    e.InvalidateVisual();
                }
            }
        }
        //This method updates the statistics for a level up
        private void LevelUp()
        {
            gameState.Level++;
            gameState.RowsForCurrentLevel = 0;
            TextBlock_Level.Text = gameState.Level.ToString();
            TextBlock_CurrentRows.Text = gameState.RowsForCurrentLevel.ToString();
            NewBlock();
        }

        //This method handles a game over scenario
        private void GameOver()
        {
            timer.Stop();
            shortcutsEnabled = false;
            //If we beat the highscore, be sure to record it.
            if(gameState.Score > highScore)
            {
                highScore = gameState.Score;
                TextBlock_HighScore.Text = highScore.ToString();
                WriteHighScore();
            }
            //Ask the user if they want to play again
            MessageBoxResult r = MessageBox.Show("Game Over.\nPlay again?", "Game Over", MessageBoxButton.YesNo);
            if (r.Equals(MessageBoxResult.Yes))
            {
                NewGame();
            }
            //If they don't, exit gracefully.
            else
            {
                timer.Stop();
                Window.GetWindow(this).Close();
            }
        }

        //This method begins a new game, clearing out all of the rectangles on the screen and creating a
        //new gamestate object.
        private void NewGame()
        {
            //Clear out existing blocks
            foreach(Cell c in gameState.Grid)
            {
                Canvas_blockGrid.Children.Remove(c.Rect);
            }
            foreach(Cell c in gameState.CurrentBlock.Cells)
            {
                Canvas_blockGrid.Children.Remove(c.Rect);
            }
            foreach(Cell c in gameState.NextBlock.Cells)
            {
                Canvas_nextBlock.Children.Remove(c.Rect);
            }
            gameState = new GameState();
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

        //This method attempts to read in the latest high score and display it on the screen.
        //If there isn't already a high score file, it sets the high score to zero.
        private void InitializeHighScore()
        {
            if(File.Exists("highscore.txt"))
            {
                try
                {
                    StreamReader reader = new StreamReader(new FileStream("highscore.txt", FileMode.Open));
                    string highScoreString = reader.ReadLine();
                    highScore = Int32.Parse(highScoreString);
                    TextBlock_HighScore.Text = highScore.ToString();
                    reader.Close();
                }
                catch(Exception)
                {
                    highScore = 0;
                    TextBlock_HighScore.Text = "0";
                }
            }
            else
            {
                highScore = 0;
                TextBlock_HighScore.Text = "0";
            }
        }

        //This method writes out the high score to the highscore file. 
        private void WriteHighScore()
        {
            try
            {
                StreamWriter writer = new StreamWriter(new FileStream("highscore.txt", FileMode.Create));
                writer.Write(highScore);
                writer.Close();
            }
            catch(Exception)
            {
                return;
            }
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




        private void Label_MainMenu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Pause game, pop up window.
            timer.Stop();
            shortcutsEnabled = false;
            MessageBoxResult r = MessageBox.Show("Are you sure you want to go to the Main Menu? Your unsaved progress will be lost.", "Are You Sure?", MessageBoxButton.YesNo);
            if(r.Equals(MessageBoxResult.Yes))
            {
                this.NavigationService.Navigate(new StartupPage(player));
            }
            else
            {
                shortcutsEnabled = true;
                timer.Start();
            }
        }

        //This method attempts to save the current game
        private void Label_SaveGame_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            timer.Stop();
            shortcutsEnabled = false;
            try
            {
                //Attempt to serialize the current gamestate and write it to a file.
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream("savegame", FileMode.Create, FileAccess.Write, FileShare.None);
                formatter.Serialize(stream, gameState);
                stream.Close();
                MessageBoxResult r = MessageBox.Show("The game was saved successfully.", "Save Successful", MessageBoxButton.OK);
                if(!MenuItem_File_LoadGame.IsEnabled)
                {
                    MenuItem_File_LoadGame.IsEnabled = true;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("An error occurred while saving the game.");
            }
            //Resume the game
            shortcutsEnabled = true;
            timer.Start();
        }

        //This method toggles whether or not the music is muted
        private void MenuItem_Options_Music_Click(object sender, RoutedEventArgs e)
        {
            if(player != null)
            {
                if(MenuItem_Options_Music.IsChecked)
                {
                    player.IsMuted = false;
                }
                else
                {
                    player.IsMuted = true;
                }
            }
        }

        //This method calls the NewGame method
        private void MenuItem_File_NewGame_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            shortcutsEnabled = false;
            NewGame();
        }

        //This method calls the save game method
        private void MenuItem_File_SaveGame_Click(object sender, RoutedEventArgs e)
        {
            Label_SaveGame_MouseLeftButtonUp(sender, null);
        }

        //This method attempts to load a saved game
        private void MenuItem_File_LoadGame_Click(object sender, RoutedEventArgs e)
        {
            //Pause game, pop up window.
            timer.Stop();
            shortcutsEnabled = false;
            MessageBoxResult r = MessageBox.Show("Are you sure you want to load a saved game? Your unsaved progress will be lost.", "Are You Sure?", MessageBoxButton.YesNo);
            if (r.Equals(MessageBoxResult.Yes))
            {
                try
                {
                    //Deserialize the saved gamestate object
                    BinaryFormatter formatter = new BinaryFormatter();
                    FileStream stream = new FileStream("savegame", FileMode.Open, FileAccess.Read, FileShare.None);
                    GameState result = (GameState)formatter.Deserialize(stream);
                    stream.Close();
                    //Reinitialize the rectangles for each cell in the grid
                    foreach (Cell c in result.Grid)
                    {
                        c.Rect = new Rectangle();
                        c.Rect.Stroke = Brushes.AntiqueWhite;
                        c.Rect.StrokeThickness = 2;
                        c.Rect.Height = 24;
                        c.Rect.Width = 24;
                        c.Rect.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(c.Color));
                    }
                    result.Factory = new BlockFactory();
                    //Reload the page with the new gamestate
                    NavigationService.Navigate(new GamePage(result, player));
                }
                catch(Exception)
                {
                    MessageBox.Show("An error occurred.");
                    shortcutsEnabled = true;
                    timer.Start();
                }
                
            }
        }


        //This method quits the game
        private void MenuItem_File_Quit_Click(object sender, RoutedEventArgs e)
        {
            Label_exitLabel_MouseLeftButtonUp(sender, null);
        }

        //This method launches the HowToPlay window. It conveniently pauses your game for you.
        private void MenuItem_Help_HowToPlay_Click(object sender, RoutedEventArgs e)
        {
            bool wasRunning = false;
            if(canPause)
            {
                PauseGame(sender, e);
                wasRunning = true;
            }
            HowToPlayWindow howtoplay = new HowToPlayWindow();
            howtoplay.ShowDialog();
            if(wasRunning)
            {
                ResumeGame(sender, e);
            }
        }

        //This method launches the About window
        private void MenuItem_Help_About_Click(object sender, RoutedEventArgs e)
        {
            bool wasRunning = false;
            if (canPause)
            {
                PauseGame(sender, e);
                wasRunning = true;
            }
            AboutWindow about = new AboutWindow();
            about.ShowDialog();
            if (wasRunning)
            {
                ResumeGame(sender, e);
            }
        }
    }

    //The following are the Commands that are used to implement keyboard shortcuts
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
        public static readonly RoutedUICommand LevelUp = new RoutedUICommand(
        "LevelUp",
        "LevelUp",
        typeof(TetrisCommands),
        new InputGestureCollection()
            {
                                                new KeyGesture(Key.Home)
            });
    }
}
