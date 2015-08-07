using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WWF
{
    static class Dictionary
    {
        static string txt = Properties.Resources.Dictionary___Lengths;
        static string txtRev = Properties.Resources.Dictionary___Lengths___Reverse;
        public static List<string> Dict = Regex.Split(txt, "\r\n").ToList();
        public static List<string> DictRev = Regex.Split(txtRev, "\r\n").ToList();
        public static List<int> Lengths = new List<int>{0, 95, 1067, 4970, 13606, 28838, 51947, 80367, 105240, 125540, 141044, 152401, 160228, 165355, 168547}; //Index of last 2-letter word (95), last 3-letter word (1067) etc. to last 15-letter word
    }
}