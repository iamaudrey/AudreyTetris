using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*The purpose of this class is to store and manage all of the data pertaining to a Tetris game. This includes information
like score, the cells in the current grid, the block that is currently falling, the next block that will fall, and the
current level.*/
namespace Tetris
{
    [Serializable]
    public class GameState
    {
        private Cell[,] grid;

        [NonSerialized]
        private Block currentBlock;
        [NonSerialized]
        private Block nextBlock;

        private int level;
        private int score;
        private BlockFactory factory;

        public Cell[,] Grid
        {
            get
            {
                return grid;
            }
            set
            {
                if (value != null)
                    grid = value;
            }
        }
        public Block CurrentBlock
        {
            get
            {
                return currentBlock;
            }
            set
            {
                if(value != null)
                {
                    currentBlock = value;
                }
            }
        }
        public Block NextBlock
        {
            get
            {
                return nextBlock;
            }
            set
            {
                if (value != null)
                {
                    nextBlock = value;
                }
            }
        }
        public int Level
        {
            get
            {
                return level;
            }
            set
            {
                if (value > 0)
                    level = value;
            }
        }
        public int Score
        {
            get
            {
                return score;
            }
            set
            {
                if (value >= 0)
                    score = value;
            }
        }
        private int rowsForCurrentLevel;
        public int RowsForCurrentLevel
        {
            get
            {
                return rowsForCurrentLevel;
            }
            set
            {
                if (value >= 0)
                    rowsForCurrentLevel = value;
            }
        }
        //No need to set this value after constructor.
        public BlockFactory Factory
        {
            get
            {
                return factory;
            }
        }

        //Constructor
        public GameState()
        {
            factory = new BlockFactory();
            grid = new Cell[10, 18];
            for(int i = 0; i < 10; i++)
            {
                for(int j = 0; j < 18; j++)
                {
                    grid[i, j] = new Cell("#FFFFFF", false);
                }
            }
            score = 0;
            level = 1;
            currentBlock = factory.GetRandomBlock();
            nextBlock = factory.GetRandomBlock();
            rowsForCurrentLevel = 0;
        }
    }
}
