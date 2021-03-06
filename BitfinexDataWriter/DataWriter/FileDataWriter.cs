﻿using System;
using System.IO;
using BitfinexDataWriter.Aggregator;

namespace BitfinexDataWriter.DataWriter
{
    public class FileDataWriter : IDataWriter
    {
        private const double Exponent = 1e-8;
        private string _fileName;

        public FileDataWriter(string instrumentName, AggregatorType aggregatorType)
        {
            var appDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            var filePath = Path.Combine(appDirectory, instrumentName, aggregatorType.ToString());
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            _fileName = Path.Combine(filePath, $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.dat");
        }

        public void Write(ResultData data)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(_fileName, FileMode.Append)))
            {
                writer.Write(ToUnixTime(data.DateTime));
                writer.Write(GetFixed(data.BestBid));
                writer.Write(GetFixed(data.BestAsk));
            }
        }

        private static UInt64 ToUnixTime(DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToUInt64((date - epoch).TotalSeconds);
        }

        private static Int64 GetFixed(double value)
        {
            return Convert.ToInt64(value / Exponent);
        }
    }
}
