# goboot-csharp-client

这是为`goboot`的`gameapi`长链接服务器的建立的C#客户端，方便引入到Unity中使用。

## 快速开始

### TCP

```C#
    private static void Main(string[] args)
        {

            try
            {
                var client = new Client("tcp", (Packet p) =>
                {
                    switch ((Pb.OpCode)p.OpCode())
                    {
                        case Pb.OpCode.OpS2CLogin:
                            var message = S2C_Login.Parser.ParseFrom(p.Body());
                            Console.WriteLine($"S2C_Login: {message}");
                            break;
                    }
                    p.Dispore();
                });

                client.Connect("192.168.1.10:6001");
                client.KeepAlive(3);

                var sendMsg = new C2S_Login()
                {
                    Account = "1",
                    Password = "123456",
                };
                client.Send((ushort)Pb.OpCode.OpC2SLogin, sendMsg.ToByteArray());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            

            Console.WriteLine($"Send Msg Success!");
            Thread.Sleep(20 * 1000);
        }
    }
```

### WebSocket

```C#
    private static void Main(string[] args)
        {

            try
            {
                var client = new Client("websocket", (Packet p) =>
                {
                    switch ((Pb.OpCode)p.OpCode())
                    {
                        case Pb.OpCode.OpS2CLogin:
                            var message = S2C_Login.Parser.ParseFrom(p.Body());
                            Console.WriteLine($"S2C_Login: {message}");
                            break;
                    }
                    p.Dispore();
                });

                client.Connect("ws://192.168.1.10:6001/ws");
                client.KeepAlive(3);

                var sendMsg = new C2S_Login()
                {
                    Account = "1",
                    Password = "123456",
                };
                client.Send((ushort)Pb.OpCode.OpC2SLogin, sendMsg.ToByteArray());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            

            Console.WriteLine($"Send Msg Success!");
            Thread.Sleep(20 * 1000);
        }
    }
```
