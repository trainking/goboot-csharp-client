using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ClientEngine;
using SuperSocket.ProtoBase;

namespace goboot_csharp_client
{
    internal class PacketReceiveFilter : FixedHeaderReceiveFilter<Packet>
    {

        private int headerSize;

        public PacketReceiveFilter(int headerSize) : base(headerSize)
        {
            this.headerSize = headerSize;
        }

        /// <summary>
        /// 读取baody
        /// </summary>
        /// <param name="bufferStream"></param>
        /// <returns></returns>
        public override Packet ResolvePackage(IBufferStream bufferStream)
        {
            byte[] head = new byte[headerSize];
            bufferStream.Read(head, 0, head.Length);
            ushort bodyLen = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(head, 0));
            ushort opcode = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(head, 2));
            byte[] body = new byte[bodyLen];
            bufferStream.Read(body, 0, body.Length);

            var p = new DefaultPacket((ushort)body.Length, opcode);
            p.WriteBody(body);

            return p;
        }

        /// <summary>
        /// 获取头
        /// </summary>
        /// <param name="bufferStream"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected override int GetBodyLengthFromHeader(IBufferStream bufferStream, int length)
        {
            byte[] head = new byte[length];
            bufferStream.Read(head, 0, length);
            ushort bodyLen = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(head, 0));
            return bodyLen;
        }
    }
}
