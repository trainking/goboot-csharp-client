using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace goboot_csharp_client
{
    public class TcpNetConn : NetConn
    {

        private TcpClient client;
        private NetConfig config;
        private NetworkStream stream;
        
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

            stream = client.GetStream();
        }


        public void Close()
        {
            stream.Close();
            client.Close();
        }

        public Packet ReadPacket()
        {            
            return new DefaultPacket(stream);
        }

        public void WritePacket(Packet packet)
        {
            if (client.Connected)
            {
                stream.Write(packet.Serialize(), 0, packet.Serialize().Length);
            }  
        }
    }
}
