using BitfinexDataWriter.Aggregator;
using BitfinexDataWriter.Messages;
using System;
using System.Linq;
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

            var argsList = args.ToList();

            var writerType = DataWriter.DataWriterType.BinaryFile;

            if (argsList.Contains("--binary"))
            {
                writerType = DataWriter.DataWriterType.BinaryFile;
                argsList.Remove("--binary");
            }
            else if (argsList.Contains("--console"))
            {
                writerType = DataWriter.DataWriterType.Console;
                argsList.Remove("--console");
            }

            var client = new BitfitnexClient(new AggregatorsStorage(writerType), uri);

            client.Listen(source.Token);

            client.Send(PingMessage.CreateMessage(5678).Serialized, source.Token).Wait();

            if (argsList.Count != 0)
            {
                foreach (var arg in argsList)
                    client.Send(SubscribeMessage.CreateMessage("book", arg).Serialized, source.Token).Wait();
            }
            else
            {
                client.Send(SubscribeMessage.CreateMessage("book", "BTCUSD").Serialized, source.Token).Wait();
            }

            Console.ReadLine();
            source.Cancel();
            Console.ReadLine();
        }
    }
}
