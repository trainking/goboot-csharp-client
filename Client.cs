using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace goboot_csharp_client
{
    public delegate void ReceviceHandle(ushort opcode, byte[] body);

    public class Client
    {

        private NetConn conn;
        private NetConfig config;

        /// <summary>
        /// 关闭时的取消请求
        /// </summary>
        private bool isClose;

        public Client(string network, Action<Packet> h)
        {
            switch (network)
            {
                case "tcp":
                    conn = new TcpNetConn(h);
                    break;
                case "kcp":
                    conn = new KcpNetConn(h);
                    break;
                case "websocket":
                    conn = new WebSocketConn(h);
                    break;
                default:
                    throw new Exception("no implement protocol");
            }
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <param name="addr"></param>
        public async void Connect(string addr)
        {
            if (isClose)
            {
                isClose = false;
            }
            await conn.Connect(addr);
        }

        /// <summary>
        /// 保持连接的心跳
        /// </summary>
        /// <param name="heart"></param>
        public async void KeepAlive(int heart)
        {
            await Task.Run(async () =>
            {
                while (!isClose)
                {
                    await conn.WritePacket(new DefaultPacket(0));
                    await Task.Delay(TimeSpan.FromSeconds(heart));
                }
            });

        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="opcode"></param>
        /// <param name="msg"></param>
        public void Send(ushort opcode, byte[] msg)
        {
            var p = new DefaultPacket((ushort)msg.Length, opcode);
            p.WriteBody(msg);
            conn.WritePacket(p);
        }

        /// <summary>
        /// 关闭客户端
        /// </summary>
        public void Close()
        {
            conn.Close();
            isClose = true;
        }
    }
}