namespace BitfinexDataWriter.Aggregator
{
    public interface IAggregator
    {
    }

    /// <summary>
    /// Интерфейс агрегатора данных.
    /// </summary>
    public interface IAggregator<TData> : IAggregator
    {
        /// <summary>
        /// Получить первоначальный снимок.
        /// </summary>
        /// <param name="books">Список заявок.</param>
        void GetSnapshot(TData[] books);

        /// <summary>
        /// Получить заявку.
        /// </summary>
        /// <param name="book">Заявка.</param>
        void GetBook(TData book);
    }
}
