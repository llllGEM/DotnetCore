using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication.Functions.Chat
{
    public static class SocketChat
    {
        public static TcpListener Listener;

        public static bool StealthMode {get;set;} = false;

        public static void Begin(CancellationToken ct) 
        {
            Console.Clear();
            //Check Utils.cs for C.WL() and all C class methods
            C.WL(@"
             _____            __        __  ________          __ 
            / ___/____  _____/ /_____  / /_/ ____/ /_  ____ _/ /_
            \__ \/ __ \/ ___/ //_/ _ \/ __/ /   / __ \/ __ `/ __/
           ___/ / /_/ / /__/ ,< /  __/ /_/ /___/ / / / /_/ / /_  
          /____/\____/\___/_/|_|\___/\__/\____/_/ /_/\__,_/\__/  
                                                       ");
            C.Display("\t1: Public \t 2: Private");
            var keyChar = C.Key().KeyChar;
            if(keyChar == '&'){
                C.Display("\t1: Server \t 2: Client");
                var kc = C.Key().KeyChar;
                C.Cursor(0, Console.CursorTop);
                if(kc == '&'){
                    Public.ServerStartAsync(ct).Wait();
                }
                else if (kc == 'é'){
                    Public.ClientConnectAsync(ct).Wait();
                }
            }
            else if (keyChar == 'é'){
                C.Display("\t1: Server \t 2: Client");
                var kc = C.Key().KeyChar;
                C.Cursor(0, Console.CursorTop);
                if(kc == '&'){
                    Private.ServerStartAsync(ct).Wait();
                }
                else if (kc == 'é'){
                    Public.ClientConnectAsync(ct, U.PrivatePort).Wait();
                }
            }
        }

        public static class Public
        {
            public static IPAddress IPServer {get; set;} = IPAddress.Any;

            public static bool SeeEverything {get;set;} = false;

            public static ObservableCollection<SocketUser> Users {get; set;} = new ObservableCollection<SocketUser>();

            public static List<string> History {get;set;} = new List<string>();

            public async static Task ServerStartAsync(CancellationToken ct)
            {
                Listener = new TcpListener(IPAddress.Any, U.GlobalPort);
                try{Listener.Start();}
                catch(Exception e){C.WL(e.Message); StopRestart(); return;}
                C.WL($"GlobalServer Started at: {Listener.Server.LocalEndPoint}");
                AskStealthMode(); AskPrivacy();
                try{
                    var t = AcceptClientsAsync(Listener);
                    var t1 = ClientBroadCastAsync();
                    var t2 = ServerBroadCastAsync(GenerateUsername(), ct); 
                    Task.WaitAll(new []{t,t1});// t2 Parallel to avoid blocking
                }catch(Exception e){C.WL(e.Message); StopRestart(); return;}
            }

            public async static Task ClientConnectAsync(CancellationToken ct, int? port = null)
            {
                AskServerIPAddress(); AskStealthMode();
                TcpClient Tcp;
                try{
                    Tcp = new TcpClient(AddressFamily.InterNetwork);
                    await Tcp.ConnectAsync(Public.IPServer, port??U.GlobalPort);
                }catch(Exception e){C.WL(e.Message);StopRestart();return;}
                C.WL($"You are Now Connected to: {Tcp.Client.RemoteEndPoint}");
                try{
                    var TWrite = SendAsync(Tcp, ct, GenerateUsername()); //Can't start to receive until username chosen (Blocking)
                    var TRead = ReceiveAsync(Tcp, ct);
                    Task.WaitAll(new []{TWrite, TRead});
                }catch(Exception e){C.WL(e.Message);StopRestart();return;}
            }

            private async static Task AcceptClientsAsync(TcpListener listener)
            {
                while(true){
                    TcpClient client;
                    try{client = await listener.AcceptTcpClientAsync();}
                    catch(Exception e){C.WL(e.Message);StopRestart();return;}
                    C.WL("New Client Connected ");
                    Users.Add(new SocketUser{Client = client});
                }
            }

            private async static Task ClientBroadCastAsync()
            {
                Users.CollectionChanged += async (s,e) => {
                    if(Users.LastOrDefault().Client.Connected)
                    await ClientSendReceiveAsync(Users.LastOrDefault().Client);
                };
            }

            private async static Task ClientSendReceiveAsync(TcpClient sender)
            {
                while(true){
                    byte[] bytes = new byte[sender.SendBufferSize];
                    await sender.GetStream().ReadAsync(bytes,0,bytes.Length);
                    var message = Encoding.UTF8.GetString(bytes).Trim(new char[2]{'\\','0'});
                    if(!StealthMode)
                        foreach(var s in Regex.Split(message,"(\\0)+"))
                            if(s == string.Empty) continue;
                            else if(Regex.Match(s, "(\\0)+").Success) continue;
                            else {
                                await SocketMessageHandler.ServerSide(sender, message);
                                break;
                            }
                    else await SocketMessageHandler.ServerSide(sender, message);
                }
            }

            private static Task ServerBroadCastAsync(string serverUsername, CancellationToken ct)
            {   
                return Task.Factory.StartNew(()=>{
                    string firstChar;
                    while(true){
                        if(ManageInput(serverUsername, out firstChar, server: true)){
                            var message = firstChar+C.Read();
                            var b = Encoding.UTF8.GetBytes(serverUsername+message);
                            foreach(var user in Users)
                                if(user.Client.Connected)
                                    user.Client?.GetStream()?.WriteAsync(b, 0, b.Length);
                            History.Add(serverUsername+message); //Add to History
                        }
                    }
                }, ct);
            }

            public static void ServerBroadCastSpecificAsync(List<SocketUser> Users,string message = null, string file = null)
            {   
                if(message != null)
                {
                    var b = Encoding.UTF8.GetBytes(message);
                    foreach(var user in Users)
                        if(user.Client.Connected)
                            user.Client?.GetStream()?.WriteAsync(b, 0, b.Length);   
                }
            }

            public static List<SocketUser> GetUsersFromName(List<string> names)
            {
                var toTrim = new char[3]{'<','_','>'};
                var usersList = new List<SocketUser>();
                foreach(var name in names)
                {
                    var user = Public.Users.Where(u => u.Username.Trim(toTrim)
                                                                 .ToLower()
                                                                 .Contains(name.Trim(toTrim)
                                                                               .ToLower()))
                                                                 .FirstOrDefault();
                    if(user != null) usersList.Add(user);
                }
                return usersList;
            }
            
        }

        public static class Private
        {
            public async static Task ServerStartAsync(CancellationToken ct)
            {
                Listener= new TcpListener(IPAddress.Any, U.PrivatePort);
                try{Listener.Start();}
                catch(Exception e){C.WL(e.Message); StopRestart(); return;}
                C.WL($"PrivateServer Started at: {Listener.Server.LocalEndPoint}");
                AskStealthMode();
                TcpClient client;
                try{client = await Listener.AcceptTcpClientAsync();}
                catch(Exception e){C.WL(e.Message);StopRestart();return;}
                C.WL("Client Connected");
                try{
                    if(client.Connected)
                    {
                        var TRead = ReceiveAsync(client, ct);
                        var TWrite = SendAsync(client, ct, GenerateUsername());
                        Task.WaitAll(new []{TRead, TWrite});
                    }
                }catch(Exception e){C.WL(e.Message);StopRestart();return;}
            }

        }

        #region Private Functions

        public static bool ManageInput(string username, out string s, bool server = false)
        {
            if(Console.KeyAvailable){
                var key = Console.ReadKey(true);
                if(key.Key == ConsoleKey.Enter) C.Write(username);
                var k = C.Key();
                if(k.Key == ConsoleKey.Enter){ s = ""; return false; } 
                if(k.Key == ConsoleKey.Tab) {
                    s = SocketMessageHandler.AutoCompleteUsers(C.Key().Key, server); 
                    C.Write(s);} //AutoComplete
                else s = k.KeyChar.ToString().ToLower();
                return true;
            }
            else Thread.Sleep(200);
            s="";
            return false;
        }
        
        private static Task SendAsync(TcpClient client, CancellationToken ct, string username = null)
        {
            return Task.Factory.StartNew(()=>
            {
                string firstPart = "";
                while(true){
                    if(ManageInput(username, out firstPart)){
                        var m = Encoding.UTF8.GetBytes(username+firstPart+C.Read());
                        if(client.Connected)
                            client?.GetStream()?.WriteAsync(m,0,m.Length);
                    }
                }
            }, ct);
        }

        private static Task ReceiveAsync(TcpClient client, CancellationToken ct)
        {
            return Task.Factory.StartNew(async ()=>
            {
                while(true){
                    byte[] bytes = new byte[client.SendBufferSize];
                    await client.GetStream().ReadAsync(bytes,0,bytes.Length);
                    var message = Encoding.UTF8.GetString(bytes).Trim(new char[2]{'\\','0'});
                    if(!StealthMode)
                        foreach(var s in Regex.Split(message,"(\\0)+"))
                            if(s == string.Empty) continue;
                            else if(Regex.Match(s, "(\\0)+").Success) continue;
                            else { SocketMessageHandler.ClientSide(message); break;}
                    else  SocketMessageHandler.ClientSide(message);
                }
            }, ct);
        }


        private static string GenerateUsername()
        {
            C.WL("Type Username or Press Enter To Generate...");
            char start = '<';char repeat = '_'; string end = "> "; // <username______> 
            var username = C.Read();
            C.WL("Press Enter Before To Write Something...");
            return username.Length > 0 
                 ? start+username.PadRight(U.NameLength,repeat)+end
                 : start+(Environment.MachineName + Environment.ProcessorCount).PadRight(U.NameLength,repeat)+end;
        }

        private static void AskPrivacy()
        {
            C.WL("See Every Message ? y/n");
            var key = C.Key().Key;
            if(key == ConsoleKey.Y 
            || key == ConsoleKey.Enter) Public.SeeEverything = true;
        }

        //if one disconnect then send an infinite stream to hide console text for everyon
        private static void AskStealthMode() 
        {
            C.WL("StealthMode ? y/n");
            var key = C.Key().Key;
            if(key == ConsoleKey.Y 
            || key == ConsoleKey.Enter) StealthMode = true;
        }

        private static void AskServerIPAddress()
        {
            C.WL("Enter Server Ip Address...");
            var ip = C.Read();
            if(ip != string.Empty)
                Public.IPServer = IPAddress.Parse(ip);
        }

        private async static void StopRestart()
        {
            Listener.Stop();
            C.Read();
            await Program.MainAsync(new string[0]);
        }

        //Not Working On OS X
        private async static Task<IPAddress> GetLocalIP() 
        {
            IPHostEntry host = await Dns.GetHostEntryAsync(Dns.GetHostName());
            return host
                   .AddressList
                   .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }

        #endregion Private Functions

    }
}