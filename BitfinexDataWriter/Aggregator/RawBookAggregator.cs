using System;
using System.Linq;
using BitfinexDataWriter.DataWriter;
using BitfinexDataWriter.Orders;
using BitfinexDataWriter.Responses;

namespace BitfinexDataWriter.Aggregator
{
    public class RawBookAggregator : IAggregator<RawBook>
    {
        private IDataWriter _dataWriter;
        private int _channelId;
        private string _instrumentName;

        public RawBookAggregator(IDataWriter dataWriter, int channelId, string instrumentName)
        {
            _dataWriter = dataWriter;
            _channelId = channelId;
            _instrumentName = instrumentName;
        }

        public void GetBook(RawBook book)
        {
            throw new NotImplementedException();
        }

        public void GetSnapshot(RawBook[] books)
        {
            foreach (var order in books.Select(FromRawBook))
            {
                var x = order;
                //AddOrder(order);
            }
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

        //Trading: if AMOUNT > 0 then bid else ask; Funding: if AMOUNT< 0 then bid else ask;

        //Algorithm to create and keep a book instance updated

        //subscribe to channel with R0 precision
        //receive the raw book snapshot and create your in-memory book structure
        //when PRICE > 0 then you have to add or update the order
        //when PRICE = 0 then you have to delete the order
    }
}
