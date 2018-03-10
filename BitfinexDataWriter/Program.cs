using BitfinexDataWriter.Messages;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BitfinexDataWriter
{
    class Program
    {
        private const int receiveChunkSize = 256;

        static void Main(string[] args)
        {
            //var ws = new ClientWebSocket() { Options = { KeepAliveInterval = new TimeSpan(0, 0, 0, 10) } };
            var uri = new Uri("wss://api.bitfinex.com/ws/2");

            var client = new BitfitnexClient(uri);

            client.Listen(CancellationToken.None);

            client.Send(PingMessage.CreateMessage(5678).Serialized, CancellationToken.None).Wait();

            client.Send(SubscribeMessage.CreateMessage("book", "BTCUSD").Serialized, CancellationToken.None).Wait();

            Console.ReadLine();
        }

        private static async Task Listen(ClientWebSocket client, CancellationToken token)
        {
            do
            {
                WebSocketReceiveResult result = null;
                var buffer = new byte[1000];
                var message = new ArraySegment<byte>(buffer);
                var resultMessage = new StringBuilder();
                do
                {
                    result = await client.ReceiveAsync(message, token);
                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    resultMessage.Append(receivedMessage);
                    if (result.MessageType != WebSocketMessageType.Text)
                        break;

                } while (!result.EndOfMessage);

                var received = resultMessage.ToString();
                Console.WriteLine($"{DateTime.UtcNow} : {received}");
                //Log.Debug(L($"Received:  {received}"));
                //_lastReceivedMsg = DateTime.UtcNow;
                //_messageReceivedSubject.OnNext(received);

            } while (client.State == WebSocketState.Open && !token.IsCancellationRequested);
        }
    }
}
