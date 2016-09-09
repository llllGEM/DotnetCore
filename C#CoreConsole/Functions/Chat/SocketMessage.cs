using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApplication.Functions.Chat
{
    public static class SocketMessage
    {
        public async static Task ServerCommandHandler(TcpClient sender, string message)
        {
            var regex = @"(<[\w]*>)";
            List<string> usernames = Regex.Split(message, regex).Where(s =>Regex.Match(s, regex).Success).ToList();
            if(!SocketChat.Public.Users.Any(u=>u.Username != null && u.Username.ToLower().Contains(usernames[0].ToLower())))
            if(SocketChat.Public.Users.Any(u=>u.Client == sender && u.Username == null))
            SocketChat.Public.Users.Where(u=>u.Client == sender && u.Username == null)
                        .FirstOrDefault()
                        .Username = usernames.FirstOrDefault();//store username 
            var m = message.ToLower();
            if(m.Contains("target") || m.Contains("-t"))
            {
                SendToTarget(usernames,message,regex);
            }
            else if(m.Contains("file") || m.Contains("-f"))
            {
                //TODO handle file transfer
            }
            else if(m.Contains("wizz") || m.Contains("-w"))
            {     
                Wizz(message, regex);
                await SendToEveryone(sender, message);
            }
            else{
               await SendToEveryone(sender, message);
            }
        }

        public static void ClientCommandHandler(string message)
        {
            DisplayRemoteMessage(message);
            var regex = @"(<[\w]*>)";
            var m = message.ToLower();
            if(m.Contains("file") || m.Contains("-f"))
            {
                //TODO handle file transfer
            }
            else if(m.Contains("wizz") || m.Contains("-w"))
            {     
                Wizz(message, regex);
            }
        }

        private async static Task SendToEveryone(TcpClient sender, string message)
        {
            DisplayRemoteMessage(message);
            foreach(var user in SocketChat.Public.Users.Where(c => c.Client != sender)){ // transmit message to everyone exept sender
                var bytes = Encoding.UTF8.GetBytes(message);
                await user.Client.GetStream().WriteAsync(bytes, 0, bytes.Length);
            }
        }

        private static void SendToTarget(List<string> usernames,string message, string regex)
        {
            var toTrim = new char[3]{'<','_','>'};
            usernames.Remove(usernames.FirstOrDefault()); // everyone exept sender
            List<SocketUser> targetsList = new List<SocketUser>();
            foreach(var user in SocketChat.Public.Users)
                foreach(var username in usernames)
                    if(user.Username.Trim(toTrim).ToLower()
                       .Contains(username.Trim(toTrim).ToLower()))
                    {
                        var tab = Regex.Split(message, regex).ToList();
                        message ="";
                        foreach(string s in tab.ToList())
                        {
                            if(s.ToLower().Trim() == "-t"|| s.ToLower().Trim() == "target"){ //optional maybe usefule to notice its private message
                                tab.Remove(s);
                                continue;
                            }
                            if(s == username){
                                tab.Remove(s);
                                continue;
                            } 
                            message += s;
                        }
                        targetsList.Add(user);
                    }
            if(targetsList.Count>0)
            SocketChat.Public.ServerBroadCastSpecificAsync(targetsList, message);
        }

        public static void Wizz(string message, string regex)
        {
            for(int i = 0; i<=10; i++)
            {
                Console.Beep();
                Console.SetWindowPosition(Console.WindowLeft-10, Console.WindowTop);
                Console.SetWindowPosition(Console.WindowLeft, Console.WindowTop-10);
                Console.SetWindowPosition(Console.WindowLeft+10, Console.WindowTop);
                Console.SetWindowPosition(Console.WindowLeft, Console.WindowTop-10);
            }
            var tab = Regex.Split(message, regex).ToList();
            message ="";
            foreach(string s in tab.ToList())
            {
                if(s.ToLower().Trim() == "-w" || s.ToLower().Trim() == "wizz" ){
                    tab.Remove(s);
                    continue;
                }
                message += s;
            }
        }
        
        public static void DisplayRemoteMessage(string message)
        {
            var colorSet = U.Colors[new Random().Next(0,U.Colors.Length)];
            Console.BackgroundColor = colorSet.Item1;
            Console.ForegroundColor = colorSet.Item2;
            C.WL(message);
            Console.ResetColor();
        }
    }
}