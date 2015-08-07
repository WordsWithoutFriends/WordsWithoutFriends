using System.Collections.Generic;

namespace WWF
{
    public class Grid
    {
        private readonly Square[,] _grid;
        
        public Grid(int gridSize)
        {
            _grid = new Square[gridSize, gridSize];
        }

        public Grid(List<List<Square>> squares)
        {
            _grid = new Square[squares.Count, squares.Count];

            var size = squares.Count;
            
            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    _grid[x, y] = squares[y][x];
                }
            }
        }

        public Square GetSquare(int x, int y)
        {
            return _grid[x, y];
        }

        public void SetSquare(int x, int y, Square square)
        {
            _grid[x, y] = square;
        }

        public Grid RotateCounterClockwise()
        {
            var rotatedGrid = new Grid(_grid.GetLength(0));

            for (var r = 0; r < _grid.GetLength(0); r++)
            {
                for (var c = 0; c < _grid.GetLength(1); c++)
                {
                    rotatedGrid.SetSquare(r, c, GetSquare(_grid.GetLength(1) - 1 - c, r));
                }
            }

            return rotatedGrid;
        }
    }
}
