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

        private TcpClient client;
        private Action<Packet> handle;
        private bool isClose;

        public TcpNetConn(Action<Packet> handler) 
        {
            client = new TcpClient();
            this.handle = handler;
            
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public async Task Connect(string addr)
        {
            var _addrA = addr.Split(':');
            await client.ConnectAsync(new IPEndPoint(IPAddress.Parse(_addrA[0]), int.Parse(_addrA[1])));

            // 开启消费消息
            onReceive();
        }

        /// <summary>
        /// 消息消费
        /// </summary>
        private void onReceive()
        {
            Task.Run(() =>
            {
                while(!this.isClose)
                {
                    byte[] headB = new byte[4];
                    NetworkStream stream = client.GetStream();
                    int bytesRead = stream.Read(headB, 0, headB.Length);
                    if (bytesRead == 4) 
                    {
                        var head = new Head(headB);
                        var p = new DefaultPacket(head.bodyLen, head.opcode);
                        if (head.bodyLen > 0)
                        {
                            byte[] body = new byte[head.bodyLen];
                            stream.Read(body, 0, body.Length);
                            p.WriteBody(body);
                        }

                        // 处理数据
                        this.handle(p);
                    }
                }
            });
        }

        /// <summary>
        /// 写入发送包
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public async Task WritePacket(Packet packet)
        {
            var data = packet.Serialize();
            NetworkStream stream = client.GetStream();
            stream.Write(packet.Serialize(), 0, data.Length);
            await Task.Delay(1);
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            client.Close();
            isClose = true;
        }
    }
}
