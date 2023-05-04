using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Diagnostics;

namespace goboot_csharp_client
{
    public class WebSocketConn : NetConn
    {
        private ClientWebSocket client;
        private CancellationTokenSource closeToken;
        private Action<Packet> handle;
        private bool IsCoonected;


        public WebSocketConn(Action<Packet> handler)
        {
            client = new ClientWebSocket();
            closeToken = new CancellationTokenSource();
            closeToken.CancelAfter(TimeSpan.FromSeconds(30));
            handle = handler;
        }

        public async Task Connect(string addr)
        {
            var uri = new Uri(addr);
            await client.ConnectAsync(uri, closeToken.Token);
        }

        private void onReceive()
        {
            Task.Run(async () =>
            {
                while(!closeToken.Token.IsCancellationRequested)
                {
                    byte[] headB = new byte[4];
                    WebSocketReceiveResult resultH = await client.ReceiveAsync(new ArraySegment<byte>(headB), closeToken.Token);

                    if (resultH.MessageType == WebSocketMessageType.Close)
                    {
                        this.Close();
                        return;
                    }

                    var head = new Head(headB);
                    var p = new DefaultPacket(head.bodyLen, head.opcode);

                    if (head.bodyLen > 0 )
                    {
                        byte[] body = new byte[head.bodyLen];
                        WebSocketReceiveResult resultB = await client.ReceiveAsync(new ArraySegment<byte>(body), closeToken.Token);
                        p.WriteBody(body);
                    }

                    // 处理数据
                    this.handle(p);

                }
            });
        }

        public void Close()
        {
            closeToken.Cancel();
            client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }

        public async Task WritePacket(Packet packet)
        {
            //if (!this.IsCoonected)
            //{
            //    this.IsCoonected= true;
            //}

            if (!closeToken.Token.IsCancellationRequested)
            {
                ArraySegment<byte> message = new ArraySegment<byte>(packet.Serialize());
                await client.SendAsync(message, WebSocketMessageType.Binary, true, closeToken.Token);
            }
            
        }
    }
}
