using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace goboot_csharp_client
{
    public class TcpNetConn : NetConn
    {

        private EasyClient client;
        private NetConfig config;
        
        public TcpNetConn(NetConfig c, Action<Packet> handler) 
        {
            config = c;
            client = new EasyClient();
            client.Initialize(new PacketReceiveFilter(4), handler);
            
        }

        public async Task Connect()
        {
            var _addrA = config.Addr.Split(':');
            await client.ConnectAsync(new IPEndPoint(IPAddress.Parse(_addrA[0]), int.Parse(_addrA[1])));
        }

        public async Task WritePacket(Packet packet)
        {
            if (!client.IsConnected)
            {
                await this.Connect();
            }
            if (client.IsConnected)
            {
                client.Send(packet.Serialize());
                await Task.Delay(1);
            } else
            {
                Console.WriteLine("连接失败!");
            }
            
        }

        public void Close()
        {
            client.Close();
        }
    }
}
