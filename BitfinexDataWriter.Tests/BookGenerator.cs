using BitfinexDataWriter.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace BitfinexDataWriter.Tests
{
    static class BookGenerator
    {
        public static Book GenerateBook(int seed)
        {
            var random = new Random(seed);

            var amountSign = random.Next(2) == 0 ? 1 : -1;

            return new Book { Amount = random.Next() * amountSign, Count = random.Next(), Price = random.Next() };
        }
    }
}
