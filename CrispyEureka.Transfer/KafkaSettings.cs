using Confluent.Kafka;

namespace CrispyEureka.Transfer
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; }

        public int PartitionsCount { get; set; }
        public short ReplicationFactor { get; set; }
        public SecurityProtocol SecurityProtocol { get; set; }
    }
}