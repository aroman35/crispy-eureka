using System;
using System.Linq;
using AutoMapper;
using CrispyEureka.Domain.MarketData.OrderBook;
using CrispyEureka.Transfer.Models;
using OrderBookEntry = CrispyEureka.Transfer.Models.OrderBookEntry;

namespace CrispyEureka.MarketDataConsumer.Profiles
{
    public class OrderBookTransferProfile : Profile
    {
        public OrderBookTransferProfile()
        {
            CreateMap<OrderBookTransferModel, OrderBook>()
                .ConstructUsing(x => new OrderBook(
                    new DateTime(x.Timestamp),
                    x.Bids.Select(bid => new Domain.MarketData.OrderBook.OrderBookEntry(bid.Price, bid.Quantity)),
                    x.Asks.Select(ask => new Domain.MarketData.OrderBook.OrderBookEntry(ask.Price, ask.Quantity)),
                    x.Figi));

            CreateMap<OrderBookEntry, Domain.MarketData.OrderBook.OrderBookEntry>()
                .ForMember(
                    x => x.Quantity,
                    context => context.MapFrom(x => x.Quantity))
                .ForMember(
                    x => x.Price,
                    context => context.MapFrom(x => x.Price))
                .ConstructUsing(
                    x => new Domain.MarketData.OrderBook.OrderBookEntry(x.Price, x.Quantity));
        }
    }
}