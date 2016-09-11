using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ConsoleApplication.Functions.Chat;

namespace ConsoleApplication
{

    public delegate bool Parse<T>(string t, out T i);
    public delegate void Writer(string s);
    public delegate string Reader();
    public delegate ConsoleKeyInfo Key();
    public delegate void Cursor(int left, int top);

    public static class U //Utils
    {
        public static int GlobalPort = 2222;
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
            new Tuple<ConsoleColor,ConsoleColor>(ConsoleColor.Black, ConsoleColor.Cyan)
        };

        public async static Task<bool> CheckEnvironementAsync()
        {
            var SysName = Environment.GetEnvironmentVariable("_system_name");
            bool b = SysName == "OSX";
            SocketChat.Public.IPServer = b ? IPAddress.Any : await GetLocalIPAsync();
            return b;
        }      

        public async static Task<IPAddress> GetLocalIPAsync() 
        {
            IPHostEntry host = await Dns.GetHostEntryAsync(Dns.GetHostName());
            return host
                   .AddressList
                   .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }  
    }

    public static class C //console alias
    {
        public static Writer Display = (s) => {Cursor(0, Console.CursorTop);Write(s);Cursor(50, Console.CursorTop);};
        public static Writer WL = Console.WriteLine;
        public static Writer Write = Console.Write;
        public static Reader Read = Console.ReadLine;
        public static Key Key = Console.ReadKey;  
        public static Cursor Cursor = Console.SetCursorPosition;
    }

    public static class Display
    {
        public static void RemoteMessage(string message)
        {
            var colorSet = U.Colors[new Random().Next(0,U.Colors.Length)];
            Console.BackgroundColor = colorSet.Item1;
            Console.ForegroundColor = colorSet.Item2;
            C.WL("\n"+message);
            Console.ResetColor();
        }

        public async static Task ReceiptAsync()
        {
            Console.ForegroundColor = await U.CheckEnvironementAsync() 
                                            ? ConsoleColor.White 
                                            : ConsoleColor.Green;
            C.Write(" ⓥ");
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