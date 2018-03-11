using System;
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
    public class BookAggregator : IAggregator<Book>
    {
        private readonly IDataWriter _dataWriter;
        private readonly int _channelId;
        private readonly string _instrument;
        private readonly Dictionary<double, double> _asks = new Dictionary<double, double>();
        private readonly Dictionary<double, double> _bids = new Dictionary<double, double>();

        private double _bestAsk;
        private double _bestBid;

        public BookAggregator(IDataWriter dataWriter, int channelId, string instrument)
        {
            _dataWriter = dataWriter ?? throw new ArgumentNullException(nameof(dataWriter));

            _channelId = channelId;
            _instrument = instrument;
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

        private void UpdateBestPrices()
        {
            var(bid, ask) = GetBestPrices();
            if (bid != _bestBid || ask != _bestAsk)
            {
                _bestBid = bid;
                _bestAsk = ask;
                _dataWriter.Write(GetResultData());
            }
        }

        private ResultData GetResultData()
        {
            return new ResultData(_instrument, DateTime.UtcNow, _bestBid, _bestAsk);
        }

        private (double Bid, double Ask) GetBestPrices()
        {
            var bidsValues = _bids.Keys;
            var bestBid = bidsValues.Any() ? bidsValues.Max() : 0;

            var askValues = _asks.Values;
            var bestAsk = askValues.Any() ? askValues.Min() : 0;

            return (bestBid, bestAsk);
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
            var key = order.Price;

            switch (order.PriceType)
            {
                case PriceType.Ask:
                    if (order.NeedDelete)
                    {
                        _asks.Remove(key);
                    }
                    else
                    {
                        _asks.TryGetValue(key, out var currentCount);
                        _asks[key] = currentCount + order.Amount;
                        if (_asks[key] <= 0)
                        {
                            _asks.Remove(key);
                        }
                    }

                    break;
                case PriceType.Bid:
                    if (order.NeedDelete)
                    {
                        _bids.Remove(key);
                    }
                    else
                    {
                        _bids.TryGetValue(key, out var currentCount);
                        _bids[key] = currentCount + order.Amount;
                        if (_bids[key] <= 0)
                        {
                            _bids.Remove(key);
                        }
                    }

                    break;
            }
        }
    }
}