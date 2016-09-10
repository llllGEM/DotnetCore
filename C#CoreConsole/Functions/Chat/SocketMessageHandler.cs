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
        public static string UserRegex = @"(<[\w]*>)";
        public static List<string> ClientSideConnectedUsers = new List<string>();
        public async static Task ServerSide(TcpClient sender, string message)
        {
            List<string> usernames = Regex.Split(message, UserRegex)
                                          .Where(s => Regex.Match(s, UserRegex).Success)
                                          .ToList();
            try{
                if(!SocketChat.Public.Users.Any(u => u.Username != null 
                                              && u.Username.ToLower()
                                                           .Contains(usernames[0]?.ToLower())))
                if(SocketChat.Public.Users.Any(u => u.Client == sender && u.Username == null))
                    SocketChat.Public.Users.Where(u => u.Client == sender && u.Username == null)
                                           .FirstOrDefault()
                                           .Username = usernames.FirstOrDefault();//store username 
            }catch(Exception){}
            var m = message.ToLower();
            if(m.Contains("users") || m.Contains("-u"))
            {
                SendConnectedUsers(sender, message);
            }
            else if(m.Contains("history") || m.Contains("-h"))
            {
                SendChatHistory(sender);
            }
            else if(m.Contains("target") || m.Contains("-t"))
            {
                //server sees it before treatment
                if(SocketChat.Public.SeeEverything) DisplayRemoteMessage(message); 
                SendToTarget(usernames,message,UserRegex);
            }
            else if(m.Contains("file") || m.Contains("-f"))
            {
                //TODO handle file transfer
            }
            else if(m.Contains("wizz") || m.Contains("-w"))
            {   // server can avoid beeing wized if seeEverything true  
                if(!SocketChat.Public.SeeEverything) Wizz(message, UserRegex); 
                if(SocketChat.Public.SeeEverything) DisplayRemoteMessage(message);
                await SendToEveryone(sender, message);
            }
            else{ //simple broadcast message
               await SendToEveryone(sender, message);
               SocketChat.Public.History.Add(message);// add message to history
            }
        }

        public static void ClientSide(string message)
        {
            var m = message.ToLower();
            if(m.Contains("users") || m.Contains("-u"))
            {
               StoreDisplayConnectedUsers(message, UserRegex);
            }
            else if(m.Contains("file") || m.Contains("-f"))
            {
                //TODO handle file transfer
            }
            else if(m.Contains("wizz") || m.Contains("-w"))
            {     
                Wizz(message, UserRegex);
            }
            else DisplayRemoteMessage(message);
        }
        public static void StoreDisplayConnectedUsers(string message, string regex)
        {
            var tab = Regex.Split(message, regex)
                            .Where(s => Regex.Match(s, regex).Success)
                            .ToList();
            if(!(tab.Count>0)) return;                
            ClientSideConnectedUsers = tab;
            var tab2 = Regex.Split(message, regex).ToList();
            message = "";
            foreach(string s in tab2.ToList())
            {
                if(s.ToLower().Trim() == "-u"|| s.ToLower().Trim() == "users"){
                    message += "<***Users Connected***>\n";
                    continue;
                }
                message += s;
            }
            DisplayRemoteMessage(message);
        }
        public static string AutoCompleteUsers(ConsoleKey key, bool server = false)
        {
            var toTrim = new char[3]{'<','_','>'};
            var wantedUser = key.ToString().ToLower();
            string autoCompletedString = " -t ";
            var connectedUsers = server ? SocketChat.Public.Users.Select(u => u.Username).ToList() 
                                        : ClientSideConnectedUsers;
            foreach(string username in connectedUsers)
            {
                var name = username.Trim(toTrim).ToLower();
                if(name.Contains(wantedUser))
                {
                    autoCompletedString += username;
                }
            }
            return autoCompletedString;
        }
        private static void SendConnectedUsers(TcpClient requester, string message)
        {
            message = "-u";
            foreach (var user in SocketChat.Public.Users.Where(u => u.Client != requester)) 
                message += user.Username+"\n";
            var target = SocketChat.Public.Users.Where(u => u.Client == requester).FirstOrDefault();
            SocketChat.Public.ServerBroadCastSpecificAsync(new List<SocketUser>{target}, message);
        }
        private static void SendChatHistory(TcpClient requester)
        {
            string previousMessages="";
            foreach(var previousMessage in SocketChat.Public.History) 
                previousMessages += previousMessage+"\n";
            var target = SocketChat.Public.Users.Where(u => u.Client == requester).FirstOrDefault();
            SocketChat.Public.ServerBroadCastSpecificAsync(new List<SocketUser>{target}, previousMessages);
        }

        private async static Task SendToEveryone(TcpClient sender, string message)
        {
            DisplayRemoteMessage(message);
            foreach(var user in SocketChat.Public.Users.Where(c => c.Client != sender))
            { // transmit message to everyone exept sender
                var bytes = Encoding.UTF8.GetBytes(message);
                await user.Client.GetStream().WriteAsync(bytes, 0, bytes.Length);
            }
        }

        public static void SendToTarget(List<string> usernames,string message, string regex)
        {
            bool exept = false;
            var toTrim = new char[3]{'<','_','>'};
            usernames.Remove(usernames.FirstOrDefault()); // everyone exept sender
            List<SocketUser> targetsList = new List<SocketUser>();
            if(SocketChat.Public.Users.Count>0)
                foreach(var user in SocketChat.Public.Users){
                    foreach(var name in usernames){
                        if(user.Username.Trim(toTrim)
                                        .ToLower()
                                        .Contains(name.Trim(toTrim).ToLower())) //if target exist
                        {
                            var tab = Regex.Split(message, regex).ToList();
                            message ="";
                            foreach(string s in tab.ToList())
                            {
                                if(s.ToLower().Trim() == "-t"|| s.ToLower().Trim() == "target"){
                                    message += "***Private Message***";
                                    continue;
                                }
                                if(s.ToLower().Trim(toTrim) == name.ToLower().Trim(toTrim)){
                                    targetsList.Add(user);
                                    continue;
                                } 
                                message += s;
                            }
                        }
                        else if(name.Trim(toTrim).ToLower().Contains("exept"))
                        {
                            exept = true;
                            var tab = Regex.Split(message, regex).ToList();
                            message ="";
                            foreach(string s in tab.ToList())
                            {
                                if(s.ToLower().Trim() == "-t"|| s.ToLower().Trim() == "target"){
                                    message += "***To All***";
                                    continue;
                                }
                                // if(s.ToLower().Trim(toTrim).Contains("exept")) //commented to let <exept> from message
                                //     continue;                                  
                                // if(s.ToLower().Trim(toTrim) == name.ToLower().Trim(toTrim))// commented to know who is exepted
                                //     continue;
                                message += s;
                            }
                            var exeptedUsers = SocketChat.Public.GetUsersFromName(usernames);
                            var targets = SocketChat.Public.Users.Except(exeptedUsers);
                            targetsList.AddRange(targets);
                            break;
                        }
                    if(exept) break;
                    }
                if(exept) break;
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
            DisplayRemoteMessage(message);
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