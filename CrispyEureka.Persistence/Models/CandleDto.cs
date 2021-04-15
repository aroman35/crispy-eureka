using System;
using CrispyEureka.Domain.MarketData.Candle;

namespace CrispyEureka.Persistence.Models
{
    public class CandleDto : IDto<Candle>
    {
        public CandleDto()
        {
            Id = Guid.NewGuid();
        }
        public decimal Open { get; set; }
        public decimal Close { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Volume { get; set; }
        public long Timestamp { get; set; }
        public int Interval { get; set; }
        public string Figi { get; set; }
        public Guid Id { get; private set; }
        
        public Candle ToRoot()
        {
            var candle = new Candle(Open, Close, High, Low, Volume, new DateTime(Timestamp), (Interval)Interval, Figi);
            return candle;
        }
    }
}