using System;

namespace BitfinexDataWriter.Orders
{
    public struct Order
    {
        public PriceType PriceType { get; }

        public double Price { get; }

        public int Count { get; }

        public bool NeedDelete => Count == 0;

        public Order(PriceType priceType, double price, int count)
        {
            PriceType = priceType;
            Price = price;
            Count = count;
        }
        
        public static Order ToDelete(PriceType priceType, double price)
        {
            return new Order(priceType, price, 0);
        }

        public static Order operator +(Order o1, Order o2)
        {
            if (o1.PriceType != o2.PriceType || o1.Price != o2.Price)
            {
                throw new ArgumentException();
            }

            return new Order(o1.PriceType, o1.Price, o1.Count + o2.Count);
        }
    }
}
