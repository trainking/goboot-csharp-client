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

        public void WriteBody(byte[] b);

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

        /// <summary>
        /// 销毁该包
        /// </summary>
        public void Dispore();
    }

    /// <summary>
    /// 头定义
    /// </summary>
    public class Head
    {
        public ushort opcode { get; }
        public ushort bodyLen { get; }

        public Head(byte[] b)
        {
            byte[] bodyB = new byte[2];
            Array.Copy(b, 0, bodyB, 0, 2);
            bodyLen = BitConverter.ToUInt16(bodyB.Reverse().ToArray(), 0);
            byte[] opcodeB = new byte[2];
            Array.Copy(b, 2, opcodeB, 0, 2);
            opcode = BitConverter.ToUInt16(opcodeB.Reverse().ToArray(), 0);
        }
    }

    public class DefaultPacket : Packet
    {
        private ushort opcode;
        private ushort bodyLen;
        private readonly MemoryStream buff;

        public DefaultPacket(ushort bodyLen, ushort opcode) 
        { 
            buff= new MemoryStream(bodyLen + 4);
            this.opcode = opcode;
            this.bodyLen = bodyLen;

            // 写入头
            writeHead();
        }

        public DefaultPacket(ushort opcode) 
        {
            buff = new MemoryStream(4);
            this.opcode = opcode;
            bodyLen= 0;
            writeHead();
        }

        private void writeHead()
        {
            // body长度复制
            byte[] lenB = BitConverter.GetBytes((ushort)bodyLen);
            // 反转成大端序
            Array.Reverse(lenB);
            buff.Write(lenB, 0, lenB.Length);

            // opcode复制进去
            byte[] opcodeB = BitConverter.GetBytes((ushort)opcode);
            // 反转成大端序
            Array.Reverse(opcodeB);
            buff.Write(opcodeB, 0, opcodeB.Length);
        }

        public void WriteBody(byte[] b)
        {
            buff.Write(b, 0, b.Length);
        }


        public byte[] Body()
        {
            byte[] body = new byte[bodyLen];
            buff.Seek(4, SeekOrigin.Begin);
            buff.Read(body, 0, body.Length);
            return body;
        }

        public ushort BodyLen()
        {
            return bodyLen;
        }

        public ushort OpCode()
        {
            return opcode;
        }

        public byte[] Serialize()
        {
            byte[] data = new byte[bodyLen+4];
            buff.Seek(0, SeekOrigin.Begin);
            buff.Read(data, 0, data.Length);
            return data;
        }

        /// <summary>
        /// 销毁该包
        /// </summary>
        public void Dispore()
        {
            buff.Close();
        }
    } 
}
