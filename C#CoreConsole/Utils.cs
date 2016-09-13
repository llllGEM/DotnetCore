using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConsoleApplication.Functions.Chat;

namespace ConsoleApplication
{

    public delegate bool Parse<T>(string t, out T i);
    public delegate void Writer(string s);
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
        public static Writer WL = Console.WriteLine;
        public static Writer Write = Console.Write;
        public static Reader Read = Console.ReadLine;
        public static Key Key = Console.ReadKey;  
        public static Cursor Cursor = Console.SetCursorPosition;
        
    }

    public static class Display
    {
        public static void CodeBlock(string message)
        {
            var listWords = Regex.Split(message, " ").ToList();
            message = "\n"+listWords.FirstOrDefault()+" "; // username
            C.Write(message);
            listWords.Remove(listWords.FirstOrDefault());
            for(var i = 0; i < listWords.Count; i++)
            {
                if(listWords[i].ToLower().Contains("code")
                || listWords[i].ToLower().Contains("-c")
                || listWords[i].ToLower().Contains("```")) continue;
                if(listWords[i].Contains("(keyword)")) 
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    C.Write(listWords[i].Replace("(keyword)", " "));
                    Console.ResetColor();
                }
                else if(listWords[i].Contains("\"")) 
                {
                    SetStringColor();
                    C.Write(listWords[i]);
                    var next = " "+listWords[++i];
                    while(!next.Contains("\""))
                    {
                        C.Write(next+" ");
                        next = " "+listWords[++i];
                    }
                    C.Write(listWords[i].Replace("(string)", " "));
                    Console.ResetColor();
                }
                else if(listWords[i].Contains("'")) 
                {
                    SetStringColor();
                    C.Write(listWords[i]);
                    var next = " "+listWords[++i];
                    while(!next.Contains("'"))
                    {
                        C.Write(next+" ");
                        next = " "+listWords[++i];
                    }
                    C.Write(listWords[i].Replace("(string)", " "));
                    Console.ResetColor();
                }
                else if(listWords[i].Contains("(number)")) 
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    C.Write(listWords[i].Replace("(number)", " "));
                    Console.ResetColor();
                }
                else C.Write(listWords[i]+" ");
            }
            C.Write("      ");
        }

        private static void SetStringColor()
        {
            Console.ForegroundColor = U.OSX 
                                      ? ConsoleColor.White // for my OSX console style
                                      : ConsoleColor.Green;
        }
        
        public static void RemoteMessage(string message, Tuple<ConsoleColor,ConsoleColor> colorSet = null)
        {
            if(colorSet == null)colorSet = U.Colors[new Random().Next(0,U.Colors.Length)];
            if(SocketChat.StealthMode) colorSet = U.Colors[0]; // no colors not to be spotted
            Console.BackgroundColor = colorSet.Item1;
            Console.ForegroundColor = colorSet.Item2;
            C.WL("\n"+message);
            Console.ResetColor();
        }

        public static void Receipt()
        {
            Console.ForegroundColor = U.OSX 
                                      ? ConsoleColor.White 
                                      : ConsoleColor.Green;
            if(U.OSX)C.Write(" ⓥ");
            else C.Write(" v");
            Console.ResetColor();
        }

        public static string GenerateUsername()
        {
            C.WL("Type Username or Press Enter To Generate...");
            char start = '<';char repeat = '_'; string end = "> "; // <username______> 
            var username = C.Read();
            C.WL("Press Enter Before To Write Something...");
            return username.Length > 0 
                 ? start+username.PadRight(U.NameLength,repeat)+end
                 : start+(Environment.MachineName + Environment.ProcessorCount).PadRight(U.NameLength,repeat)+end;
        }

        public static void AskPrivacy()
        {
            C.WL("See Every Message ? y/n");
            var key = C.Key().Key;
            if(key == ConsoleKey.Y 
            || key == ConsoleKey.Enter) SocketChat.Public.SeeEverything = true;
        }

        //if one disconnect then send an infinite stream to hide console text for everyon
        public static void AskStealthMode() 
        {
            C.WL("StealthMode ? y/n");
            var key = C.Key().Key;
            if(key == ConsoleKey.Y 
            || key == ConsoleKey.Enter) SocketChat.StealthMode = true;
        }

        public static void AskServerIPAddress()
        {
            C.WL("Enter Server Ip Address...Or Press Enter To Discover (Only Mac) \t\t");
            var ip = C.Read();
            if(ip != string.Empty)
                SocketChat.Public.IPServer = IPAddress.Parse(ip);
        }

    }
}