using System;
using System.Linq;
using BitfinexDataWriter.Responses;

namespace BitfinexDataWriter.Tests
{
    internal static class BookGenerator
    {
        public static Book GenerateBook(int seed)
        {
            var random = new Random(seed);

            var amountSign = random.Next(2) == 0 ? 1 : -1;

            return new Book { Amount = random.Next() * amountSign, Count = random.Next(), Price = random.Next() };
        }

        public static Book[] GenerateBooks(int seed, int count)
        {
            var random = new Random(seed);
            return Enumerable.Range(1, count).Select(r => BookGenerator.GenerateBook(random.Next())).ToArray();
        }

        public static double BestBid(this Book[] books) => books.Where(b => b.Count > 0 && b.Amount > 0).Max(b => b.Price);

        public static double BestAsk(this Book[] books) => books.Where(b => b.Count > 0 && b.Amount < 0).Min(b => b.Price);
    }
}
