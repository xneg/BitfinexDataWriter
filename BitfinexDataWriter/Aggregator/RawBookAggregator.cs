using System;
using System.Collections.Generic;
using System.Linq;
using BitfinexDataWriter.DataWriter;
using BitfinexDataWriter.Orders;
using BitfinexDataWriter.Responses;

namespace BitfinexDataWriter.Aggregator
{
    public class RawBookAggregator : IAggregator<RawBook>
    {
        private readonly IDataWriter _dataWriter;
        private readonly int _channelId;
        private readonly string _instrument;
        private readonly Dictionary<ulong, Order> _asks = new Dictionary<ulong, Order>();
        private readonly Dictionary<ulong, Order> _bids = new Dictionary<ulong, Order>();

        private double _bestAsk;
        private double _bestBid;

        public RawBookAggregator(IDataWriter dataWriter, int channelId, string instrumentName)
        {
            _dataWriter = dataWriter;
            _channelId = channelId;
            _instrument = instrumentName;
        }

        public void GetBook(RawBook book)
        {
            AddOrder(FromRawBook(book));
            UpdateBestPrices();
        }

        public void GetSnapshot(RawBook[] books)
        {
            foreach (var order in books.Select(FromRawBook))
            {
                AddOrder(order);
            }

            UpdateBestPrices();
        }

        private (double Bid, double Ask) GetBestPrices()
        {
            var bestBid = _bids?.Values.Max(b => b.Price) ?? 0;
            var bestAsk = _asks?.Values.Min(a => a.Price) ?? 0;
            return (bestBid, bestAsk);
        }

        private void UpdateBestPrices()
        {
            var (bid, ask) = GetBestPrices();
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

        private Order FromRawBook(RawBook book)
        {
            var priceType = book.Amount > 0 ? PriceType.Bid : PriceType.Ask;

            var orderId = book.OrderId;

            if (book.Price > 0)
            {
                return new Order(orderId, priceType, book.Price, book.Amount);
            }
            else
            {
                return Order.ToDelete(priceType, book.Price);
            }
        }

        private void AddOrder(Order order)
        {
            var key = order.OrderId;

            switch (order.PriceType)
            {
                case PriceType.Ask:
                    if (order.NeedDelete)
                    {
                        _asks.Remove(key);
                    }
                    else
                    {
                        _asks.TryGetValue(key, out var currentOrder);
                        _asks[key] = currentOrder + order;
                        if (_asks[key].Amount <= 0)
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
                        _bids.TryGetValue(key, out var currentOrder);
                        _bids[key] = currentOrder + order;
                        if (_bids[key].Amount <= 0)
                        {
                            _bids.Remove(key);
                        }
                    }

                    break;
            }
        }

        //Trading: if AMOUNT > 0 then bid else ask; Funding: if AMOUNT< 0 then bid else ask;

        //Algorithm to create and keep a book instance updated

        //subscribe to channel with R0 precision
        //receive the raw book snapshot and create your in-memory book structure
        //when PRICE > 0 then you have to add or update the order
        //when PRICE = 0 then you have to delete the order
    }
}
