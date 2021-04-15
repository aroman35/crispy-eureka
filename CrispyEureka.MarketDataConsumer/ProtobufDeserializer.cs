using System;
using System.IO;
using Confluent.Kafka;
using CrispyEureka.Transfer;
using ProtoBuf;
using SerializationContext = Confluent.Kafka.SerializationContext;

namespace CrispyEureka.MarketDataConsumer
{
    public class ProtobufDeserializer<TMessagePayload> : IDeserializer<TransferMessage<TMessagePayload>>
        where TMessagePayload : IMessagePayload
    {
        public TransferMessage<TMessagePayload> Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            using var decodingStream = new MemoryStream(data.ToArray());
            var transferMessage = Serializer.Deserialize<TransferMessage<TMessagePayload>>(decodingStream);

            return transferMessage;
        }
    }
}