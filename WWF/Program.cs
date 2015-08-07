using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace WWF
{
    class Program
    {
        private const int BoardSize = 15;

        static public void Main(string[] args)
        {
            var grid = new Board.Fill(BoardSize);
            var board = grid.BoardFilled;
            //board = GetGrid(board); //For board letter input by user

            WriteBoard(board);

            var letters = GetLetters();

            var startTime = DateTime.Now;

            var words = new List<Word>();

            words.AddRange(SquareRunThrough(board, letters, Direction.Down)); //Down words

            board = board.RotateCounterClockwise();

            words.AddRange(SquareRunThrough(board, letters, Direction.Across)); //Across words
            
            var endTime = DateTime.Now;

            var wordsOrdered = from element in words 
                               orderby element.Row, element.Column, element.WordScore descending 
                               select element;

            WriteWords(wordsOrdered.ToList());

            Console.WriteLine("Time = {0} seconds", Math.Round((endTime - startTime).TotalSeconds, 2));

            Console.ReadKey();
        }

        static Grid GetGrid(Grid board)
        {
            var letterFrequencyTotal = new List<int>(); //Cumulative letter frequency for board. Max # of each tile allowed.
            for (var row = 0; row < BoardSize; row++)
            {
                for (; ; )
                {
                    var letterFrequencyRow = new List<int>(letterFrequencyTotal);
                    
                    Console.Write("Enter Row {0} letters: ", row + 1);
                    var rowLetters = (Console.ReadLine().ToLower().ToList());

                    if (rowLetters.Count < BoardSize) //Fill rest of row with blanks
                    {
                        for (var j = rowLetters.Count; j < BoardSize; j++)
                        {
                            rowLetters.Add(Constants.Blank);
                        }
                    }

                    letterFrequencyRow = LetterFreq(rowLetters, letterFrequencyRow, BoardSize); //Get letter frequencies of current row

                    if (!LettersCheck(rowLetters, letterFrequencyRow, BoardSize)) { continue; } //Check validity of entered characters

                    letterFrequencyTotal = letterFrequencyRow;

                    for (var column = 0; column < BoardSize; column++) //Add row letters to board
                    {
                        board.GetSquare(row, column).Letter = rowLetters[column];
                    }

                    break;
                }
            }
            
            return board;
        }

        static void WriteBoard(Grid board)
        {
            Console.WriteLine(" 123456789012345");
            for (var i = 0; i < BoardSize; i++)
            {
                Console.Write("|");
                for (var j = 0; j < BoardSize - 1; j++)
                {
                    Console.Write((board.GetSquare(i, j)).Letter.ToString(CultureInfo.InvariantCulture).ToUpper());
                }
                Console.WriteLine((board.GetSquare(i, BoardSize - 1)).Letter.ToString(CultureInfo.InvariantCulture).ToUpper() + "| {0}", i + 1);
            }
        }

        static void WriteWords(List<Word> words )
        {
            foreach (var i in words)
            {
                if (i.Letters.Count > 0) //Can use to restrict results
                {
                    Console.WriteLine("({0},{1}) {2}\t {3}\t Total Score: {4}", i.Row + 1, i.Column + 1, i.Direction, string.Concat(i.Letters).ToUpper(), i.WordScore);
                }
            }
            Console.WriteLine(words.Count + " words");
        }

        static List<char> GetLetters()
        {
            List<char> letters;
            
            for (; ; )
            {
                var letFreq = new List<int>();
                Console.Write("Enter letters: ");
                letters = Console.ReadLine().ToLower().ToCharArray().ToList();
                
                if (letters.Count == 0)
                {
                    continue;
                }

                letFreq = LetterFreq(letters, letFreq, 7);

                if (!LettersCheck(letters, letFreq, 7)) { continue; }

                break;
            }
            return (letters);
        }
        
        static List<Word> SquareRunThrough(Grid board, List<char> letters, Direction direction)
        {
            var words = new List<Word>();

            var limitsLtRtUpLw = Limits(board); //Limits let code ignore checking large sections of board  
            
            for (var row = 0; row < limitsLtRtUpLw[3]; row++)
            {
                for (var column = limitsLtRtUpLw[0]; column < limitsLtRtUpLw[1] + 1; column++)
                {
                    var contactRow = BoardSize;

                    if (!CheckSquare(board, row, Math.Min(row + letters.Count - 1, BoardSize - 1), column, ref contactRow))
                    {
                        continue;
                    }

                    words.AddRange(WordSolver(board, letters, row, column, contactRow, direction));
                }
            }

            var wordsOrdered = from element in words //Order across words by square (down words ordered incidentally)
                orderby element.Row 
                select element;

            return wordsOrdered.ToList();
        }

        static List<int> Limits(Grid grid)
        {
            var lts = new List<int> { BoardSize - 1, 0, BoardSize - 1, 0 }; //Left-Column/Right-Column/Upper-Row/Lower-Row bounds

            for (var r = 0; r < BoardSize; r++)
            {
                for (var c = 0; c < BoardSize; c++)
                {
                    var chr = Regex.IsMatch(grid.GetSquare(r,c).Letter.ToString(CultureInfo.InvariantCulture), @"[a-z]");
                    if (chr && c <= lts[0]){lts[0] = Math.Max(c - 1, 0);}
                    if (chr && c >= lts[1]){lts[1] = Math.Min(c + 1, BoardSize - 1);}
                    if (chr && r <= lts[2]){lts[2] = Math.Max(r - 1, 0);}
                    if (chr && r >= lts[3]){lts[3] = Math.Min(r + 1, BoardSize - 1);}
                }
            }
            
            return lts;
        }

        static bool CheckSquare(Grid grid, int top, int bottom, int column, ref int contactRow) //Check for tiles which disqualify square (1,2), otherwise for a connection which qualifies it (3)
        {
            if ((!grid.GetSquare(Math.Max(top - 1, 0), column).IsBlank && top > 0)) // 1)Check square above for letter
                { return false;}

            for (var row = top; row < BoardSize; row++) // 2)Check for continuous column of letters to bottom (Includes bottom row)
            {
                if (grid.GetSquare(row, column).IsBlank)
                {
                    break;
                }
                if (!grid.GetSquare(row, column).IsBlank && row == BoardSize - 1)
                {
                    return false;
                }
            }

            for (var row = top; row < bottom + 1; row++) // 3)Check letters-by-3 box (squares in column and adjacent columns on either side)
            {
                if (grid.GetSquare(row, Math.Max(column - 1, 0)).Letter.ToString(CultureInfo.InvariantCulture) + grid.GetSquare(row, column).Letter + grid.GetSquare(row, Math.Min(BoardSize - 1, column + 1)).Letter != "   ")
                {
                    contactRow = row - top + 1;
                    return true;
                }
            }

            if (bottom != BoardSize - 1 && !grid.GetSquare(bottom + 1, column).IsBlank) // 3)Check square below bottom-most reach 
            {
                contactRow = bottom - top + 2;
                return true;
            }
            
            return false;
        }

        static List<Word> WordSolver(Grid grid, List<char> letters, int row, int column, int contactRow, Direction direction)
        {
            var mould = new Mould(grid, letters.Count, row, column, BoardSize); //Find 'mould' (line of empty and filled squares down or across) for current square, eg. "  C D"

            var words = new List<Word>();

            var lowerIndex = Dictionary.Lengths[Math.Max(contactRow - 2, 0)]; //Check only dictionary words of lengths in applicable range
            var upperIndex = Dictionary.Lengths[mould.Count - 1] + 1;

            for (var i = lowerIndex; i < upperIndex; i++) 
            {
                var dictionaryWord = Dictionary.Dict[i];
                if (dictionaryWord.Length > mould.Count) { continue; }

                var word = dictionaryWord.ToCharArray().ToList();
                var blankLetters = new List<char>();

                if (!mould.MouldFits(word) || !mould.WordFits(contactRow, letters, word, ref blankLetters) || !PerpendicularWords(grid, word, row, column, direction))
                {
                    continue;
                }

                //Blank tiles have 0 score. Thus when used for a letter that occurs more than once multiple words must be recored, e.g LOSSE#, LOS#ES and LO#SES
                var wordBlanks = mould.GenerateBlankVariations(word, blankLetters);

                foreach (var w in wordBlanks) //Add each word to final list
                {
                    words.Add(new Word
                    {
                        Row = row,
                        Column = column,
                        Direction = direction,
                        Letters = word,
                        Wrd = string.Concat(word),
                        Scores = LetterValues(w),
                    });
                }
            }

            for (var w = 0; w < words.Count; w++) //Score each word
            {
                words[w] = WordScorer(words[w], grid); 
                if (direction == Direction.Across) //Reset coordinates of across words due to rotation
                {
                    var temp = words[w].Row;
                    words[w].Row = BoardSize - 1 - words[w].Column;
                    words[w].Column = temp;
                }
            }

            return words;
        }

        
        static bool PerpendicularWords(Grid grid, List<char> word, int row, int column, Direction direction) //Check any perpendicular words are legal
        {
            for (var r = 0; r < word.Count; r++) //Check perp word at each letter
            {
                var perpWord = word[r].ToString(CultureInfo.InvariantCulture);
                for (var c = column - 1; c > -1; c--) //Construct perp word with letters to left
                {
                    if (grid.GetSquare(r + row, c).IsBlank) { break; }
                    perpWord = grid.GetSquare(r + row, c).Letter + perpWord;
                }
                for (var c = column + 1; c < BoardSize; c++) //Add letters to right
                {
                    if (grid.GetSquare(r + row, c).IsBlank) { break; }
                    perpWord += grid.GetSquare(r + row, c).Letter.ToString(CultureInfo.InvariantCulture);
                }

                if (perpWord.Length == 1) { continue; } //I.E. no adjacent letters

                var dictionary = direction == Direction.Down ? new List<string>(Dictionary.Dict) : new List<string>(Dictionary.DictRev); //Rotation for across words means perp words are reversed. Use dictionary with reverse words.
                var lowerIndex = Dictionary.Lengths[perpWord.Length - 2]; //Indices to only check dictionary words of exact length
                var upperIndex = Dictionary.Lengths[perpWord.Length - 1];

                for (var w = lowerIndex; w < upperIndex + 1; w++ )
                {
                    if (perpWord == dictionary[w])
                    {
                        break;
                    }
                    if (w == upperIndex && perpWord != dictionary[w] )
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        static List<int> LetterFreq(List<char> letters, List<int> letterFrequency, int length) //Returns list of letter frequencies [char, count, max allowed, char, count, max allowed, etc.]
        {
            for (var letter = 0; letter < letters.Count; letter += 1)
            {
                if ((Convert.ToInt32(letters[letter]) < 97 || Convert.ToInt32(letters[letter]) > 122) && Convert.ToInt32(letters[letter]) != 32){ continue; }
                var duplicate = 0;
                var count = 0;
                while (count < letterFrequency.Count)
                {
                    if (letterFrequency[count] == Convert.ToInt32(letters[letter]))
                    {
                        duplicate += 1;
                        break;
                    }
                    count += 3;
                }

                if (duplicate > 0)
                {
                    letterFrequency[count + 1] += 1;
                }
                else
                {
                    letterFrequency.Add(Convert.ToInt32(letters[letter]));
                    letterFrequency.Add(1);
                    letterFrequency.Add(0);
                }
            }

            for (var k = letterFrequency.Count - 3; k > -1; k -= 3)
            {
                int max;
                switch (letterFrequency[k]) //Number of each tiles allowed
                {
                    case 106: case 107: case 113: case 120: case 122:
                        max = 1; break;
                    case 32: case 98: case 99: case 102: case 109: case 112: case 118: case 119: case 121:
                        max = 2; break;
                    case 103: 
                        max = 3; break;
                    case 104: case 108: case 117:
                        max = 4; break;
                    case 100: case 110: case 115: 
                        max = 5; break;
                    case 114:
                        max = 6; break;
                    default:
                        max = -1; break;
                }

                if (length == BoardSize)
                {
                    switch (letterFrequency[k])
                    {
                        case 32:
                            max = -1; break;
                        case 116:
                            max = 7; break;
                        case 105: case 111:
                            max = 8; break;
                        case 97:
                            max = 9; break;
                        case 101:
                            max = 13; break;
                    }
                }
                
                letterFrequency[k + 2] = max;
            }

            return (letterFrequency);
        }

        static bool LettersCheck(List<char> charLetters, List<int> letterFrequency, int length) //Used for both rack tiles(7) and board row tiles(15). Provides every applicable error message.
        {
            var pass = true;

            foreach (var c in charLetters)
            {
                if ((c < 32 || 32 < c) && (c < 65 || 90 < c) && (c < 97 || c > 122))
                {
                    Console.WriteLine("Letters only!");
                    pass = false;
                    break;
                }
            }

            if (charLetters.Count > length)
            {
                if (length == 7)
                {
                    Console.WriteLine("Maximum of 7 tiles allowed!");
                }
                else if (length == BoardSize)
                {
                    Console.WriteLine("Maximum of {0} allowed!", BoardSize);
                }
                pass = false;
            }

            for (var j = 0; j < letterFrequency.Count; j += 3)
            {
                if (letterFrequency[j] < 33 && letterFrequency[j + 1] > letterFrequency[j + 2] && length == 7)
                {
                    Console.WriteLine("Maximum of 2 blank tiles allowed!");
                    pass = false;
                }
                else if ((letterFrequency[j] == 106 || letterFrequency[j] == 107 || letterFrequency[j] == 113 || letterFrequency[j] == 120 || letterFrequency[j] == 122) && letterFrequency[j + 1] > letterFrequency[j + 2])
                {
                    Console.WriteLine("Maximum of {0} {1} tile allowed!", letterFrequency[j + 2], Convert.ToChar(letterFrequency[j] - 32));
                    pass = false;
                }
                else if (((97 < letterFrequency[j]) && (letterFrequency[j] < 101) || (101 < letterFrequency[j]) && (letterFrequency[j] < 105) || (105 < letterFrequency[j]) && (letterFrequency[j] < 111) || (111 < letterFrequency[j]) && (letterFrequency[j] < 116) || letterFrequency[j] > 116) && letterFrequency[j + 1] > letterFrequency[j + 2] && length == 7)
                {
                    Console.WriteLine("Maximum of {0} {1} tiles allowed!", letterFrequency[j + 2], Convert.ToChar(letterFrequency[j] - 32));
                    pass = false;
                }
                else if (letterFrequency[j] > 32 && (letterFrequency[j + 1] > letterFrequency[j + 2]) && length == BoardSize)
                {
                    Console.WriteLine("Maximum of {0} {1} tiles allowed!", letterFrequency[j + 2], Convert.ToChar(letterFrequency[j] - 32));
                    pass = false;
                }
            }

            return pass;
        }

        public static List<int> LetterValues(List<char> word)
        {
            var letterScores = new List<int>();

            foreach (var i in word)
            {
                switch (i)
                {
                    case Constants.Blank:
                        letterScores.Add(0); break;
                    case 'a': case 'e': case 'i': case 'o': case 'r': case 's': case 't':
                        letterScores.Add(1); break;
                    case 'd': case 'l': case 'n': case 'u':
                        letterScores.Add(2); break;
                    case 'g': case 'h': case 'y':
                        letterScores.Add(3); break;
                    case 'b': case 'c': case 'f': case 'm': case 'p': case 'w':
                        letterScores.Add(4); break;
                    case 'k': case 'v':
                        letterScores.Add(5); break;
                    case 'x':
                        letterScores.Add(8); break;
                    case 'j': case 'q': case 'z':
                        letterScores.Add(10); break;
                }
            }
            return letterScores;
        }

        static Word WordScorer(Word word, Grid grid)
        {
            var totalScore = 0;
            var totalWordMultiplier = 1; //Total word multiplier for base word score
            var baseScore = 0; //Basic score of actual word before final word multiplication
            var letterCount = 0; //Check if all letters are used for 35 bonus

            for (var r = 0; r < word.Letters.Count; r++)
            {
                var letterMultiplier = grid.GetSquare(r + word.Row, word.Column).IsBlank ? grid.GetSquare(r + word.Row, word.Column).LetterBonus : 1; //Determine if square letter muliplier is applicable
                var wordMultiplier = grid.GetSquare(r + word.Row, word.Column).IsBlank ? grid.GetSquare(r + word.Row, word.Column).WordBonus : 1; //Determine if square word multiplier is applicable

                totalWordMultiplier *= wordMultiplier; //Add square word multiplier to total word multiplier

                baseScore += word.Scores[r] * letterMultiplier; //Add current letter score to base word score

                if (!grid.GetSquare(r + word.Row, word.Column).IsBlank) { continue; } //Existing letter - perpendicular word doesn't score

                letterCount++;

                var crossWord = word.Scores[r] * letterMultiplier; //Letter value times letter bonus

                var count = 0; //Count of perpendicular tiles. Needed due to potential blank tiles with 0 score.
                for (var c = word.Column - 1; c > -1; c--) //Add letter scores for perp word to left
                {
                    if (grid.GetSquare(r + word.Row, c).IsBlank) { break; }
                    crossWord += grid.GetSquare(r + word.Row, c).Score;
                    count++;
                }
                for (var c = word.Column + 1; c < BoardSize; c++) //Add letter scores for perp word to right
                {
                    if (grid.GetSquare(r + word.Row, c).IsBlank) { break; }
                    crossWord += grid.GetSquare(r + word.Row, c).Score;
                    count++;
                }
                if (count == 0) { continue; } //No perpendicular word  

                totalScore += crossWord * wordMultiplier; //Add each perp word score to total score
            }

            totalScore += baseScore * totalWordMultiplier; //Add base word score to total score
            if (letterCount == 7) { totalScore += 35; }
            word.WordScore = totalScore;

            return word;
        }
    }
}
