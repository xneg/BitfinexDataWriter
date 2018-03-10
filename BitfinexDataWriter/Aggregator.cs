using BitfinexDataWriter.Responses;
using BitfinexDataWriter.Orders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BitfinexDataWriter
{
    public class Aggregator
    {
        private int _channelId;
        private string _instrument;

        private Dictionary<double, int> _asks = new Dictionary<double, int>();
        private Dictionary<double, int> _bids = new Dictionary<double, int>();
        private double _bestAsk;
        private double _bestBid;

        public int ChannelId => _channelId;

        public string Instrument => _instrument;

        public Aggregator(int channelId, string instrument)
        {
            _channelId = channelId;
            _instrument = instrument;
        }

        public void GetBook(Book book)
        {
            ManageOrder(FromBook(book));
            if (UpdateBestPrices())
                PrintBestPrices();
        }

        public void GetSnapshot(Book[] books)
        {
            foreach(var order in books.Select(FromBook))
            {
                ManageOrder(order);
            }
            if (UpdateBestPrices())
                PrintBestPrices();
        }

        private bool UpdateBestPrices()
        {
            var (bid, ask) = GetBestPrices();
            if (bid != _bestBid || ask != _bestAsk)
            {
                _bestBid = bid;
                _bestAsk = ask;
                return true;
            }
            return false;
        }
        
        private void PrintBestPrices()
        {
            Console.WriteLine($"{DateTime.UtcNow} : {_bestBid} {_bestAsk}");
        }

        private (double Bid, double Ask) GetBestPrices()
        {
            var bestBid = _bids.Keys.Max();
            var bestAsk = _asks.Keys.Min();
            return (bestBid, bestAsk);
        }

        private Order FromBook(Book book)
        {
            var priceType = book.Amount > 0 ? PriceType.Bid : PriceType.Ask;

            if (book.Count > 0)
                return new Order(priceType, book.Price, book.Count);
            else
                return Order.ToDelete(priceType, book.Price);
        }

        private void ManageOrder(Order order)
        {
            var key = order.Price;

            switch (order.PriceType)
            {
                case PriceType.Ask:
                    if (order.NeedDelete)
                        _asks.Remove(key);
                    else
                    {
                        _asks.TryGetValue(key, out var currentCount);
                        _asks[key] = currentCount + order.Count;
                    }
                    break;
                case PriceType.Bid:
                    if (order.NeedDelete)
                        _bids.Remove(key);
                    else
                    {
                        _bids.TryGetValue(key, out var currentCount);
                        _bids[key] = currentCount + order.Count;
                    }
                    break;
            }
        }
    }
}