using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApplication.Functions.Chat
{
    public static class SocketMessageHandler
    {
        public async static Task ServerSide(TcpClient sender, string message)
        {
            var regex = @"(<[\w]*>)";
            List<string> usernames = Regex.Split(message, regex).Where(s => Regex.Match(s, regex).Success).ToList();
            if(!SocketChat.Public.Users.Any(u => u.Username != null && u.Username.ToLower().Contains(usernames[0].ToLower())))
                if(SocketChat.Public.Users.Any(u => u.Client == sender && u.Username == null))
                    SocketChat.Public.Users.Where(u => u.Client == sender && u.Username == null)
                                           .FirstOrDefault()
                                           .Username = usernames.FirstOrDefault();//store username 
            var m = message.ToLower();
            if(m.Contains("history") || m.Contains("-h"))
            {
                SendHistory(sender);
            }
            else if(m.Contains("target") || m.Contains("-t"))
            {
                SendToTarget(usernames,message,regex);
                if(SocketChat.Public.SeeEverything)DisplayRemoteMessage(message); 
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
            else{ //simple broadcast message
               await SendToEveryone(sender, message);
               SocketChat.Public.History.Add(message);// add message to history
            }
        }

        public static void ClientSide(string message)
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
        private static void SendHistory(TcpClient requester)
        {
            string previousMessages="";
            foreach(var previousMessage in SocketChat.Public.History) 
                previousMessages += previousMessage+"\n";
            var target = SocketChat.Public.Users.Where(u => u.Client == requester).ToList();
            SocketChat.Public.ServerBroadCastSpecificAsync(target, previousMessages);
        }

        private async static Task SendToEveryone(TcpClient sender, string message)
        {
            DisplayRemoteMessage(message);
            foreach(var user in SocketChat.Public.Users.Where(c => c.Client != sender)){ // transmit message to everyone exept sender
                var bytes = Encoding.UTF8.GetBytes(message);
                await user.Client.GetStream().WriteAsync(bytes, 0, bytes.Length);
            }
        }

        public static void SendToTarget(List<string> usernames,string message, string regex)
        {
            var toTrim = new char[3]{'<','_','>'};
            usernames.Remove(usernames.FirstOrDefault()); // everyone exept sender
            List<SocketUser> targetsList = new List<SocketUser>();
            if(SocketChat.Public.Users.Count>0)
                foreach(var user in SocketChat.Public.Users)
                    foreach(var username in usernames)
                        if(user.Username.Trim(toTrim)
                                        .ToLower()
                                        .Contains(username.Trim(toTrim)
                                                          .ToLower()))
                        {
                            var tab = Regex.Split(message, regex).ToList();
                            message ="";
                            foreach(string s in tab.ToList())
                            {
                                if(s.ToLower().Trim() == "-t"|| s.ToLower().Trim() == "target"){
                                    message += "***Private Message***";
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
            for(int i = 0; i<=5; i++)
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
                    message += "***Wizz***";
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