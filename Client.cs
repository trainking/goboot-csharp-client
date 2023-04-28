using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace goboot_csharp_client
{
    public delegate void ReceviceHandle(ushort opcode, byte[] body);

    public class Client
    {
        public static Client ClientFactory(string network, NetConfig config, Action<Packet> h)
        {
            var client = new Client(network, config, h);
            // 心跳
            client.KeepAlive(3);
            return client;
        }

        private NetConn conn;
        private NetConfig config;

        /// <summary>
        /// 关闭时的取消请求
        /// </summary>
        private CancellationTokenSource closeToken;

        public Client(string network, NetConfig config, Action<Packet> h)
        {
             switch(network) {
                case "tcp":
                    conn = new TcpNetConn(config, h);
                    break;
                case "websocket":
                    conn = new WebSocketConn(config, h);
                    break;
                default:
                    throw new Exception("no implement protocol");
            }

            closeToken = new CancellationTokenSource();

        }

        /// <summary>
        /// 保持连接的心跳
        /// </summary>
        /// <param name="heart"></param>
        public void KeepAlive(int heart)
        {
            Task.Run(async () =>
            {
                while(!closeToken.IsCancellationRequested)
                {
                    await conn.WritePacket(new DefaultPacket(0));
                    await Task.Delay(TimeSpan.FromSeconds(heart), closeToken.Token);
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
            closeToken.Cancel();
        }
    }
}