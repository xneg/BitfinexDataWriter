using BitfinexDataWriter.Aggregator;

namespace BitfinexDataWriter.DataWriter
{
    public interface IDataWriter
    {
        void Write(ResultData data);
    }
}
