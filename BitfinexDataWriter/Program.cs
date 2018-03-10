using BitfinexDataWriter.Messages;
using System;
using System.Threading;

namespace BitfinexDataWriter
{
    class Program
    {
        // TODO: добавить в качестве аргументов имена инструментов
        static void Main(string[] args)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            var uri = new Uri("wss://api.bitfinex.com/ws/2");

            var client = new BitfitnexClient(uri);

            client.Listen(source.Token);

            client.Send(PingMessage.CreateMessage(5678).Serialized, source.Token).Wait();

            client.Send(SubscribeMessage.CreateMessage("book", "BTCUSD").Serialized, source.Token).Wait();

            Console.ReadLine();
            source.Cancel();
            Console.ReadLine();
        }
    }
}
