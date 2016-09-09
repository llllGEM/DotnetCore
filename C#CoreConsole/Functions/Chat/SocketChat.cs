using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication.Functions.Chat
{
    public static class SocketChat
    {
        public static TcpListener Listener;

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
            C.Display("1: Public \t 2: Private");
            var keyChar = C.Key().KeyChar;
            if(keyChar == '&'){
                C.Display("1: Server \t 2: Client");
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
                C.Display("1: Server \t 2: Client");
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
            public static ObservableCollection<SocketUser> Users = new ObservableCollection<SocketUser>();

            public async static Task ServerStartAsync(CancellationToken ct)
            {
                Listener = new TcpListener(IPAddress.Any, U.GlobalPort);
                try{Listener.Start();}
                catch(Exception e){C.WL(e.Message); StopRestart(); return;}
                C.WL($"GlobalServer Started at: {Listener.Server.LocalEndPoint}");
                try{
                    var t = AcceptClientsAsync(Listener);
                    var t1 = ClientBroadCastAsync();
                    var t2 = ServerBroadCastAsync(GenerateUsername(), ct); // Parallel to avoid blocking
                    Task.WaitAll(new []{t,t1});
                }catch(Exception e){C.WL(e.Message); StopRestart(); return;}
            }

            public async static Task ClientConnectAsync(CancellationToken ct, int? port = null)
            {
                TcpClient Tcp;
                try{
                    Tcp = new TcpClient(AddressFamily.InterNetwork);
                    await Tcp.ConnectAsync(IPAddress.Any, port??U.GlobalPort);
                }catch(Exception e){C.WL(e.Message);StopRestart();return;}
                C.WL($"You are Now Connected to: {Tcp.Client.RemoteEndPoint}");
                try{
                    if(Tcp.Client.Connected)
                    {
                        var TWrite = SendAsync(Tcp, ct, GenerateUsername()); //Can't start to receive until username chosen (Blocking)
                        var TRead = ReceiveAsync(Tcp, ct);
                        Task.WaitAll(new []{TWrite, TRead});
                    }
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
                    byte[] bytes = new byte[sender.ReceiveBufferSize];
                    await sender.GetStream().ReadAsync(bytes,0,bytes.Length);
                    var message = Encoding.UTF8.GetString(bytes).Trim(new char[2]{'\\','0'});
                    await SocketMessage.ServerCommandHandler(sender, message);
                }
            }

            private static Task ServerBroadCastAsync(string serverUsername, CancellationToken ct)
            {   
                return Task.Factory.StartNew(()=>{
                    while(true){
                        if(C.Key().Key == ConsoleKey.Enter)C.Write(serverUsername);
                        else continue;
                        var message = C.Read();
                        var b = Encoding.UTF8.GetBytes(serverUsername+message);
                        foreach(var user in Users)
                        user.Client.GetStream().WriteAsync(b, 0, b.Length);
                    }
                }, ct);
            }

            public static void ServerBroadCastSpecificAsync(List<SocketUser> Users,string message = null, string file = null)
            {   
                if(message != null)
                {
                    var b = Encoding.UTF8.GetBytes(message);
                    foreach(var user in Users)
                    user.Client.GetStream().WriteAsync(b, 0, b.Length);   
                }
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
        
        private static Task SendAsync(TcpClient client, CancellationToken ct, string username = null ,string message = null)
        {
            return Task.Factory.StartNew(()=>{
                while(true){
                    if(C.Key().Key == ConsoleKey.Enter)C.Write(username);
                    else continue;
                    var m = Encoding.UTF8.GetBytes(message??(username != null ? username+C.Read() : C.Read()));
                    client.GetStream().WriteAsync(m,0,m.Length);
                }
            }, ct);
        }

        private static Task ReceiveAsync(TcpClient client, CancellationToken ct)
        {
            return Task.Factory.StartNew(async ()=>{
                while(true){
                    byte[] bytes = new byte[client.ReceiveBufferSize];
                    await client.GetStream().ReadAsync(bytes,0,bytes.Length);
                    var message = Encoding.UTF8.GetString(bytes).Trim(new char[2]{'\\','0'});
                    SocketMessage.ClientCommandHandler(message);
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