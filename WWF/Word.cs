using System.Collections.Generic;

namespace WWF
{
    public class Word
    {
        public int Row { get; set; }

        public int Column { get; set; }

        public Direction Direction { get; set; }

        public List<char> Letters { get; set; }

        public string Wrd { get; set; }

        public List<int> Scores { get; set; }

        public int WordScore { get; set; }
    }
}