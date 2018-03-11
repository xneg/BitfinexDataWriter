namespace BitfinexDataWriter.DataWriter
{
    /// <summary>
    /// Тип провайдера записи данных.
    /// </summary>
    public enum DataWriterType
    {
        /// <summary>
        /// Вывод в консоль.
        /// </summary>
        Console = 1,

        /// <summary>
        /// Бинарный файл.
        /// </summary>
        BinaryFile = 2,
    }
}
