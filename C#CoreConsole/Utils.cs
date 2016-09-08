using System;

namespace ConsoleApplication
{

    public delegate bool Parse<T>(string t, out T i);
    public delegate void Writer(string s);
    public delegate string Reader();
    public delegate ConsoleKeyInfo Key();

    public static class U
    {
        public static int GlobalPort = 7777;
        public static int PrivatePort = 6666;
        public static int NameLength = 15;
        public static Parse<int> ParseInt = int.TryParse;
        public static Parse<long> ParseLong = long.TryParse;
        public static Tuple<ConsoleColor, ConsoleColor>[] Colors = 
        {  
            new Tuple<ConsoleColor,ConsoleColor>(ConsoleColor.Black, ConsoleColor.White), 
            new Tuple<ConsoleColor,ConsoleColor>(ConsoleColor.Black, ConsoleColor.Red),
            new Tuple<ConsoleColor,ConsoleColor>(ConsoleColor.Black, ConsoleColor.Gray),
            new Tuple<ConsoleColor,ConsoleColor>(ConsoleColor.Black, ConsoleColor.Yellow),
            new Tuple<ConsoleColor,ConsoleColor>(ConsoleColor.Black, ConsoleColor.Green)
        };

    }

    public static class C 
    {
        public static Writer Display = Console.WriteLine;
        public static Writer Write = Console.Write;
        public static Reader Read = Console.ReadLine;
        public static Key Key = Console.ReadKey;  
    }

}