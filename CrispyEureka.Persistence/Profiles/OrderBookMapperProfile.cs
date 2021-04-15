using AutoMapper;
using CrispyEureka.Domain.MarketData.OrderBook;
using CrispyEureka.Persistence.Models;

namespace CrispyEureka.Persistence.Profiles
{
    public class OrderBookMapperProfile : Profile
    {
        public OrderBookMapperProfile()
        {
            CreateMap<OrderBook, OrderBookDto>()
                .ForMember(
                    x => x.Timestamp,
                    config => config.MapFrom(x => x.Timestamp))
                .ForMember(
                    x => x.Asks,
                    config => config.MapFrom(x => x.Asks))
                .ForMember(
                    x => x.Bids,
                    config => config.MapFrom(x => x.Bids))
                .ForMember(
                    x => x.Figi,
                    config => config.MapFrom(x => x.Figi));

            CreateMap<OrderBookEntry, OrderBookEntryDto>()
                .ForMember(
                    x => x.Price,
                    context => context.MapFrom(x => x.Price))
                .ForMember(
                    x => x.Quantity,
                    context => context.MapFrom(x => x.Quantity));
        }
    }
}