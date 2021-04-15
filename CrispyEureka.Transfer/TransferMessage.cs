using ProtoBuf;

namespace CrispyEureka.Transfer
{
    [ProtoContract]
    public class TransferMessage<TPayload>
        where TPayload : IMessagePayload
    {
        protected TransferMessage()
        {
            
        }
        
        public TransferMessage(string key, TPayload payload)
        {
            Key = key;
            MessagePayload = payload;
        }
        
        [ProtoMember(1)]
        public string Key { get; set; }
        [ProtoMember(2)]
        public TPayload MessagePayload { get; set; }
        public string Figi => MessagePayload.Figi;
    }
}