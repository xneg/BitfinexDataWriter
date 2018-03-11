using System;

namespace BitfinexDataWriter.Orders
{
    public class Order
    {
        public ulong OrderId { get; }

        public PriceType PriceType { get; }

        public double Price { get; }

        public double Amount { get; }

        public bool NeedDelete => Amount == 0;

        public Order(PriceType priceType, double price, double amount) :
            this (0, priceType, price, amount)
        {
        }

        public Order(ulong orderId, PriceType priceType, double price, double amount)
        {
            OrderId = orderId;
            PriceType = priceType;
            Price = price;
            Amount = amount;
        }

        public static Order ToDelete(PriceType priceType, double price) => ToDelete(0, priceType, price);

        public static Order ToDelete(ulong orderId, PriceType priceType, double price) => new Order(orderId, priceType, price, 0);

        public static Order operator +(Order o1, Order o2)
        {
            if (o1 == null && o2 == null)
            {
                throw new ArgumentException();
            }

            if (o1 == null)
            {
                return o2;
            }

            if (o2 == null)
            {
                return o1;
            }

            if (o1.PriceType != o2.PriceType || o1.Price != o2.Price)
            {
                throw new ArgumentException();
            }

            return new Order(o1.PriceType, o1.Price, o1.Amount + o2.Amount);
        }
    }
}
