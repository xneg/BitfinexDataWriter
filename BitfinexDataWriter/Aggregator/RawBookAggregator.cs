using System;
using BitfinexDataWriter.Responses;

namespace BitfinexDataWriter.Aggregator
{
    public class RawBookAggregator : IAggregator
    {
        public void GetBook(Book book)
        {
            throw new NotImplementedException();
        }

        public void GetSnapshot(Book[] books)
        {
            throw new NotImplementedException();
        }
    }
}
