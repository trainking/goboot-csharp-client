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

        public ReceviceHandle handler;

        /// <summary>
        /// 关闭时的取消请求
        /// </summary>
        private CancellationTokenSource closeToken;

        public Client(string network, NetConfig config, int heart, ReceviceHandle h)
        {
             switch(network) {
                case "tcp":
                    conn = new TcpNetConn(config);
                    break;
                default:
                    throw new Exception("no implement protocol");
            }

            closeToken = new CancellationTokenSource();
            handler += h;

            // 开启接收循环
            readLoop();

            // 保持心跳
            keepAlive(heart);
        }

        /// <summary>
        /// 保持连接的心跳
        /// </summary>
        /// <param name="heart"></param>
        private void keepAlive(int heart)
        {
            Task.Run(async () =>
            {
                while(!closeToken.IsCancellationRequested)
                {
                    conn.WritePacket(new DefaultPacket(0));
                    await Task.Delay(TimeSpan.FromSeconds(heart), closeToken.Token);
                }
                Console.WriteLine(closeToken.IsCancellationRequested);
            });

        }

        /// <summary>
        /// 读取包，循环
        /// </summary>
        private void readLoop()
        {
            Task.Run(async () =>
            {
                while (!closeToken.IsCancellationRequested)
                {
                    var packet = conn.ReadPacket();
                   if (packet.OpCode() == 0)
                    {
                        throw new Exception("close network.");
                    }
                    if (handler!= null)
                    {
                        handler(packet.OpCode(), packet.Body());
                    }
                    await Task.Delay(1);
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
            var p = new DefaultPacket(msg, opcode);
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