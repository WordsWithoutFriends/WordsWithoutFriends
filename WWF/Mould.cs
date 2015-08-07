using System.Collections.Generic;
using System.Linq;

namespace WWF
{
    public class Mould
    {
        private readonly List<char> _mould; 
        
        public Mould(Grid grid, int letterCount, int row, int column, int boardSize)
        {
            _mould = new List<char>();

            for (var r = row; r < boardSize; r++)
            {
                _mould.Add(grid.GetSquare(r, column).Letter);
            }

            var blanks = _mould.Count(i => i == Constants.Blank);

            if (blanks > letterCount)
            {
                for (var r = _mould.Count - 1; r > -1; r--)
                {
                    if (blanks == letterCount) { break; }
                    if (_mould[r] == Constants.Blank)
                    {
                        _mould.RemoveAt(r);
                        blanks--;
                        continue;
                    }

                    _mould.RemoveAt(r);
                }
            }
        }

        public int Count { get { return _mould != null ? _mould.Count : 0; } }

        public bool MouldFits(List<char> word) //Quick check that letters in mould correspond with letters in word
        {
            for (var j = 0; j < word.Count; j++)
            {
                if (_mould[j] != Constants.Blank && _mould[j] != word[j])
                {
                    return false;
                }
            }
            return true;
        }

        public bool WordFits(int contactRow, List<char> letters, List<char> word, ref List<char> blankLetters) //Check word can be constructed from tiles and legally fits mould
        {
            var count = word.Count;
            var connection = false;
            var tempLetters = new List<char>(letters);

            for (var letter = 0; letter < word.Count; letter++) //Check each letter in word
            {
                if (word[letter] == _mould[letter]) //Check if letter is already on the board
                {
                    count--;
                    connection = true;
                    continue;
                }

                var str = string.Concat(tempLetters);
                if (!str.Contains(word[letter])) //Check if letter exists in rack tiles
                {
                    if (str.Contains(Constants.Blank)) //If not, check if blank tile exists to represent letter
                    {
                        count--;
                        tempLetters.Remove(Constants.Blank);
                        blankLetters.Add(word[letter]); //blankLetters is a record of letters represented by blank tile/s
                        continue;
                    }
                    return false; //If word can't be constructed
                }

                tempLetters.Remove(word[letter]);
                count--;
            }


            if (tempLetters.Count == letters.Count) { return false; } //Discount existing words on the board where no rack letters are used

            if (word.Count >= contactRow) //Check for connection to adjacent columns
            {
                connection = true;
            }

            if (_mould.Count > word.Count) //Check for subsequent letters after word, eg. CAT not acceptable in mould _A_M
            {
                if (_mould[word.Count] != Constants.Blank)
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

        public List<List<char>> GenerateBlankVariations(List<char> word, List<char> blanks)
        {
            var words = new List<List<char>>();

            if (blanks.Count == 0) //No variations with 0 blank tiles
            {
                words.Add(word);
                return words;
            }

            var iteration = 1;

            for (var b = 0; b < blanks.Count; b++) //Do once or twice depending on 1 or 2 blank tiles used
            {
                for (var y = 0; y < iteration; y++)
                {
                    var wrd = b == 1 ? new List<char>(words[y]) : new List<char>(word);

                    for (var c = wrd.Count - 1; c > -1; c--) //Remove all letters but those of blank tile, excluding tiles that already exist, eg. aaa resulting from ALABAS###S, aa from ALAB###ERS
                    {
                        if (wrd[c] == blanks[b] && wrd[c] != _mould[c])
                        {
                            continue;
                        }
                        wrd.RemoveAt(c);
                    }

                    for (var l = 0; l < wrd.Count; l++) //Put blank tile in place of each letter occurence, eg. aaa to *aa, then aaa to a*a etc. (done at 2nd comment down)
                    {
                        var wd = b == 1 ? new List<char>(words[y]) : new List<char>(word);

                        for (var c = 0; c < wd.Count; c++) //Word without mould letters, eg. ALABAS###S
                        {
                            wd[c] = word[c] == _mould[c] ? Constants.Blank : wd[c];
                        }

                        var lts = new List<char>(wrd);
                        lts[l] = Constants.Blank; //Replace tile instance with ' '
                        var indices = new List<int>();

                        for (var ind = 0; ind < lts.Count; ind++) //Find indices of duplicate tiles to place back in, eg. [0,2,4] for As in ALABASTERS
                        {
                            var index = wd.FindIndex(x => x == blanks[b]);
                            indices.Add(index);
                            wd[index] = Constants.Blank;
                        }

                        for (var i = 0; i < lts.Count; i++) //Reinsert current duplicate scheme, eg. *LABAS###S, AL*BAS###S, ALAB*S###S
                        {
                            wd[indices[i]] = lts[i];
                        }

                        for (var i = 0; i < word.Count; i++) //Reinsert mould letters, eg. *LABASTERS, AL*BASTERS, ALAB*STERS. End up with three variations of ALABASTERS with ' ' at different occurences of 'A'
                        {
                            if (_mould[i] != Constants.Blank)
                            {
                                wd[i] = _mould[i];
                            }
                        }

                        words.Add(wd);
                    }
                }

                iteration = words.Count; //Needed when there are two blank tiles utilised
            }

            for (var w = words.Count - 1; w > -1; w--) //Remove duplicate words
            {
                if (words[w].Count(x => x == Constants.Blank) < blanks.Count)
                {
                    words.RemoveAt(w);
                    continue;
                }

                for (var x = w - 1; x > -1; x--)
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
    }
}
