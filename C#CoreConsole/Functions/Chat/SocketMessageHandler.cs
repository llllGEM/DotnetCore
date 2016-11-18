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

        public async static Task ServerSide(TcpClient sender, string message, bool server = false)
        {
            SocketUser currentSocketUser;
            var usernames = StoreUsername(sender, message, out currentSocketUser);
            var m = message.ToLower();
            if(m.Contains("code") || m.Contains("-c")|| m.Contains("```"))
            {
                try{
                    await SendCodeBlock(currentSocketUser, sender, message, server);
                }
                catch(Exception){}
            }
            else if(m.Contains("users") || m.Contains("-u"))
            {
                SendConnectedUsers(sender, server);
            }
            else if(m.Contains("history") || m.Contains("-h"))
            {
                SendChatHistory(sender);
            }
            else if(m.Contains("@") || m.Contains("-t") || m.Contains("target"))
            {
                //server sees it before treatment
                if(SocketChat.Public.SeeEverything) Display.RemoteMessage(message);
                try{ SendToTarget(currentSocketUser, usernames,message); }
                catch(Exception){} 
            }
            else if(m.Contains("wizz") || m.Contains("-w"))
            {   // server can avoid beeing wized if seeEverything true  
                if(!SocketChat.Public.SeeEverything) 
                    if(!U.OSX) 
                        if(!SocketChat.StealthMode)
                            Wizz(message); 
                if(SocketChat.Public.SeeEverything) Display.RemoteMessage(message); //logs wizz in server not in history
                await SendToEveryone(sender, message);
            }
            else{ //simple broadcast message
                try{
                    if(!server) Display.RemoteMessage(message); // display local
                    await SendToEveryone(sender, message);
                    SocketChat.Public.History.Add(message);// add message to history
                }
                catch(Exception e){C.WL(e.Message);}
            }

            SendReceiptTo(sender);
        }
        
        public static void ClientSide(string message)
        {
            var m = message.ToLower();
            if(message.Contains("ⓥ"))
            {
                Display.Receipt();
            }
            else if(m.Contains("code") || m.Contains("-c"))
            {
                Display.CodeBlock(message); // dislay colored code
            }
            else if(m.Contains("users") || m.Contains("-u"))
            {
               StoreDisplayConnectedUsers(message, UserRegex);
            }
            else if(m.Contains("wizz") || m.Contains("-w"))
            {     
                if(!U.OSX)
                    if(!SocketChat.StealthMode) 
                        Wizz(message);
            }
            else Display.RemoteMessage(message);
        }

        private async static Task SendCodeBlock(SocketUser coder, TcpClient sender, string message, bool server)
         {
            var listWords = message.Split(' ').ToList();
            message="\n"+listWords.FirstOrDefault()+" ";
            listWords.Remove(listWords.FirstOrDefault());
            foreach(var word in listWords)
            {
                var w = word.ToString().ToLower().Trim();
                if(U.CS_Keywords.Contains(w)) 
                    message+=word+"(keyword)";
                else if(Regex.Match(word, "\\d").Success)
                        message+=word+"(number)";
                else message += word;
                message+= " ";
            }
            string formattedMessage;
            CheckForCodeStrings(message, out formattedMessage);
            await SendToEveryone(sender, formattedMessage);
            if(server)C.Cursor(0, Console.CursorTop-2); // replace the actual line server side
            Display.CodeBlock(formattedMessage);// display in server
            if(coder != null)
                SocketChat.Public.ServerBroadCastSpecific(new List<SocketUser>{coder}, formattedMessage);
            SocketChat.Public.History.Add(formattedMessage);
        }

        private static string CheckForCodeStrings(string message, out string messageChecked)
        {
            var listStrings = message.Split(' ').ToList();
            messageChecked = "";
            for(var i =0 ; i < listStrings.Count; i++)
                if(listStrings[i].Contains("\"")){
                    messageChecked += listStrings[i]+" ";
                    do{
                        var next = listStrings[++i];
                        if(next.Contains("\""))
                        {
                            messageChecked+=next+"(string) ";
                            break;
                        }
                        else messageChecked+=next+" ";
                    }
                    while(true);
                }
                else if(listStrings[i].Contains("'")){
                        messageChecked += listStrings[i]+" ";
                        do{
                            var next = listStrings[++i];
                            if(next.Contains("'"))
                            {
                                messageChecked+=next+"(string) ";
                                break;
                            }
                            else messageChecked+=next+" ";
                        }
                        while(true);
                }
                else messageChecked+= listStrings[i]+" ";
            return messageChecked;
        }

        public static void StoreDisplayConnectedUsers(string message, string regex)
        {
            var tab = Regex.Split(message, regex)
                           .Where(s => Regex.Match(s, regex).Success)
                           .ToList();
            if(!(tab.Count>0)) return;                
            ClientSideConnectedUsers = tab;
            var tab2 = Regex.Split(message, regex).ToList();
            message = "\n";
            foreach(string s in tab2.ToList())
            {
                if(s.ToLower().Trim() == "-u"|| s.ToLower().Trim() == "users"){
                    message += $"<*** {tab.Count} User{{s}} Connected***>\n";
                    continue;
                }
                message += s;
            }
            Display.RemoteMessage(message);
        }

        public static string AutoCompleteUsers(ConsoleKey key, bool server = false)
        {
            var toTrim = new char[3]{'<','_','>'};
            var wantedUser = key.ToString().ToLower();
            string autoCompletedString = " @";
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

        private static void SendConnectedUsers(TcpClient requester, bool server)
        {
            var message = "users ";
            foreach (var user in SocketChat.Public.Users.Where(u => u.Client != requester)) 
                message += user.Username+"\n";
            var target = SocketChat.Public.Users.Where(u => u.Client == requester).FirstOrDefault();
            if(target != null)
                SocketChat.Public.ServerBroadCastSpecific(new List<SocketUser>{target}, message);
            if(server) 
                Display.RemoteMessage(message);
        }

        private static void SendChatHistory(TcpClient requester)
        {
            string previousMessages="\n";
            foreach(var previousMessage in SocketChat.Public.History) 
                previousMessages += previousMessage+"\n";
            var target = SocketChat.Public.Users?.Where(u => u.Client == requester).FirstOrDefault();
            if(target != null)
            SocketChat.Public.ServerBroadCastSpecific(new List<SocketUser>{target}, previousMessages);
        }

        private async static void SendReceiptTo(TcpClient sender)
        {
            var receipt = Encoding.UTF8.GetBytes("ⓥ");
            if(sender.Connected) await sender.GetStream().WriteAsync(receipt,0,receipt.Length);
        }

        private async static Task SendToEveryone(TcpClient sender, string message)
        {
            foreach(var user in SocketChat.Public.Users.Where(c => c.Client != sender))
            { // transmit message to everyone exept sender
                var bytes = Encoding.UTF8.GetBytes(message);
                if(user.Client.Connected)
                    await user.Client.GetStream()?.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        public static void SendToTarget(SocketUser sender, List<string> usernames,string message)
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
                            var tab = Regex.Split(message, UserRegex).ToList();
                            message ="\n";
                            foreach(string s in tab.ToList())
                            {
                                if(s.ToLower().Trim() == "@" || s.ToLower().Trim() == "-t"|| s.ToLower().Trim() == "target"){
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
                        if(name.Trim(toTrim).ToLower().Contains("exept"))
                        {
                            exept = true;
                            var tab = Regex.Split(message, UserRegex).ToList();
                            message ="\n";
                            foreach(string s in tab.ToList())
                            {
                                if(s.ToLower().Trim() == "@" || s.ToLower().Trim() == "-t"|| s.ToLower().Trim() == "target"){
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
                            exeptedUsers.Add(sender);
                            var targets = SocketChat.Public.Users.Except(exeptedUsers);
                            targetsList = targets.ToList();
                            break;
                        }
                    if(exept) break;
                    }
                if(exept) break;
                }

            if(targetsList.Count>0)
                SocketChat.Public.ServerBroadCastSpecific(targetsList, message);
        }

        public static void Wizz(string message)
        {
            for(int i = 0; i<=10; i++)
            {
                Console.Beep(37+i, 100);
                Console.WindowLeft += 5;
                Console.WindowTop += 2;
                Console.WindowLeft -= 5;
                Console.WindowTop -= 2;
            }
            var tab = Regex.Split(message, UserRegex).ToList();
            message ="";
            foreach(string s in tab.ToList())
            {
                if(s.ToLower().Trim() == "-w" || s.ToLower().Trim() == "wizz" ){
                    message += "***Wizz***";
                    continue;
                }
                message += s;
            }
            Display.RemoteMessage(message);
        }

        public static List<string> StoreUsername(TcpClient sender, string message, out SocketUser socketUser)
        {
            socketUser = null;
            List<string> usernames = Regex.Split(message, UserRegex)
                                          .Where(s => Regex.Match(s, UserRegex).Success)
                                          .ToList();
            if(usernames.Count>0)
                if(!SocketChat.Public.Users.Any(u => u.Username != null 
                                                  && u.Username.ToLower()
                                                               .Contains(usernames[0]?.ToLower())))
                    if(sender != null)
                        if(SocketChat.Public.Users.Any(u => u.Client == sender && u.Username == null)){
                            socketUser = SocketChat.Public.Users.Where(u => u.Client == sender && u.Username == null)
                                                                .FirstOrDefault();
                            socketUser.Username = usernames.FirstOrDefault();// store username in user object
                        }
                        else socketUser = SocketChat.Public.Users.Where(u => u.Username == usernames.FirstOrDefault())
                                                                 .FirstOrDefault();
            return usernames;
        }

    }
}