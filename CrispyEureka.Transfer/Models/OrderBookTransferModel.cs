using ProtoBuf;

namespace CrispyEureka.Transfer.Models
{
    [ProtoContract]
    public class OrderBookTransferModel : IMessagePayload
    {
        [ProtoMember(1)]
        public string Figi { get; set; }
        [ProtoMember(2)]
        public long Timestamp { get; set; }
        [ProtoMember(3)]
        public OrderBookEntry[] Asks { get; set; }
        [ProtoMember(4)]
        public OrderBookEntry[] Bids { get; set; }
    }

    [ProtoContract]
    public class OrderBookEntry
    {
        [ProtoMember(1)]
        public int Quantity { get; set; }
        [ProtoMember(2)]
        public decimal Price { get; set; }
    }
}