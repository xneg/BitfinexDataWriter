using System.Collections.Generic;
using System.Linq;
using BitfinexDataWriter.DataWriter;
using BitfinexDataWriter.Orders;
using BitfinexDataWriter.Responses;

namespace BitfinexDataWriter.Aggregator
{
    /// <summary>
    /// Реализация агрегатора для канала Books.
    /// </summary>
    public class BookAggregator : BaseAggregator, IAggregator<Book>
    {
        private readonly int _channelId;

        private readonly SortedDictionary<double, double> _asks = new SortedDictionary<double, double>();
        private readonly SortedDictionary<double, double> _bids = new SortedDictionary<double, double>();

        public BookAggregator(IDataWriter dataWriter, int channelId, string instrument) 
            : base(dataWriter, instrument)
        {
            _channelId = channelId;
        }

        public void GetBook(Book book)
        {
            AddOrder(FromBook(book));
            UpdateBestPrices();
        }

        public void GetSnapshot(Book[] books)
        {
            foreach (var order in books.Select(FromBook))
            {
                AddOrder(order);
            }

            UpdateBestPrices();
        }

        private Order FromBook(Book book)
        {
            var priceType = book.Amount > 0 ? PriceType.Bid : PriceType.Ask;

            if (book.Count > 0)
            {
                return new Order(priceType, book.Price, book.Count);
            }
            else
            {
                return Order.ToDelete(priceType, book.Price);
            }
        }

        private void AddOrder(Order order)
        {

            switch (order.PriceType)
            {
                case PriceType.Ask:
                    var keyAsk = order.Price;

                    if (order.NeedDelete)
                    {
                        _asks.Remove(keyAsk);
                    }
                    else
                    {
                        _asks.TryGetValue(keyAsk, out var currentCount);
                        _asks[keyAsk] = currentCount + order.Amount;
                        if (_asks[keyAsk] <= 0)
                        {
                            _asks.Remove(keyAsk);
                        }
                    }

                    break;
                case PriceType.Bid:
                    var keyBid = -order.Price;

                    if (order.NeedDelete)
                    {
                        _bids.Remove(keyBid);
                    }
                    else
                    {
                        _bids.TryGetValue(keyBid, out var currentCount);
                        _bids[keyBid] = currentCount + order.Amount;
                        if (_bids[keyBid] <= 0)
                        {
                            _bids.Remove(keyBid);
                        }
                    }

                    break;
            }
        }

        protected override (double Bid, double Ask) GetBestPrices()
        {
            var bestBid = -_bids.Keys.FirstOrDefault();
            var bestAsk = _asks.Keys.FirstOrDefault();

            return (bestBid, bestAsk);
        }
    }
}