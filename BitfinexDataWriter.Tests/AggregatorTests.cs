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
            var books = BookGenerator.GenerateBooks(13, 100);

            var bestBid = books.BestBid();
            var bestAsk = books.BestAsk();

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
            var books = BookGenerator.GenerateBooks(17, 100);
            var aggregator = PrepareAggregator(books);

            mockedDataWriter
                .Setup(d => d.Write(It.IsAny<ResultData>()))
                .Callback((ResultData data) =>
                {
                    Assert.Equal(data.InstrumentName, instrumentName);
                    Assert.Equal(data.BestAsk, books.BestAsk());
                    Assert.Equal(data.BestBid, books.BestBid());
                });

            var newBidBook = new Book { Amount = 1, Count = 1, Price = books.BestBid() - 1 };

            aggregator.GetBook(newBidBook);

            var newAskBook = new Book { Amount = -1, Count = 1, Price = books.BestAsk() + 1 };

            aggregator.GetBook(newAskBook);

            mockedDataWriter.Verify(d => d.Write(It.IsAny<ResultData>()), Times.Never);
        }

        [Fact]
        public void UpdateChangingTest()
        {
            var books = BookGenerator.GenerateBooks(19, 100);
            var aggregator = PrepareAggregator(books);

            var bestAsk = books.BestAsk();
            var bestBid = books.BestBid();

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
            var books = BookGenerator.GenerateBooks(23, 100);
            var aggregator = PrepareAggregator(books);

            var bestAsk = books.BestAsk();
            var bestBid = books.BestBid();

            mockedDataWriter
                .Setup(d => d.Write(It.IsAny<ResultData>()))
                .Callback((ResultData data) =>
                {
                    Assert.Equal(data.InstrumentName, instrumentName);
                    Assert.Equal(data.BestAsk, bestAsk);
                    Assert.Equal(data.BestBid, bestBid);
                });

            var deleteBook = new Book { Count = 0, Amount = 1, Price = bestBid };
            bestBid = books.Where(b => b.Count > 0 && b.Amount > 0).OrderByDescending(b => b.Price).Skip(1).First().Price;

            aggregator.GetBook(deleteBook);

            mockedDataWriter.Verify(d => d.Write(It.IsAny<ResultData>()), Times.Once);
        }

        private IAggregator PrepareAggregator(Book[] books)
        {
            var aggregator = new BookAggregator(mockedDataWriter.Object, channelId, instrumentName);

            aggregator.GetSnapshot(books);
            mockedDataWriter.ResetCalls();

            return aggregator;
        }
    }
}
