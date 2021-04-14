using System.IO;
using Confluent.Kafka;
using CrispyEureka.Transfer;
using ProtoBuf;
using SerializationContext = Confluent.Kafka.SerializationContext;

namespace CrispyEureka.MarketDataConnector.Kafka
{
    public class ProtobufSerializer<TPayloadMessage> : ISerializer<TransferMessage<TPayloadMessage>>
        where TPayloadMessage : IMessagePayload
    {
        public byte[] Serialize(TransferMessage<TPayloadMessage> data, SerializationContext context)
        {
            using var encodeStream = new MemoryStream();
            Serializer.Serialize(encodeStream, data);
            return encodeStream.ToArray();
        }
    }
}