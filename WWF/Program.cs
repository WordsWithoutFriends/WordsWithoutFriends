using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WWF
{
    class Program
    {
        static public void Main(string[] args)
        {
            Board.Fill grid = new Board.Fill();
            List<List<Square>> board = grid.BoardFilled;
            //board = GetGrid(board); //For board letter input by user

            WriteBoard(board);

            List<char> letters = GetLetters();

            DateTime startTime = DateTime.Now;

            var words = new List<Word>();

            words.AddRange(SquareRunThrough(board, letters, "Down")); //Down words

            board = Rotate(board);

            words.AddRange(SquareRunThrough(board, letters, "Across")); //Across words
            
            DateTime endTime = DateTime.Now;

            var wordsOrdered = from element in words 
                               orderby element.Row, element.Column, element.WordScore descending 
                               select element;

            WriteWords(wordsOrdered.ToList());

            Console.WriteLine("Time = {0} seconds", Math.Round((endTime - startTime).TotalSeconds, 2));

            Console.ReadKey();
        }

        static List<List<Square>> GetGrid(List<List<Square>> board )
        {
            var letterFrequencyTotal = new List<int>(); //Cumulative letter frequency for board. Max # of each tile allowed.
            for (int row = 0; row < 15; row++)
            {
                for (; ; )
                {
                    var letterFrequencyRow = new List<int>(letterFrequencyTotal);
                    
                    Console.Write("Enter Row {0} letters: ", row + 1);
                    List<char> rowLetters = (Console.ReadLine().ToLower().ToList());

                    if (rowLetters.Count < 15) //Fill rest of row with blanks
                    {
                        for (int j = rowLetters.Count; j < 15; j++)
                        {
                            rowLetters.Add(' ');
                        }
                    }

                    letterFrequencyRow = LetterFreq(rowLetters, letterFrequencyRow, 15); //Get letter frequencies of current row

                    if (!LettersCheck(rowLetters, letterFrequencyRow, 15)) { continue; } //Check validity of entered characters

                    letterFrequencyTotal = letterFrequencyRow;

                    for (int column = 0; column < 15; column++) //Add row letters to board
                    {
                        board[row][column].Letter = rowLetters[column];
                    }

                    break;
                }
            }
            
            return board;
        }

        static void WriteBoard(List<List<Square>> board)
        {
            Console.WriteLine(" 123456789012345");
            for (int i = 0; i < 15; i++)
            {
                Console.Write("|");
                for (int j = 0; j < 14; j++)
                {
                    Console.Write((board[i][j]).Letter.ToString().ToUpper());
                }
                Console.WriteLine((board[i][14]).Letter.ToString().ToUpper() + "| {0}", i + 1);
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
        
        static List<Word> SquareRunThrough(List<List<Square>> board, List<char> letters, string direction )
        {
            var words = new List<Word>();

            List<int> limitsLtRtUpLw = Limits(board); //Limits let code ignore checking large sections of board  
            
            for (int row = 0; row < limitsLtRtUpLw[3]; row++)
            {
                for (int column = limitsLtRtUpLw[0]; column < limitsLtRtUpLw[1] + 1; column++)
                {
                    int contactRow = 15;

                    if (!CheckSquare(board, row, Math.Min(row + letters.Count - 1, 14), column, ref contactRow))
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

        static List<int> Limits(List<List<Square>> grid )
        {
            var lts = new List<int> { 14, 0, 14, 0 }; //Left-Column/Right-Column/Upper-Row/Lower-Row bounds
            
            for (int r = 0; r < 15; r++)
            {
                for (int c = 0; c < 15; c++)
                {
                    bool chr = Regex.IsMatch(grid[r][c].Letter.ToString(), @"[a-z]");
                    if (chr && c <= lts[0]){lts[0] = Math.Max(c - 1, 0);}
                    if (chr && c >= lts[1]){lts[1] = Math.Min(c + 1, 14);}
                    if (chr && r <= lts[2]){lts[2] = Math.Max(r - 1, 0);}
                    if (chr && r >= lts[3]){lts[3] = Math.Min(r + 1, 14);}
                }
            }
            
            return lts;
        }

        static bool CheckSquare(List<List<Square>> grid, int top, int bottom, int column, ref int contactRow) //Check for tiles which disqualify square (1,2), otherwise for a connection which qualifies it (3)
        {
            if ((grid[Math.Max(top - 1, 0)][column].Letter != ' ' && top > 0)) // 1)Check square above for letter
                { return false;}
            
            for (int row = top; row < 15; row++) // 2)Check for continuous column of letters to bottom (Includes bottom row)
            {
                if (grid[row][column].Letter == ' ')
                {
                    break;
                }
                if (grid[row][column].Letter != ' ' && row == 14)
                {
                    return false;
                }
            }

            for (int row = top; row < bottom + 1; row++) // 3)Check letters-by-3 box (squares in column and adjacent columns on either side)
            {
                if (grid[row][Math.Max(column - 1, 0)].Letter.ToString() + grid[row][column].Letter + grid[row][Math.Min(14, column + 1)].Letter != "   ")
                {
                    contactRow = row - top + 1;
                    return true;
                }
            }

            if (bottom != 14 && grid[bottom + 1][column].Letter != ' ') // 3)Check square below bottom-most reach 
            {
                contactRow = bottom - top + 2;
                return true;
            }
            
            return false;
        }

        static List<List<Square>> Rotate(List<List<Square>> grid)
        {
            List<List<Square>> rotatedGrid = new Board().BoardEmpty;

            for (int r = 0; r < 15; r++)
            {
                for (int c = 0; c < 15; c++)
                {
                    rotatedGrid[r][c] = grid[14 - c][r];
                }
            }

            return rotatedGrid;
        }

        static List<Word> WordSolver(List<List<Square>> grid, List<char> letters, int row, int column, int contactRow, string direction)
        {
            List<char> mould = Mould(grid, letters.Count, row, column); //Find 'mould' (line of empty and filled squares down or across) for current square, eg. "  C D"

            var words = new List<Word>();

            int lowerIndex = Dictionary.Lengths[Math.Max(contactRow - 2, 0)]; //Check only dictionary words of lengths in applicable range
            int upperIndex = Dictionary.Lengths[mould.Count - 1] + 1;

            for (int i = lowerIndex; i < upperIndex; i++) 
            {
                string dictionaryWord = Dictionary.Dict[i];
                if (dictionaryWord.Length > mould.Count) { continue; }

                List<char> word = dictionaryWord.ToCharArray().ToList();
                List<char> blankLetters = new List<char>();

                if (!MouldFit(mould, word) || !WordFit(mould, contactRow, letters, word, ref blankLetters) || !PerpendicularWords(grid, word, row, column, direction))
                {
                    continue;
                }

                //Blank tiles have 0 score. Thus when used for a letter that occurs more than once multiple words must be recored, e.g LOSSE#, LOS#ES and LO#SES
                List<List<char>> wordBlanks = BlankVariations(mould, word, blankLetters);

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

            for (int w = 0; w < words.Count; w++) //Score each word
            {
                words[w] = WordScorer(words[w], grid); 
                if (direction == "Across") //Reset coordinates of across words due to rotation
                {
                    int temp = words[w].Row;
                    words[w].Row = 14 - words[w].Column;
                    words[w].Column = temp;
                }
            }

            return words;
        }

        static List<char> Mould(List<List<Square>> grid, int letterCount, int row, int column )
        {
            var mould = new List<char>();

            for (int r = row; r < 15; r++)
            {
                mould.Add(grid[r][column].Letter);
            }

            int blanks = mould.Count(i => i == ' ');

            if (blanks > letterCount)
            {
                for (int r = mould.Count - 1; r > -1; r--)
                {
                    if (blanks == letterCount) { break; }
                    if (mould[r] == ' ')
                    {
                        mould.RemoveAt(r);
                        blanks--;
                        continue;
                    }

                    mould.RemoveAt(r);
                }
            }

            return mould;
        }
        
        static bool MouldFit(List<char> mould, List<char> word) //Quick check that letters in mould correspond with letters in word
        {
            for (int j = 0; j < word.Count; j++)
            {
                if (mould[j] != ' ' && mould[j] != word[j])
                {
                    return false;
                }
            }
            return true;
        }

        static bool WordFit(List<char> mould, int contactRow, List<char> letters, List<char> word, ref List<char> blankLetters ) //Check word can be constructed from tiles and legally fits mould
        {
            int count = word.Count;
            bool connection = false;
            var tempLetters = new List<char>(letters);

            for (int letter = 0; letter < word.Count; letter++) //Check each letter in word
            {
                if (word[letter] == mould[letter]) //Check if letter is already on the board
                {
                    count --;
                    connection = true;
                    continue;
                }

                var str = string.Concat(tempLetters);
                if (!str.Contains(word[letter])) //Check if letter exists in rack tiles
                {
                    if (str.Contains(' ')) //If not, check if blank tile exists to represent letter
                    {
                        count --;
                        tempLetters.Remove(' ');
                        blankLetters.Add(word[letter]); //blankLetters is a record of letters represented by blank tile/s
                        continue;
                    }
                    return false; //If word can't be constructed
                }

                tempLetters.Remove(word[letter]);
                count --;
            }


            if (tempLetters.Count == letters.Count) { return false; } //Discount existing words on the board where no rack letters are used
            
            if (word.Count >= contactRow) //Check for connection to adjacent columns
            {
                connection = true;
            }
            
            if (mould.Count > word.Count) //Check for subsequent letters after word, eg. CAT not acceptable in mould _A_M
            {
                if (mould[word.Count] != ' ')
                {
                    connection = false;
                }
            }

            if (count == 0 && connection) //All letters of word covered and word connects somewhere with existing board tiles
            {
                return true;
            }

            return false;
        }
        
        static bool PerpendicularWords(List<List<Square>> grid, List<char> word, int row, int column, string direction ) //Check any perpendicular words are legal
        {
            for (int r = 0; r < word.Count; r++) //Check perp word at each letter
            {
                string perpWord = word[r].ToString();
                for (int c = column - 1; c > -1; c--) //Construct perp word with letters to left
                {
                    if (grid[r + row][c].Letter == ' ') { break; }
                    perpWord = grid[r + row][c].Letter.ToString() + perpWord;
                }
                for (int c = column + 1; c < 15; c++) //Add letters to right
                {
                    if (grid[r + row][c].Letter == ' ') { break; }
                    perpWord += grid[r + row][c].Letter.ToString();
                }

                if (perpWord.Length == 1) { continue; } //I.E. no adjacent letters

                List<string> dictionary = direction == "Down" ? new List<string>(Dictionary.Dict) : new List<string>(Dictionary.DictRev); //Rotation for across words means perp words are reversed. Use dictionary with reverse words.
                int lowerIndex = Dictionary.Lengths[perpWord.Length - 2]; //Indices to only check dictionary words of exact length
                int upperIndex = Dictionary.Lengths[perpWord.Length - 1];

                for (int w = lowerIndex; w < upperIndex + 1; w++ )
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

        static List<List<char>> BlankVariations(List<char> mould, List<char> word, List<char> blanks)
        {
            var words = new List<List<char>>();

            if (blanks.Count == 0) //No variations with 0 blank tiles
            {
                words.Add(word);
                return words;
            }

            int iteration = 1;

            for (int b = 0; b < blanks.Count; b++) //Do once or twice depending on 1 or 2 blank tiles used
            {
                for (int y = 0; y < iteration; y++) 
                {
                    List<char> wrd = b == 1 ? new List<char>(words[y]) : new List<char>(word);
                    
                    for (int c = wrd.Count - 1; c > -1; c--) //Remove all letters but those of blank tile, excluding tiles that already exist, eg. aaa resulting from ALABAS###S, aa from ALAB###ERS
                    {
                        if (wrd[c] == blanks[b] && wrd[c] != mould[c])
                        {
                            continue;
                        }
                        wrd.RemoveAt(c);
                    }

                    for (int l = 0; l < wrd.Count; l++) //Put blank tile in place of each letter occurence, eg. aaa to *aa, then aaa to a*a etc. (done at 2nd comment down)
                    {
                        List<char> wd = b == 1 ? new List<char>(words[y]) : new List<char>(word);

                        for (int c = 0; c < wd.Count; c++) //Word without mould letters, eg. ALABAS###S
                        {
                            wd[c] = word[c] == mould[c] ? ' ' : wd[c];
                        }

                        var lts = new List<char>(wrd);
                        lts[l] = ' '; //Replace tile instance with ' '
                        var indices = new List<int>();

                        for (int ind = 0; ind < lts.Count; ind++) //Find indices of duplicate tiles to place back in, eg. [0,2,4] for As in ALABASTERS
                        {
                            int index = wd.FindIndex(x => x == blanks[b]);
                            indices.Add(index);
                            wd[index] = ' ';
                        }

                        for (int i = 0; i < lts.Count; i++) //Reinsert current duplicate scheme, eg. *LABAS###S, AL*BAS###S, ALAB*S###S
                        {
                            wd[indices[i]] = lts[i];
                        }

                        for (int i = 0; i < word.Count; i++) //Reinsert mould letters, eg. *LABASTERS, AL*BASTERS, ALAB*STERS. End up with three variations of ALABASTERS with ' ' at different occurences of 'A'
                        {
                            if (mould[i] != ' ')
                            {
                                wd[i] = mould[i];
                            }
                        }

                        words.Add(wd);
                    }
                }

                iteration = words.Count; //Needed when there are two blank tiles utilised
            }
            
            for (int w = words.Count - 1; w > -1; w--) //Remove duplicate words
            {
                if (words[w].Count(x => x == ' ') < blanks.Count) 
                {
                    words.RemoveAt(w);
                    continue;
                }

                for (int x = w - 1; x > -1; x--)
                {
                    if (string.Concat(words[x]) == string.Concat(words[w]))
                    {
                        words.RemoveAt(w);
                        break;
                    }
                }
            }
          
            return words;
        }
        
        static List<int> LetterFreq(List<char> letters, List<int> letterFrequency, int length) //Returns list of letter frequencies [char, count, max allowed, char, count, max allowed, etc.]
        {
            for (int letter = 0; letter < letters.Count; letter += 1)
            {
                if ((Convert.ToInt32(letters[letter]) < 97 || Convert.ToInt32(letters[letter]) > 122) && Convert.ToInt32(letters[letter]) != 32){ continue; }
                int duplicate = 0;
                int count = 0;
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

            for (int k = letterFrequency.Count - 3; k > -1; k -= 3)
            {
                int max = 0;
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

                if (length == 15)
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
            bool pass = true;

            foreach (char c in charLetters)
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
                else if (length == 15)
                {
                    Console.WriteLine("Maximum of 15 allowed!");
                }
                pass = false;
            }

            for (int j = 0; j < letterFrequency.Count; j += 3)
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
                else if (letterFrequency[j] > 32 && (letterFrequency[j + 1] > letterFrequency[j + 2]) && length == 15)
                {
                    Console.WriteLine("Maximum of {0} {1} tiles allowed!", letterFrequency[j + 2], Convert.ToChar(letterFrequency[j] - 32));
                    pass = false;
                }
            }

            return pass;
        }

        public static List<int> LetterValues(List<char> word)
        {
            List<int> letterScores = new List<int>();

            foreach (char i in word)
            {
                switch (i)
                {
                    case ' ':
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

        static Word WordScorer(Word word, List<List<Square>> grid)
        {
            int totalScore = 0;
            int totalWordMultiplier = 1; //Total word multiplier for base word score
            int baseScore = 0; //Basic score of actual word before final word multiplication
            int letterCount = 0; //Check if all letters are used for 35 bonus

            for (int r = 0; r < word.Letters.Count; r++)
            {
                int letterMultiplier = grid[r + word.Row][word.Column].Letter == ' ' ? grid[r + word.Row][word.Column].LetterBonus : 1; //Determine if square letter muliplier is applicable
                int wordMultiplier = grid[r + word.Row][word.Column].Letter == ' ' ? grid[r + word.Row][word.Column].WordBonus : 1; //Determine if square word multiplier is applicable

                totalWordMultiplier *= wordMultiplier; //Add square word multiplier to total word multiplier

                baseScore += word.Scores[r] * letterMultiplier; //Add current letter score to base word score

                if (grid[r + word.Row][word.Column].Letter != ' ') { continue; } //Existing letter - perpendicular word doesn't score

                letterCount++;

                int crossWord = word.Scores[r] * letterMultiplier; //Letter value times letter bonus

                int count = 0; //Count of perpendicular tiles. Needed due to potential blank tiles with 0 score.
                for (int c = word.Column - 1; c > -1; c--) //Add letter scores for perp word to left
                {
                    if (grid[r + word.Row][c].Letter == ' ') { break; }
                    crossWord += grid[r + word.Row][c].Score;
                    count++;
                }
                for (int c = word.Column + 1; c < 15; c++) //Add letter scores for perp word to right
                {
                    if (grid[r + word.Row][c].Letter == ' ') { break; }
                    crossWord += grid[r + word.Row][c].Score;
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
    
    class Board
    {
        public List<List<Square>> BoardEmpty = new List<List<Square>>{
               new List<Square>{new Square(), new Square(), new Square(), new Square(){WordBonus = 3}, new Square(), new Square(), new Square(){LetterBonus = 3}, new Square(), new Square(){LetterBonus = 3}, new Square(), new Square(), new Square(){WordBonus = 3}, new Square(), new Square(), new Square()},
               new List<Square>{new Square(), new Square(), new Square(){LetterBonus = 2}, new Square(), new Square(), new Square(){WordBonus = 2}, new Square(), new Square(), new Square(), new Square(){WordBonus = 2}, new Square(), new Square(), new Square(){LetterBonus = 2}, new Square(), new Square()},
               new List<Square>{new Square(), new Square(){LetterBonus = 2}, new Square(), new Square(), new Square(){LetterBonus = 2}, new Square(), new Square(), new Square(), new Square(), new Square(), new Square(){LetterBonus = 2}, new Square(), new Square(), new Square(){LetterBonus = 2}, new Square()},
               new List<Square>{new Square(){WordBonus = 3}, new Square(), new Square(), new Square(){LetterBonus = 3}, new Square(), new Square(), new Square(), new Square(){WordBonus = 2}, new Square(), new Square(), new Square(), new Square(){LetterBonus = 3}, new Square(), new Square(), new Square(){WordBonus = 3}},
               new List<Square>{new Square(), new Square(), new Square(){LetterBonus = 2}, new Square(), new Square(), new Square(), new Square(){LetterBonus = 2}, new Square(), new Square(){LetterBonus = 2}, new Square(), new Square(), new Square(), new Square(){LetterBonus = 2}, new Square(), new Square()},
               new List<Square>{new Square(), new Square(){WordBonus = 2}, new Square(), new Square(), new Square(), new Square(){LetterBonus = 3}, new Square(), new Square(), new Square(), new Square(){LetterBonus = 3}, new Square(), new Square(), new Square(), new Square(){WordBonus = 2}, new Square()},
               new List<Square>{new Square(){LetterBonus = 3}, new Square(), new Square(), new Square(), new Square(){LetterBonus = 2}, new Square(), new Square(), new Square(), new Square(), new Square(), new Square(){LetterBonus = 2}, new Square(), new Square(), new Square(), new Square(){LetterBonus = 3}},
               new List<Square>{new Square(), new Square(), new Square(), new Square(){WordBonus = 2}, new Square(), new Square(), new Square(), new Square(), new Square(), new Square(), new Square(), new Square(){WordBonus = 2}, new Square(), new Square(), new Square()},
               new List<Square>{new Square(){LetterBonus = 3}, new Square(), new Square(), new Square(), new Square(){LetterBonus = 2}, new Square(), new Square(), new Square(), new Square(), new Square(), new Square(){LetterBonus = 2}, new Square(), new Square(), new Square(), new Square(){LetterBonus = 3}},
               new List<Square>{new Square(), new Square(){WordBonus = 2}, new Square(), new Square(), new Square(), new Square(){LetterBonus = 3}, new Square(), new Square(), new Square(), new Square(){LetterBonus = 3}, new Square(), new Square(), new Square(), new Square(){WordBonus = 2}, new Square()},
               new List<Square>{new Square(), new Square(), new Square(){LetterBonus = 2}, new Square(), new Square(), new Square(), new Square(){LetterBonus = 2}, new Square(), new Square(){LetterBonus = 2}, new Square(), new Square(), new Square(), new Square(){LetterBonus = 2}, new Square(), new Square()},
               new List<Square>{new Square(){WordBonus = 3}, new Square(), new Square(), new Square(){LetterBonus = 3}, new Square(), new Square(), new Square(), new Square(){WordBonus = 2}, new Square(), new Square(), new Square(), new Square(){LetterBonus = 3}, new Square(), new Square(), new Square(){WordBonus = 3}},
               new List<Square>{new Square(), new Square(){LetterBonus = 2}, new Square(), new Square(), new Square(){LetterBonus = 2}, new Square(), new Square(), new Square(), new Square(), new Square(), new Square(){LetterBonus = 2}, new Square(), new Square(), new Square(){LetterBonus = 2}, new Square()},
               new List<Square>{new Square(), new Square(), new Square(){LetterBonus = 2}, new Square(), new Square(), new Square(){WordBonus = 2}, new Square(), new Square(), new Square(), new Square(){WordBonus = 2}, new Square(), new Square(), new Square(){LetterBonus = 2}, new Square(), new Square()},
               new List<Square>{new Square(), new Square(), new Square(), new Square(){WordBonus = 3}, new Square(), new Square(), new Square(){LetterBonus = 3}, new Square(), new Square(){LetterBonus = 3}, new Square(), new Square(), new Square(){WordBonus = 3}, new Square(), new Square(), new Square()},
            };

        public class Fill : Board
        {
            public List<List<Square>> BoardFilled = new List<List<Square>>();

            public Fill()
            {
                BoardFilled = BoardEmpty;

                List<string> letters = new List<string>
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

                for (int i = 0; i < 15; i++)
                {
                    List<char> chrs = letters[i].ToCharArray().ToList();
                    for (int j = 0; j < 15; j++)
                    {
                        BoardFilled[i][j].Row = i;
                        BoardFilled[i][j].Column = j;
                        BoardFilled[i][j].Letter = chrs[j];
                        BoardFilled[i][j].Score = Program.LetterValues(BoardFilled[i][j].Letter.ToString().ToCharArray().ToList())[0]; //TODO: !!
                        BoardFilled[i][j].LetterBonus = BoardFilled[i][j].LetterBonus == 0 ? 1 : BoardFilled[i][j].LetterBonus;
                        BoardFilled[i][j].WordBonus = BoardFilled[i][j].WordBonus == 0 ? 1 : BoardFilled[i][j].WordBonus;
                    }
                }
            }
        }
    }

    public class Square
    {
        public int Row { get; set; }

        public int Column { get; set; }

        public char Letter { get; set; }

        public int Score { get; set; }

        public int LetterBonus { get; set; }

        public int WordBonus { get; set; }
    }

    public class Word
    {
        public int Row { get; set; }

        public int Column { get; set; }

        public string Direction { get; set; }

        public List<char> Letters { get; set; }

        public string Wrd { get; set; }

        public List<int> Scores { get; set; }

        public int WordScore { get; set; }
    }

    static class Dictionary
    {
        static string txt = Properties.Resources.Dictionary___Lengths;
        static string txtRev = Properties.Resources.Dictionary___Lengths___Reverse;
        public static List<string> Dict = Regex.Split(txt, "\r\n").ToList();
        public static List<string> DictRev = Regex.Split(txtRev, "\r\n").ToList();
        public static List<int> Lengths = new List<int>{0, 95, 1067, 4970, 13606, 28838, 51947, 80367, 105240, 125540, 141044, 152401, 160228, 165355, 168547}; //Index of last 2-letter word (95), last 3-letter word (1067) etc. to last 15-letter word
    }
}
