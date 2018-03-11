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

            IAggregatorsStorage aggregatorsStorage = new AggregatorsStorage(writerType);
            var client = new BitfitnexClient(uri);

            client.OnSubscribeResponse += aggregatorsStorage.OnChannelCreated;
            client.OnDataReceive += aggregatorsStorage.OnDataReceive;

            client.Listen(source.Token);

            client.Send(PingMessage.CreateMessage(5678).Serialized, source.Token).Wait();

            if (argsList.Count != 0)
            {
                foreach (var arg in argsList)
                {
                    client.Send(SubscribeMessage.SubscribeToRawBookMessage(arg).Serialized, source.Token).Wait();
                    //client.Send(SubscribeMessage.SubscribeToBookMessage(arg).Serialized, source.Token).Wait();
                }
            }
            else
            {
                client.Send(SubscribeMessage.SubscribeToBookMessage("BTCUSD").Serialized, source.Token).Wait();
            }

            Console.ReadLine();
            source.Cancel();
            Console.ReadLine();
        }
    }
}
