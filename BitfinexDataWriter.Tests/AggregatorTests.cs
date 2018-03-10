using BitfinexDataWriter.Aggregator;
using BitfinexDataWriter.DataWriter;
using BitfinexDataWriter.Responses;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace BitfinexDataWriter.Tests
{
    public class AggregatorTests
    {
        private const string instrumentName = "instrument";
        private const int channelId = 1;

        private readonly Mock<IDataWriter> mockedDataWriter = new Mock<IDataWriter>();

        [Fact]
        public void SnapshotTest()
        {
            var random = new Random(13);
            var books = Enumerable.Range(1, 5).Select(r => BookGenerator.GenerateBook(random.Next())).ToArray();

            var bestBid = books.Where(b => b.Count > 0 && b.Amount > 0).Max(b => b.Price);
            var bestAsk = books.Where(b => b.Count > 0 && b.Amount < 0).Min(b => b.Price);

            var aggregator = new BookAggregator(mockedDataWriter.Object, channelId, instrumentName);

            mockedDataWriter
                .Setup(d => d.Write(It.IsAny<ResultData>()))
                .Callback((ResultData data) =>
                {
                    Assert.Equal(data.InstrumentName, instrumentName);
                    Assert.Equal(data.BestAsk, bestAsk);
                    Assert.Equal(data.BestBid, bestBid);
                });

            aggregator.GetSnapshot(books);

            mockedDataWriter.Verify(d => d.Write(It.IsAny<ResultData>()), Times.Once);
            mockedDataWriter.ResetCalls();
        }

        [Fact]
        public void UpdateNotChangingTest()
        {
            var (aggregator, bestBid, bestAsk) = PrepareAggregator(17);

            mockedDataWriter
                .Setup(d => d.Write(It.IsAny<ResultData>()))
                .Callback((ResultData data) =>
                {
                    Assert.Equal(data.InstrumentName, instrumentName);
                    Assert.Equal(data.BestAsk, bestAsk);
                    Assert.Equal(data.BestBid, bestBid);
                });

            var newBidBook = new Book { Amount = 1, Count = 1, Price = bestBid - 1 };

            aggregator.GetBook(newBidBook);

            var newAskBook = new Book { Amount = -1, Count = 1, Price = bestAsk + 1 };

            aggregator.GetBook(newAskBook);

            mockedDataWriter.Verify(d => d.Write(It.IsAny<ResultData>()), Times.Never);
        }

        [Fact]
        public void UpdateChangingTest()
        {
            var (aggregator, bestBid, bestAsk) = PrepareAggregator(19);

            mockedDataWriter
                .Setup(d => d.Write(It.IsAny<ResultData>()))
                .Callback((ResultData data) =>
                {
                    Assert.Equal(data.InstrumentName, instrumentName);
                    Assert.Equal(data.BestAsk, bestAsk);
                    Assert.Equal(data.BestBid, bestBid);
                });

            var newBidBook = new Book { Amount = 1, Count = 1, Price = bestBid + 1 };
            bestBid = newBidBook.Price;

            aggregator.GetBook(newBidBook);

            var newAskBook = new Book { Amount = -1, Count = 1, Price = bestAsk - 1 };
            bestAsk = newAskBook.Price;

            aggregator.GetBook(newAskBook);

            mockedDataWriter.Verify(d => d.Write(It.IsAny<ResultData>()), Times.Exactly(2));
        }

        [Fact]
        public void DeleteChangingTest()
        {
            var (aggregator, bestBid, bestAsk) = PrepareAggregator(23);

            mockedDataWriter
                .Setup(d => d.Write(It.IsAny<ResultData>()))
                .Callback((ResultData data) =>
                {
                    Assert.Equal(data.InstrumentName, instrumentName);
                    Assert.Equal(data.BestAsk, bestAsk);
                });

            var deleteBook = new Book { Count = 0, Amount = 1, Price = bestBid };

            aggregator.GetBook(deleteBook);

            mockedDataWriter.Verify(d => d.Write(It.IsAny<ResultData>()), Times.Once);
        }

        private (IAggregator aggregator, double bestBid, double bestAsk) PrepareAggregator(int seed)
        {
            var random = new Random(seed);
            var aggregator = new BookAggregator(mockedDataWriter.Object, channelId, instrumentName);
            var books = Enumerable.Range(1, 100).Select(r => BookGenerator.GenerateBook(random.Next())).ToArray();

            var bestBid = books.Where(b => b.Count > 0 && b.Amount > 0).Max(b => b.Price);
            var bestAsk = books.Where(b => b.Count > 0 && b.Amount < 0).Min(b => b.Price);

            aggregator.GetSnapshot(books);
            mockedDataWriter.ResetCalls();

            return (aggregator, bestBid, bestAsk);
        }
    }
}
