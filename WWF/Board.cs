using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WWF
{
    class Board
    {
        public List<List<Square>> BoardEmpty = new List<List<Square>>{
            new List<Square>{new Square(), new Square(), new Square(), new Square {WordBonus = 3}, new Square(), new Square(), new Square {LetterBonus = 3}, new Square(), new Square {LetterBonus = 3}, new Square(), new Square(), new Square {WordBonus = 3}, new Square(), new Square(), new Square()},
            new List<Square>{new Square(), new Square(), new Square {LetterBonus = 2}, new Square(), new Square(), new Square {WordBonus = 2}, new Square(), new Square(), new Square(), new Square {WordBonus = 2}, new Square(), new Square(), new Square {LetterBonus = 2}, new Square(), new Square()},
            new List<Square>{new Square(), new Square {LetterBonus = 2}, new Square(), new Square(), new Square {LetterBonus = 2}, new Square(), new Square(), new Square(), new Square(), new Square(), new Square {LetterBonus = 2}, new Square(), new Square(), new Square {LetterBonus = 2}, new Square()},
            new List<Square>{new Square {WordBonus = 3}, new Square(), new Square(), new Square {LetterBonus = 3}, new Square(), new Square(), new Square(), new Square {WordBonus = 2}, new Square(), new Square(), new Square(), new Square {LetterBonus = 3}, new Square(), new Square(), new Square {WordBonus = 3}},
            new List<Square>{new Square(), new Square(), new Square {LetterBonus = 2}, new Square(), new Square(), new Square(), new Square {LetterBonus = 2}, new Square(), new Square {LetterBonus = 2}, new Square(), new Square(), new Square(), new Square {LetterBonus = 2}, new Square(), new Square()},
            new List<Square>{new Square(), new Square {WordBonus = 2}, new Square(), new Square(), new Square(), new Square {LetterBonus = 3}, new Square(), new Square(), new Square(), new Square {LetterBonus = 3}, new Square(), new Square(), new Square(), new Square {WordBonus = 2}, new Square()},
            new List<Square>{new Square {LetterBonus = 3}, new Square(), new Square(), new Square(), new Square {LetterBonus = 2}, new Square(), new Square(), new Square(), new Square(), new Square(), new Square {LetterBonus = 2}, new Square(), new Square(), new Square(), new Square {LetterBonus = 3}},
            new List<Square>{new Square(), new Square(), new Square(), new Square {WordBonus = 2}, new Square(), new Square(), new Square(), new Square(), new Square(), new Square(), new Square(), new Square {WordBonus = 2}, new Square(), new Square(), new Square()},
            new List<Square>{new Square {LetterBonus = 3}, new Square(), new Square(), new Square(), new Square {LetterBonus = 2}, new Square(), new Square(), new Square(), new Square(), new Square(), new Square {LetterBonus = 2}, new Square(), new Square(), new Square(), new Square {LetterBonus = 3}},
            new List<Square>{new Square(), new Square {WordBonus = 2}, new Square(), new Square(), new Square(), new Square {LetterBonus = 3}, new Square(), new Square(), new Square(), new Square {LetterBonus = 3}, new Square(), new Square(), new Square(), new Square {WordBonus = 2}, new Square()},
            new List<Square>{new Square(), new Square(), new Square {LetterBonus = 2}, new Square(), new Square(), new Square(), new Square {LetterBonus = 2}, new Square(), new Square {LetterBonus = 2}, new Square(), new Square(), new Square(), new Square {LetterBonus = 2}, new Square(), new Square()},
            new List<Square>{new Square {WordBonus = 3}, new Square(), new Square(), new Square {LetterBonus = 3}, new Square(), new Square(), new Square(), new Square {WordBonus = 2}, new Square(), new Square(), new Square(), new Square {LetterBonus = 3}, new Square(), new Square(), new Square {WordBonus = 3}},
            new List<Square>{new Square(), new Square {LetterBonus = 2}, new Square(), new Square(), new Square {LetterBonus = 2}, new Square(), new Square(), new Square(), new Square(), new Square(), new Square {LetterBonus = 2}, new Square(), new Square(), new Square {LetterBonus = 2}, new Square()},
            new List<Square>{new Square(), new Square(), new Square {LetterBonus = 2}, new Square(), new Square(), new Square {WordBonus = 2}, new Square(), new Square(), new Square(), new Square {WordBonus = 2}, new Square(), new Square(), new Square {LetterBonus = 2}, new Square(), new Square()},
            new List<Square>{new Square(), new Square(), new Square(), new Square {WordBonus = 3}, new Square(), new Square(), new Square {LetterBonus = 3}, new Square(), new Square {LetterBonus = 3}, new Square(), new Square(), new Square {WordBonus = 3}, new Square(), new Square(), new Square()},
        };

        public class Fill : Board
        {
            public List<List<Square>> BoardFilled = new List<List<Square>>();

            public Fill(int boardSize)
            {
                BoardFilled = BoardEmpty;

                var letters = new List<string>
                {
                    //012345678901234
                    "               ", //0
                    "               ", //1
                    "               ", //2
                    "               ", //3
                    "               ", //4
                    "               ", //5
                    "choleric       ", //6
                    "       a       ", //7 M
                    "     bottle    ", //8
                    "               ", //9
                    "               ", //10
                    "               ", //11
                    "               ", //12
                    "               ", //13
                    "               ", //14
                    //012345678901234
                };

                for (var i = 0; i < boardSize; i++)
                {
                    var chrs = letters[i].ToCharArray().ToList();
                    for (var j = 0; j < boardSize; j++)
                    {
                        BoardFilled[i][j].Row = i;
                        BoardFilled[i][j].Column = j;
                        BoardFilled[i][j].Letter = chrs[j];
                        BoardFilled[i][j].Score = Program.LetterValues(BoardFilled[i][j].Letter.ToString(CultureInfo.InvariantCulture).ToCharArray().ToList())[0]; //TODO: !!
                        BoardFilled[i][j].LetterBonus = BoardFilled[i][j].LetterBonus == 0 ? 1 : BoardFilled[i][j].LetterBonus;
                        BoardFilled[i][j].WordBonus = BoardFilled[i][j].WordBonus == 0 ? 1 : BoardFilled[i][j].WordBonus;
                    }
                }
            }
        }
    }
}