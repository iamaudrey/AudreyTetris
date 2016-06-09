//Block.cs
//Audrey Henry

/*This file contains defenitions for the Block, Cell, and BlockFactory classes. A Block represents
 a Tetris block, like the line or 'T' block. Blocks are made up of a 2-dimensional array of Cells.
 To determine the shape and appearance of the Block, the 2-D array is filled with Cells, and each
 Cell is either populated or it's not. If a cell in a 2-D array isn't "populated", then that cell
 doesn't render, and it won't be considered when performing collision detection later. 
 The BlockFactory class has the "blueprints" for each type of Block in the game, and allows us to
 grab a random block.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Tetris
{
    public class Block
    {
        private Cell[,] cells;
        public Cell[,] Cells
        {
            get
            {
                return cells;
            }
            set
            {
                if (value != null)
                    cells = value;
            }
        }

        //Coordinates in the game grid that the bottom left corner of the block
        //corresponds to. 
        private int[] originCoords;
        public int[] OriginCoords
        {
            get
            {
                return originCoords;
            }
            set
            {
                originCoords = value;
            }
        }
        
        //Constructor
        public Block()
        {
            originCoords = new int[2]{0,0};
        }

        //These methods return a new Block after rotating
        public void RotateLeft()
        {
            Cell[,] newCells = new Cell[cells.GetLength(0), cells.GetLength(1)]; 
            int max = cells.GetLength(0) - 1;
            for (int i = 0; i < cells.GetLength(0); i++)
            {
                for (int j = 0; j < cells.GetLength(1); j++)
                {
                    newCells[i, j] = cells[max - j, i];
                }

            }
            Cells = newCells;
        }

        public Block RotateRight()
        {
            Block newBlock = new Block();
            newBlock.OriginCoords = this.OriginCoords;
            Cell[,] newCells = new Cell[cells.GetLength(0), cells.GetLength(1)];
            int max = cells.GetLength(0) - 1;
            for (int i = 0; i < cells.GetLength(0); i++)
            {
                for (int j = 0; j < cells.GetLength(1); j++)
                {
                    newCells[i, j] = cells[j, max - i];
                }

            }
            newBlock.Cells = newCells;
            return newBlock;
        }
        //This method copies the contents of the current block and returns a new instance
        public Block CreateCopy()
        {
            Block newBlock = new Block();
            newBlock.OriginCoords = originCoords;
            newBlock.Cells = cells;
            return newBlock;
        }
        //These methods update the origin coordinates of the block
        public void MoveLeft()
        {
            originCoords[0]--;
        }
        public void MoveRight()
        {
            originCoords[0]++;
        }
        public void MoveDown()
        {
            originCoords[1]--;
        }
    }

    [Serializable]
    public class BlockFactory
    {
        Random rand;
        public BlockFactory()
        {
            rand = new Random();
        }
        //Random generator for a block
        public Block GetRandomBlock()
        {
            int randomInt = rand.Next(100);
            Block result;
            if(randomInt < 13)
            {
                result = NewLeftLBlock();
            }
            else if(randomInt < 27)
            {
                result = NewRightLBlock();
            }
            else if(randomInt < 42)
            {
                result = NewLeftSBlock();
            }
            else if(randomInt < 56)
            {
                result = NewRightSBlock();
            }
            else if(randomInt < 70)
            {
                result = NewSquareBlock();
            }
            else if(randomInt < 84)
            {
                result = NewTBlock();
            }
            else
            {
                result = NewLineBlock();
            }
            randomInt = rand.Next(4);
            for(int i = 0; i < randomInt; i++)
            {
                result.RotateLeft();
            }
            return result;
        }

        //The following are templates for the different block types
        public Block NewTBlock()
        {
            Block result = new Block();
            Cell[,] newCells = new Cell[3, 3];
            newCells[0, 0] = new Cell("#ff0000");
            newCells[0, 1] = new Cell("#ff0000");
            newCells[0, 2] = new Cell("#ff0000");
            newCells[1, 0] = new Cell("#ff0000", false);
            newCells[1, 1] = new Cell("#ff0000");
            newCells[1, 2] = new Cell("#ff0000", false);
            newCells[2, 0] = new Cell("#ff0000", false);
            newCells[2, 1] = new Cell("#ff0000", false);
            newCells[2, 2] = new Cell("#ff0000", false);
            result.Cells = newCells;
            return result;
        }
        
        public Block NewLeftLBlock()
        {
            Block result = new Block();
            Cell[,] newCells = new Cell[3, 3];
            newCells[0, 0] = new Cell("#00cc99");
            newCells[0, 1] = new Cell("#00cc99");
            newCells[0, 2] = new Cell("#00cc99", false);
            newCells[1, 0] = new Cell("#00cc99");
            newCells[1, 1] = new Cell("#00cc99", false);
            newCells[1, 2] = new Cell("#00cc99", false);
            newCells[2, 0] = new Cell("#00cc99");
            newCells[2, 1] = new Cell("#00cc99", false);
            newCells[2, 2] = new Cell("#00cc99", false);
            result.Cells = newCells;
            return result;
        }
        
        public Block NewRightLBlock()
        {
            Block result = new Block();
            Cell[,] newCells = new Cell[3, 3];
            newCells[0, 0] = new Cell("#006600", false);
            newCells[0, 1] = new Cell("#006600");
            newCells[0, 2] = new Cell("#006600");
            newCells[1, 0] = new Cell("#006600", false);
            newCells[1, 1] = new Cell("#006600", false);
            newCells[1, 2] = new Cell("#006600");
            newCells[2, 0] = new Cell("#006600", false);
            newCells[2, 1] = new Cell("#006600", false);
            newCells[2, 2] = new Cell("#006600");
            result.Cells = newCells;
            return result;
        }

        public Block NewSquareBlock()
        {
            Cell[,] cells = new Cell[2,2];
            cells[0, 0] = new Cell("#000099");
            cells[0, 1] = new Cell("#000099");
            cells[1, 0] = new Cell("#000099");
            cells[1, 1] = new Cell("#000099");
            Block result = new Block();
            result.Cells = cells;
            return result;
        }

        public Block NewLineBlock()
        {
            Block result = new Block();
            Cell[,] newCells = new Cell[4, 4];
            for (int i = 0; i < newCells.GetLength(0); i++)
            {
                for(int j = 0; j < newCells.GetLength(1); j++)
                {
                    if(j == 1)
                    {
                        newCells[i, j] = new Cell("#0099cc");
                    }
                    else
                    {
                        newCells[i, j] = new Cell("#0099cc", false);
                    }
                }
            }
            result.Cells = newCells;
            return result;
        }

        public Block NewLeftSBlock()
        {
            Block result = new Block();
            Cell[,] newCells = new Cell[3, 3];
            newCells[0, 0] = new Cell("#9900ff");
            newCells[0, 1] = new Cell("#9900ff");
            newCells[0, 2] = new Cell("#9900ff", false);
            newCells[1, 0] = new Cell("#9900ff", false);
            newCells[1, 1] = new Cell("#9900ff");
            newCells[1, 2] = new Cell("#9900ff");
            newCells[2, 0] = new Cell("#9900ff", false);
            newCells[2, 1] = new Cell("#9900ff", false);
            newCells[2, 2] = new Cell("#9900ff", false);
            result.Cells = newCells;
            return result;
        }

        public Block NewRightSBlock()
        {
            Block result = new Block();
            Cell[,] newCells = new Cell[3, 3];
            newCells[0, 0] = new Cell("#ff6600", false);
            newCells[0, 1] = new Cell("#ff6600");
            newCells[0, 2] = new Cell("#ff6600");
            newCells[1, 0] = new Cell("#ff6600");
            newCells[1, 1] = new Cell("#ff6600");
            newCells[1, 2] = new Cell("#ff6600", false);
            newCells[2, 0] = new Cell("#ff6600", false);
            newCells[2, 1] = new Cell("#ff6600", false);
            newCells[2, 2] = new Cell("#ff6600", false);
            result.Cells = newCells;
            return result;
        }
    }

    [Serializable]
    public class Cell
    {
        private bool isPopulated;
        public bool IsPopulated
        {
            get
            {
                return isPopulated;
            }
            set
            {
                isPopulated = value;
            }
        }
        private string color;
        public string Color
        {
            get
            {
                return color;
            }
            set
            {
                if (value != null)
                {
                    color = value;
                }
            }
        }

        [NonSerialized]
        private Rectangle rect;
        public Rectangle Rect
        {
            get
            {
                return rect;
            }
            set
            {
                if (value != null)
                    rect = value;
            }
        }

        //Constructors
        public Cell(string newColor)
        {
            isPopulated = true;
            color = newColor;
            rect = new Rectangle();
            rect.Stroke = Brushes.AntiqueWhite;
            rect.StrokeThickness = 2;
            rect.Height = 24;
            rect.Width = 24;
            rect.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(newColor));
        }
        public Cell(string newColor, bool populated)
        {
            isPopulated = populated;
            color = newColor;
            rect = new Rectangle();
            rect.Stroke = Brushes.AntiqueWhite;
            rect.StrokeThickness = 2;
            rect.Height = 24;
            rect.Width = 24;
            rect.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(newColor));
        }
    }
}
