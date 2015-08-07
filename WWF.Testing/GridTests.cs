using System.Collections.Generic;
using Xunit;

namespace WWF.Testing
{
    public class GridTests
    {
        [Fact]
        public void CreatesGridFromLists()
        {
            var grid = new Grid(new List<List<Square>>
            {
                new List<Square> {new Square {Letter = 'A'}, new Square {Letter = 'B'}},
                new List<Square> {new Square {Letter = 'C'}, new Square {Letter = 'D'}}
            });

            Assert.Equal('A', grid.GetSquare(0, 0).Letter);
            Assert.Equal('B', grid.GetSquare(1, 0).Letter);
            Assert.Equal('C', grid.GetSquare(0, 1).Letter);
            Assert.Equal('D', grid.GetSquare(1, 1).Letter);
        }

        [Fact]
        public void RotateGrid()
        {
            var grid = new Grid(new List<List<Square>>
            {
                new List<Square> {new Square {Letter = 'A'}, new Square {Letter = 'B'}},
                new List<Square> {new Square {Letter = 'C'}, new Square {Letter = 'D'}}
            });

            var rotatedGrid = grid.RotateCounterClockwise();

            Assert.Equal('B', rotatedGrid.GetSquare(0, 0).Letter);
            Assert.Equal('D', rotatedGrid.GetSquare(1, 0).Letter);
            Assert.Equal('A', rotatedGrid.GetSquare(0, 1).Letter);
            Assert.Equal('C', rotatedGrid.GetSquare(1, 1).Letter);
        }
    }
}
