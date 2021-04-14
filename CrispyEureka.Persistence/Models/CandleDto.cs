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
        public DateTime Timestamp { get; set; }
        public Interval Interval { get; set; }
        public string Figi { get; set; }
        public Guid Id { get; private set; }
        
        public Candle ToRoot()
        {
            var candle = new Candle(Open, Close, High, Low, Volume, Timestamp, Interval, Figi);
            return candle;
        }
    }
}