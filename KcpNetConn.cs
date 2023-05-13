using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net.Sockets.Kcp;
using System.Net.Sockets.Kcp.Simple;
using System.Threading.Tasks;
using System.Net;

namespace goboot_csharp_client
{
    public class KcpNetConn : NetConn
    {
        private SimpleKcpClient client = null;
        private Action<Packet> handle;
        private bool isClose;


        public KcpNetConn(Action<Packet> handle)
        {
            this.handle = handle;
        }

        public async Task Connect(string addr)
        {
            var _addrA = addr.Split(':');
            client = new SimpleKcpClient(0, new IPEndPoint(IPAddress.Parse(_addrA[0]), int.Parse(_addrA[1])));

            OnUpdate();  // kcp 需要定时update
            OnReceive();
            await Task.Delay(1);
        }

        private void OnUpdate()
        {
            Task.Run(async () =>
            {
                while (!this.isClose)
                {
                    client.kcp.Update(DateTimeOffset.UtcNow);
                    await Task.Delay(10);
                }
            });
        }

        private void OnReceive()
        {
            Task.Run(async () =>
            {
                while(!this.isClose)
                {
                    byte[] headB = new byte[4];

                    var bytes = await client.ReceiveAsync();
                    
                    if (bytes.Length > 0 )
                    {
                        var head = new Head(bytes[0..4]);
                        var p = new DefaultPacket(head.bodyLen, head.opcode);
                        if (head.bodyLen > 0)
                        {
                            p.WriteBody(bytes[4..bytes.Length]);
                        }

                        this.handle(p);
                    }
                }
            });
        }

        public void Close()
        {
            isClose = true;
        }


        public async Task WritePacket(Packet packet)
        {
            if (!this.isClose)
            {
                var data = packet.Serialize();
                client.SendAsync(data, data.Length);
                await Task.Delay(1);
            }
        }
    }
}
