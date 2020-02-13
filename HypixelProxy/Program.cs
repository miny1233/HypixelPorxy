using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace HypixelProxy
{
    class Program
    {
        public static IPHostEntry Hypixel;
        static void Main(string[] args)
        {
            string IP;
            Console.WriteLine("绑定本地IP（不填为0.0.0.0）");
            string rmsg = Console.ReadLine();
            if (rmsg != "")IP = rmsg;
            else IP = "0.0.0.0";
            Console.WriteLine("绑定本地端口（必须）");
            string Port = Console.ReadLine();
            Console.WriteLine("解析Hypixel域名中");
            try
            {
                Hypixel = Dns.GetHostEntry("mc.hypixel.net");
                Console.WriteLine("解析完成");
            }
            catch
            {
                Console.WriteLine("无法完成域名解析");
                return;
            }
            int port = int.Parse(Port);
            Socket Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Server.Bind(new IPEndPoint(IPAddress.Parse(IP), port));
            Server.Listen(100);
            while (true)
            {
                Socket Client = Server.Accept();              
                object client = Client;
                Proxy.Control(client);
            }
        }
    }

    class Proxy
    {
        public static Socket[] proxy;
        public static void Send()
        {
           
            int size;
            byte[] vs = new byte[8 * 1024];            
            while (true)
            {
                try
                {                   
                    if (!proxy[0].Connected || !proxy[1].Connected) break;
                    size = proxy[0].Receive(vs, SocketFlags.None);
                    proxy[1].Send(vs, size, SocketFlags.None);                  
                    if(size == 0)
                    {
                        proxy[0].Close();
                        proxy[1].Close();

                    }
                }
                catch
                {                 
                    break;
                }
            }
        }

        public static void Receive()
        {
            int size;
            byte[] vs = new byte[8 * 1024];          
            while (true)
            {                
                try
                {                    
                    if (!proxy[0].Connected || !proxy[1].Connected) break;
                    size = proxy[1].Receive(vs, SocketFlags.None);
                    proxy[0].Send(vs, size, SocketFlags.None);
                    if (size == 0)
                    {
                        proxy[0].Close();
                        proxy[1].Close();

                    }
                }
                catch
                {                   
                    break;
                }
            }
        }

        public static void Control(object socket)
        {
            Socket Cilent = (Socket)socket;
            Socket ToServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ToServer.Connect(new IPEndPoint(Program.Hypixel.AddressList[0], 25565));          
            Thread send = new Thread(Send);
            Thread receive = new Thread(Receive);
            Socket[] proxys ={Cilent,ToServer};
            proxy = proxys;
            send.Start();
            receive.Start();
        }
    }
}
