using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace goboot_csharp_client
{
    public interface Packet
    {
        /// <summary>
        /// 序列化消息输出
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize();

        /// <summary>
        /// 获取OpCode
        /// </summary>
        /// <returns></returns>
        public ushort OpCode();

        /// <summary>
        /// 获取Body字节数
        /// </summary>
        /// <returns></returns>
        public ushort BodyLen();

        /// <summary>
        /// 获取Body
        /// </summary>
        /// <returns></returns>
        public byte[] Body();
    }

    public class DefaultPacket : Packet
    {
        private byte[] buff;

        public DefaultPacket(byte[] b, ushort opcode) 
        { 
            buff = new byte[b.Length+4];

            // body长度复制
            byte[] lenB = BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder(b.Length));
            Array.Copy(lenB, 0, buff, 0, lenB.Length);

            // opcode复制进去
            byte[] opcodB = BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder(opcode));
            Array.Copy(opcodB, 0, buff, 2,opcodB.Length);

            if (b.Length > 0)
            {
                Array.Copy(b, 0, buff, 4, b.Length);
            }
        }

        public DefaultPacket(ushort opcode) 
        {
            buff = new byte[4];
            // body长度复制
            byte[] lenB = BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder(0));
            Array.Copy(lenB, 0, buff, 0, lenB.Length);

            // opcode复制进去
            byte[] opcodB = BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder(opcode));
            Array.Copy(opcodB, 0, buff, 2, opcodB.Length);
        }

        public DefaultPacket(byte[] b)
        {
            buff = b;
        }

        public DefaultPacket(NetworkStream stream)
        {
            byte[] head = new byte[4];
            int bytesRead = stream.Read(head, 0, head.Length);
            ushort bodyLen = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(head, 0));
            ushort opCode = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(head, 2));

            buff = new byte[bodyLen + 4];

            Array.Copy(head, 0, buff, 0, head.Length);

            if (bodyLen > 0)
            {
                stream.Read(buff, 4, bodyLen);
            }
        }

        public byte[] Body()
        {
            return buff[4..];
        }

        public ushort BodyLen()
        {
            return (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(buff, 0));
        }

        public ushort OpCode()
        {
            return (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(buff, 2));
        }

        public byte[] Serialize()
        {
            return buff;
        }
    } 
}
