using System.Net.Sockets;

namespace ConsoleApplication.Functions.Chat
{
    public class SocketUser
    {

        public TcpClient Client {get; set;}

        public string Username {get; set;} = null;
        
    }
}