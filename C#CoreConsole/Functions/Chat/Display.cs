using System;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConsoleApplication.Functions.Chat
{
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
            SetStringColor();
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
            C.WL("Enter Server Ip Address...Or Press Enter To Discover \t\t");
            var ip = C.Read();
            if(ip != string.Empty)
                SocketChat.Public.IPServer = IPAddress.Parse(ip);
        }

    }
}