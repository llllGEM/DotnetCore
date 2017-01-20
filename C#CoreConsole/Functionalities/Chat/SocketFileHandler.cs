using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ConsoleApplication.Functions.Chat
{
    public static class SocketFileHandler
    {
        public async static Task Start(TcpClient client) 
        {
            C.WL("Send or Receive ? s/r");
            var key = Console.ReadKey(true);
            if(key.Key == ConsoleKey.S)
                await SendFileAsync(client);
            else if(key.Key == ConsoleKey.R)
                await ReceiveFileAsync(client);
        }
        public async static Task SendFileAsync(TcpClient receiver)
        {
            C.WL("Enter Path to File...");
            var filePath = C.Read().Trim();
            byte[] fileBytes;
            try{ fileBytes = File.ReadAllBytes(filePath); }
            catch(Exception e ){ C.WL(e.Message); return; }
            var compressedBytes = Compress(fileBytes);
            if(receiver != null){
                if(receiver.Connected)
                    await receiver.GetStream().WriteAsync(compressedBytes, 0, compressedBytes.Length);
                return;
            }
            foreach(var user in SocketChat.Public.Users)
                if(user.Client.Connected)
                    await user.Client.GetStream().WriteAsync(compressedBytes, 0, compressedBytes.Length);
            Program.ProgressBar(false, ConsoleColor.DarkGreen, "File Transferred");
            return;
        }

        public async static Task ReceiveFileAsync(TcpClient transmitter)
        {
            if(transmitter != null){
                C.WL("Enter File Extension...");
                var extention = C.Read().Trim().Replace(".", "").ToLower();
                C.WL("Enter Where You Want to Store the File (Full Path)...");
                var path = C.Read().Trim();
                if(path == string.Empty) path = Directory.GetCurrentDirectory();
                var fileBytes = new byte[transmitter.SendBufferSize];
                await transmitter.GetStream().ReadAsync(fileBytes, 0,fileBytes.Length);
                var decompressedBytes = Decompress(fileBytes);
                File.WriteAllBytes($"{path}/{Path.GetRandomFileName()}.{extention}", fileBytes);
                Program.ProgressBar(false, ConsoleColor.DarkGreen, "File Received & Beeing Written or Saved");
            }
            return;
        }

        public static byte[] Compress(byte[] fileBytes)
        {
            using (var compressIntoMs = new MemoryStream())
            {
                using (var gzs = new BufferedStream(new GZipStream(compressIntoMs, 
                 CompressionMode.Compress), fileBytes.Length))
                {
                    gzs.Write(fileBytes, 0, fileBytes.Length);
                }
                return compressIntoMs.ToArray(); 
            }
        }

        public static byte[] Decompress(byte[] fileBytes)
        {
            using (var compressedMs = new MemoryStream())
            {
                using (var decompressedMs = new MemoryStream())
                {
                    using (var gzs = new BufferedStream(new GZipStream(compressedMs, 
                     CompressionMode.Decompress), fileBytes.Length))
                    {
                        gzs.CopyTo(decompressedMs);
                    }
                    return decompressedMs.ToArray(); 
                }
            }
        }
    }
}