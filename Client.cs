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
        private CancellationTokenSource closeToken;

        public Client(string network, Action<Packet> h)
        {
             switch(network) {
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

            closeToken = new CancellationTokenSource();

        }

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <param name="addr"></param>
        public void Connect(string addr)
        {
            var task = this.conn.Connect(addr);
            task.Wait();
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
            conn.Close();
            closeToken.Cancel();
        }
    }
}