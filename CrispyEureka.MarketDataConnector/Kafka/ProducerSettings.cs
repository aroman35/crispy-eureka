namespace CrispyEureka.MarketDataConnector.Kafka
{
    public class ProducerSettings
    {
        public string ProducerId { get; set; }
        public int BatchSize { get; set; }
        public string TopicName { get; set; }
        public int SendTimeoutMs { get; set; }
    }

    public class ProducerSettings<TMessagePayload> : ProducerSettings
    {
    }
}