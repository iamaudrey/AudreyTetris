using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    class Block
    {
        private Cell[][] cells;
        public Cell[][] Cells
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

        private String color;
        public String Color
        {
            get
            {
                return color;
            }
            set
            {
                if (value != null)
                    color = value;
            }
        }

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

        }
    }

    class Cell
    {
        private bool isPopulated;
        public bool IsPopulated { get; set; }

        //Constructors
        public Cell()
        {
            isPopulated = true;
        }
        public Cell(bool populated)
        {
            isPopulated = populated;
        }
    }
}
