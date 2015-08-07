namespace WWF
{
    static class LetterScorer
    {
        public static int GetScore(char letter)
        {
            switch (letter)
            {
                case 'a':
                case 'e':
                case 'i':
                case 'o':
                case 'r':
                case 's':
                case 't':
                    return 1;
                case 'd':
                case 'l':
                case 'n':
                case 'u':
                    return 2;
                case 'g':
                case 'h':
                case 'y':
                    return 3;
                case 'b':
                case 'c':
                case 'f':
                case 'm':
                case 'p':
                case 'w':
                    return 4;
                case 'k':
                case 'v':
                    return 5;
                case 'x':
                    return 8;
                case 'j':
                case 'q':
                case 'z':
                    return 10;
                default:
                    return 0;
            }
        }
    }
}
