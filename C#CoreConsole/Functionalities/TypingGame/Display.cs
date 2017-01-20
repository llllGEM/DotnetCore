using System;
using System.Linq;

namespace ConsoleApplication.Functions.TypingGame
{
    public static class Display
    {
        public static void ResetCursor(string word)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            C.Cursor(Console.WindowWidth/2-10, Console.WindowHeight/2);
            C.Write(word.PadRight(20, ' '));
            C.Cursor(Console.WindowWidth/2-10, Console.WindowHeight/2);
        }

        public static void ValidateChar(string s)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            C.Write(s);
        }

        public static void Score()
        {
            var scores = TypingGame.Scores;
            if(scores.Count == 0) return;
            var top = Console.CursorTop;
            C.WL(" ");
            foreach(var s in scores)
                C.WL(scores.IndexOf(s).ToString().PadRight(5)+" "+s);
            var avg = TimeSpan.FromSeconds(scores.Average(s => s.Seconds));
            C.WL($"Average: {avg}");
            C.Cursor(Console.CursorLeft, top);
        }
    }
}