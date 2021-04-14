using ProtoBuf;

namespace CrispyEureka.Transfer.Models
{
    [ProtoContract]
    public class CandleTransferModel : IMessagePayload
    {
        [ProtoMember(1)]
        public string Figi { get; set; }
        [ProtoMember(2)]
        public decimal Open { get; set; }
        [ProtoMember(3)]
        public decimal Close { get; set; }
        [ProtoMember(4)]
        public decimal High { get; set; }
        [ProtoMember(5)]
        public decimal Low { get; set; }
        [ProtoMember(6)]
        public decimal Volume { get; set; }
        [ProtoMember(7)]
        public long Timestamp { get; set; }
        [ProtoMember(8)]
        public int Interval { get; set; }
    }
}