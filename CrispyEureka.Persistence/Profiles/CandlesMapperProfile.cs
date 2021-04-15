using AutoMapper;
using CrispyEureka.Domain.MarketData.Candle;
using CrispyEureka.Persistence.Models;

namespace CrispyEureka.Persistence.Profiles
{
    public class CandlesMapperProfile : Profile
    {
        public CandlesMapperProfile()
        {
            CreateMap<Candle, CandleDto>()
                .ForMember(x => x.Timestamp, context => context.MapFrom(x => x.Timestamp.Ticks))
                .ForMember(x => x.Interval, context => context.MapFrom(x => (int) x.Interval));
        }
    }
}