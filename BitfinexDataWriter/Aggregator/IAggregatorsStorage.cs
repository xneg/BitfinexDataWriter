﻿namespace BitfinexDataWriter.Aggregator
{
    /// <summary>
    /// Интерфейс хранилища агрегаторов.
    /// </summary>
    public interface IAggregatorsStorage
    {
        /// <summary>
        /// Создать агрегатор и добавить его в хранилище.
        /// </summary>
        /// <param name="channelId">ID канала.</param>
        /// <param name="instrumentName">Имя инструмента.</param>
        void CreateAggregator(int channelId, string instrumentName);

        /// <summary>
        /// Получить агрегатор по ID канала.
        /// </summary>
        /// <param name="channelId">ID канала.</param>
        /// <returns>Экземпляр агрегатора.</returns>
        IAggregator GetAggregator(int channelId);
    }
}
