using BitfinexDataWriter.Responses;

namespace BitfinexDataWriter.Aggregator
{
    public interface IAggregator
    {
        void GetSnapshot(Book[] books);

        void GetBook(Book book);
    }
}
