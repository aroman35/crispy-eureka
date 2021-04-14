using System;

namespace CrispyEureka.Domain.MarketData.Candle
{
    public class Candle : AggregateRoot, IEquatable<Candle>, IComparable<Candle>
    {
        public Candle(
            decimal open,
            decimal close,
            decimal high,
            decimal low,
            decimal volume,
            DateTime timestamp,
            Interval interval,
            string figi)
        {
            Open = open;
            Close = close;
            High = high;
            Low = low;
            Volume = volume;
            Timestamp = timestamp;
            Interval = interval;
            Figi = figi;
        }
        
        public decimal Open { get; }
        public decimal Close { get; }
        public decimal High { get; }
        public decimal Low { get; }
        public decimal Volume { get; }
        public DateTime Timestamp { get; }
        public Interval Interval { get; }
        public string Figi { get; }

        public bool Equals(Candle other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Timestamp.Equals(other.Timestamp) &&
                   Interval == other.Interval &&
                   string.Equals(Figi, other.Figi, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Candle) obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Timestamp);
            hashCode.Add((int) Interval);
            hashCode.Add(Figi, StringComparer.InvariantCultureIgnoreCase);
            return hashCode.ToHashCode();
        }

        public int CompareTo(Candle other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Timestamp.CompareTo(other.Timestamp);
        }
    }
}