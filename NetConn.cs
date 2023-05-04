using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace goboot_csharp_client
{
    /// <summary>
    /// 网络连接的抽象
    /// </summary>
    public interface NetConn
    {
        /// <summary>
        /// 连接服务
        /// </summary>
        /// <returns></returns>
        public Task Connect(string addr);

        /// <summary>
        /// 写入包
        /// </summary>
        /// <param name="packet"></param>
        public Task WritePacket(Packet packet);

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close();
    }

    /// <summary>
    /// 配置抽象
    /// </summary>
    public struct NetConfig
    {
        public string Addr;
        public int WriteTimeout;
        public int ReadTimeout;
        public string WebSocketPath;
    }
}
