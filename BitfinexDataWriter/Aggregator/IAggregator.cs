using BitfinexDataWriter.Responses;

namespace BitfinexDataWriter.Aggregator
{
    /// <summary>
    /// Интерфейс агрегатора данных.
    /// </summary>
    public interface IAggregator
    {
        /// <summary>
        /// Получить первоначальный снимок.
        /// </summary>
        /// <param name="books">Список заявок.</param>
        void GetSnapshot(Book[] books);

        /// <summary>
        /// Получить заявку.
        /// </summary>
        /// <param name="book">Заявка.</param>
        void GetBook(Book book);
    }
}
