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
            C.Display("\t<--: Public \t Private :-->           ");
            var key = C.Key().Key;
            if(key == ConsoleKey.LeftArrow){
                C.Display("\t<--: Server \t Client :-->        ");
                var k = C.Key().Key;
                C.Cursor(0, Console.CursorTop);
                if(k == ConsoleKey.LeftArrow){
                    Public.ServerStartAsync(ct).Wait();
                }
                else if (k == ConsoleKey.RightArrow){
                    Public.ClientConnectAsync(ct).Wait();
                }
            }
            else if (key == ConsoleKey.RightArrow){
                C.Display("\t<--: Server \t Client :-->        ");
                var k = C.Key().Key;
                C.Cursor(0, Console.CursorTop);
                if(k == ConsoleKey.LeftArrow){
                    Private.ServerStartAsync(ct).Wait();
                }
                else if (k == ConsoleKey.RightArrow){
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
                await U.CheckEnvironementAsync();
                Listener = new TcpListener(IPServer, U.GlobalPort);
                try{ Listener.Start(); }
                catch(Exception e){C.WL(e.Message); StopRestart(); return;}
                C.WL($"GlobalServer Started at: {Listener.Server.LocalEndPoint}");
                Display.AskStealthMode(); Display.AskPrivacy();
                try{
                    var t = AcceptClientsAsync(Listener);
                    var t1 = ClientBroadCastAsync();
                    var t2 = ServerBroadCastAsync(Display.GenerateUsername(), ct); 
                    Task.WaitAll(new []{t,t1});// t2 Parallel to avoid blocking
                }catch(Exception e){ C.WL(e.Message); StopRestart(); return;}
            }

            public async static Task ClientConnectAsync(CancellationToken ct, int? port = null)
            {
                await U.CheckEnvironementAsync();
                Display.AskServerIPAddress(); Display.AskStealthMode();
                TcpClient Tcp;
                try{
                    Tcp = new TcpClient(AddressFamily.InterNetwork);
                    await Tcp.ConnectAsync(Public.IPServer, port??U.GlobalPort);
                }catch(Exception e){C.WL(e.Message);StopRestart();return;}
                C.WL($"You are Now Connected to: {Tcp.Client.RemoteEndPoint}");
                try{
                    var TWrite = SendAsync(Tcp, ct, Display.GenerateUsername()); //Can't start to receive until username chosen (Blocking)
                    var TRead = ReceiveAsync(Tcp, ct);
                    Task.WaitAll(new []{TWrite, TRead});
                }catch(Exception e){ C.WL(e.Message); StopRestart(); return;}
            }

            private async static Task AcceptClientsAsync(TcpListener listener)
            {
                while(true){
                    TcpClient client;
                    try{ client = await listener.AcceptTcpClientAsync(); }
                    catch(Exception e){ C.WL(e.Message); StopRestart(); return;}
                    C.WL("New Client Connected ");
                    Users.Add(new SocketUser{Client = client});
                }
            }

            private async static Task ClientBroadCastAsync()
            {
                Users.CollectionChanged += async (s,e) => {
                    if(Users.LastOrDefault().Client.Connected)
                    await ClientReceiveSendAsync(Users.LastOrDefault().Client);
                };
            }

            private async static Task ClientReceiveSendAsync(TcpClient sender)
            {
                while(true){
                    byte[] bytes = new byte[sender.SendBufferSize];
                    await sender.GetStream().ReadAsync(bytes,0,bytes.Length);
                    var message = Encoding.UTF8.GetString(bytes).Trim(new char[2]{'\\','0'});
                    if(!StealthMode)
                        foreach(var s in Regex.Split(message,"(\\0)+")){
                            C.WL(s);
                            if(s == string.Empty) continue;
                            else if(s == " ") continue;
                            else if(Regex.Match(s, "(\\0)+").Success) continue;
                            else {
                                await SocketMessageHandler.ServerSide(sender, message);
                                break;
                        }}
                    else await SocketMessageHandler.ServerSide(sender, message);
                }
            }

            private static Task ServerBroadCastAsync(string serverUsername, CancellationToken ct)
            {   
                return Task.Factory.StartNew(async()=>{
                    string firstChar;
                    while(true)
                        if(ManageInput(serverUsername, out firstChar, server: true)){
                            var inputUser = firstChar+C.Read().Trim();
                            if(inputUser.ToLower().Contains("file")) 
                                SocketFileHandler.Start(null).Wait();
                            var message = serverUsername+inputUser;
                            C.Cursor(message.Length, Console.CursorTop-1); //don't show Enter pressed
                            await SocketMessageHandler.ServerSide(new TcpClient(), message, true);
                            if(Users.Where(u => u.Client.Connected).Count() > 0)
                                Display.Receipt();
                            History.Add(serverUsername+message); //Add to History
                        }
                }, ct);
            }

            public static void ServerBroadCastSpecific(List<SocketUser> Users,string message, string file = null)
            {   
                var b = Encoding.UTF8.GetBytes(message);
                foreach(var user in Users)
                    if(user.Client.Connected)
                        user.Client.GetStream()?.WriteAsync(b, 0, b.Length);   
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
                await U.CheckEnvironementAsync();
                Listener= new TcpListener(Public.IPServer, U.PrivatePort);
                try{ Listener.Start(); }
                catch(Exception e){ C.WL(e.Message); StopRestart(); return;}
                C.WL($"PrivateServer Started at: {Listener.Server.LocalEndPoint}");
                Display.AskStealthMode();
                TcpClient client;
                try{ client = await Listener.AcceptTcpClientAsync(); }
                catch(Exception e){ C.WL(e.Message); StopRestart(); return;}
                C.WL("Client Connected");
                try{
                    if(client.Connected)
                    {
                        var TRead = ReceiveAsync(client, ct);
                        var TWrite = SendAsync(client, ct, Display.GenerateUsername());
                        Task.WaitAll(new []{TRead, TWrite});
                    }
                }catch(Exception e){ C.WL(e.Message); StopRestart(); return;}
            }

        }

        #region Private Functions

        public static bool ManageInput(string username, out string s, bool server = false)
        {
            if(Console.KeyAvailable){
                ConsoleKeyInfo key;
                if(U.OSX)key = Console.ReadKey(true);
                else { key = C.Key(); C.WL(""); }
                if(key.Key == ConsoleKey.Enter) C.Write(username);
                var k = C.Key();
                if(k.Key == ConsoleKey.Enter){ s = ""; return false; } 
                if(k.Key == ConsoleKey.Tab) {
                    C.Cursor(username.Length, Console.CursorTop);//hide tab pressed
                    s = SocketMessageHandler.AutoCompleteUsers(C.Key().Key, server); 
                    C.Write(s);} //AutoComplete
                else s = k.KeyChar.ToString().ToLower();
                return true;
            }
            else Thread.Sleep(200);
            s="";
            return false;
        }
        
        private static Task SendAsync(TcpClient client, CancellationToken ct, string username)
        {
            return Task.Factory.StartNew(()=>
            {
                string firstPart = "";
                while(true)
                    if(ManageInput(username, out firstPart)){
                        var inputUser = firstPart+C.Read().Trim();
                        if(inputUser.ToLower().Contains("file"))
                            SocketFileHandler.Start(client).Wait();
                        var message = username+inputUser;
                        var b = Encoding.UTF8.GetBytes(message);
                        C.Cursor(message.Length, Console.CursorTop-1); //don't show Enter pressed
                        if(client.Connected)
                            client.GetStream()?.WriteAsync(b,0,b.Length);
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
                        foreach(var s in Regex.Split(message,"(\\0)+")){
                            Display.RemoteMessage(s);
                            if(s == string.Empty) continue;
                            else if(Regex.Match(s, "(\\0)+").Success) continue;
                            else { SocketMessageHandler.ClientSide(message); break;}}
                    else  SocketMessageHandler.ClientSide(message);
                }
            }, ct);
        }

        private async static void StopRestart()
        {
            Listener.Stop();
            C.Read();
            await Program.MainAsync(new string[0]);
        }
        
        #endregion Private Functions

    }
}