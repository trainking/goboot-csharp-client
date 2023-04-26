using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace goboot_csharp_client
{
    public class TcpNetConn : NetConn
    {

        private TcpClient client;
        private NetConfig config;
        
        public TcpNetConn(NetConfig c) 
        {
            config = c;
            var _addrA = c.Addr.Split(':');
            if ( _addrA.Length == 2 )
            {
                client = new TcpClient(_addrA[0], int.Parse(_addrA[1]));
            }else{
                throw new Exception("address not is <host>:<port>");
            }
        }

        public void Close()
        {
            client.Close();
        }

        public Packet ReadPacket()
        {
            NetworkStream stream = client.GetStream();
            return new DefaultPacket(stream);
        }

        public void WritePacket(Packet packet)
        {
            NetworkStream stream = client.GetStream();
            stream.Write(packet.Serialize());
        }
    }
}
