using System;
using System.Linq;
using System.Threading;
using BitfinexDataWriter.Aggregator;
using BitfinexDataWriter.Messages;

namespace BitfinexDataWriter
{
    internal class Program
    {
        private static void Main(string[] args)
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

            var aggregatorType = AggregatorType.Books;

            if (argsList.Contains("--raw"))
            {
                aggregatorType = AggregatorType.RawBooks;
                argsList.Remove("--raw");
            }

            IAggregatorsStorage aggregatorsStorage = new AggregatorsStorage(writerType);
            var client = new BitfitnexClient(uri);

            client.OnSubscribeResponse += aggregatorsStorage.OnChannelCreated;
            client.OnDataReceive += aggregatorsStorage.OnDataReceive;

            Console.WriteLine("Start listen...");

            client.Listen(source.Token);

            client.Send(PingMessage.CreateMessage(5678).Serialized, source.Token).Wait();

            if (argsList.Count != 0)
            {
                foreach (var arg in argsList)
                {
                    if (aggregatorType == AggregatorType.Books)
                        client.Send(SubscribeMessage.SubscribeToBookMessage(arg).Serialized, source.Token).Wait();
                    else
                        client.Send(SubscribeMessage.SubscribeToRawBookMessage(arg).Serialized, source.Token).Wait();
                    Console.WriteLine($"Subscribed to {arg}...");
                }
            }
            else
            {
                client.Send(SubscribeMessage.SubscribeToBookMessage("BTCUSD").Serialized, source.Token).Wait();
            }

            Console.ReadLine();
            source.Cancel();
            Console.WriteLine("Finishing...");
            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }
    }
}
