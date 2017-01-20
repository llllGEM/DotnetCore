using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ConsoleApplication.Functions.Chat;

namespace ConsoleApplication
{
    public delegate bool Parse<T>(string t, out T i);
    public delegate void Writer(string s);
    public delegate void CharWriter(char c);
    public delegate string Reader();
    public delegate ConsoleKeyInfo Key();
    public delegate void Cursor(int left, int top);
    public delegate int Incrementer(int i, out int j);

    public static class U //Utils
    {
        public static bool OSX = false;
        public static int GlobalPort = 1111;
        public static int PrivatePort = 9999;
        public static int NameLength = 15;
        public static Parse<int> ParseInt = int.TryParse;
        public static Parse<long> ParseLong = long.TryParse;
        public static Tuple<ConsoleColor, ConsoleColor>[] Colors = 
        {  
            new Tuple<ConsoleColor,ConsoleColor>(ConsoleColor.Black, ConsoleColor.White), 
            new Tuple<ConsoleColor,ConsoleColor>(ConsoleColor.Black, ConsoleColor.Red),
            new Tuple<ConsoleColor,ConsoleColor>(ConsoleColor.Black, ConsoleColor.Gray),
            new Tuple<ConsoleColor,ConsoleColor>(ConsoleColor.Black, ConsoleColor.Yellow),
            new Tuple<ConsoleColor,ConsoleColor>(ConsoleColor.Black, ConsoleColor.Green),
            new Tuple<ConsoleColor,ConsoleColor>(ConsoleColor.Black, ConsoleColor.Blue)
        };

        public static List<string> CS_Keywords = new List<string>()
        {
            "abstract", "as", "async", "await", 
            "base", "bool", "break", 
            "case", "catch", "checked", "continue",
            "char", "class", "const",
            "default", "delegate", "decimal", "do", "double",
            "dynamic",
            "else","event", "explicit", "extern", "enum",
            "false", "finally", "fixed", "for", "foreach",
            "float",
            "goto", "get",
            "if", "implicit", "in", "interface", "internal", 
            "is", "int",
            "lock", "long",
            "namespace", "new", "null", 
            "object", "operator", "out", "override", 
            "params","private", "protected", "public",
            "readonly", "ref", "return",
            "sealed", "sizeof", "stackalloc", "static", "string",
            "switch", "sbyte", "short", "struct", "set",
            "this", "throw","true", "try", "typeof",
            "unchecked", "unsafe", "using", "ushort", 
            "virtual", "var", "void", "volatile",
            "while",
            "yield"
        };

        public async static Task<bool> CheckEnvironementAsync()
        {
            var SysName = Environment.GetEnvironmentVariable("_system_name");
            U.OSX = SysName == "OSX";
            SocketChat.Public.IPServer = U.OSX ? IPAddress.Any : await GetLocalIPAsync();
            return U.OSX;
        }      

        public async static Task<IPAddress> GetLocalIPAsync() 
        {
            IPHostEntry host = await Dns.GetHostEntryAsync(Dns.GetHostName());
            return host
                   .AddressList
                   .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }  

        public static Incrementer Incrementer = (int i, out int j) => { return j = i++; };// The Most Useless Function Ever
        
    }

    public static class C //console alias
    {
        public static Writer Display = (s) => { Cursor(0, Console.CursorTop); Write(s); Cursor(50, Console.CursorTop); };
        public static Writer Inline = (s) => { Cursor(0, Console.CursorTop); Write(s); Cursor(0, Console.CursorTop); };
        public static Writer WL = Console.WriteLine;
        public static Writer Write = Console.Write;
        public static CharWriter WriteChar = Console.Write;
        public static Reader Read = Console.ReadLine;
        public static Key Key = Console.ReadKey;  
        public static Cursor Cursor = Console.SetCursorPosition;
        
    }

}