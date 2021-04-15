using CrispyEureka.Transfer;

namespace CrispyEureka.MarketDataConsumer
{
    public class ConsumerSettings
    {
        public bool IsEnabled { get; set; }
        public string ConsumerGroupId { get; set; }
        public int ConsumersCount { get; set; }
        public string TopicName { get; set; }
        public int BatchSize { get; set; }
    }
    
    public class ConsumerSettings<TMessagePayload> : ConsumerSettings
        where TMessagePayload: IMessagePayload
    {
    }
}