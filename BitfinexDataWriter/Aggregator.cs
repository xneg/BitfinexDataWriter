using BitfinexDataWriter.Responses;
using System;

namespace BitfinexDataWriter
{
    public class Aggregator
    {
        private int _channelId;
        private string _instrument;

        public int ChannelId => _channelId;
        public string Instrument => _instrument;

        public Aggregator(int channelId, string instrument)
        {
            _channelId = channelId;
            _instrument = instrument;
        }

        public void GetBook(Book book)
        {
            Console.WriteLine("Getting book!");
        }

        public void GetSnapshot(Book[] books)
        {
            Console.WriteLine("Getting snapshot!");
        }
    }
}