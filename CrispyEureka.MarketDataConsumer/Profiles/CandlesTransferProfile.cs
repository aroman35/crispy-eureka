using System;
using AutoMapper;
using CrispyEureka.Domain.MarketData.Candle;
using CrispyEureka.Transfer.Models;

namespace CrispyEureka.MarketDataConsumer.Profiles
{
    public class CandlesTransferProfile : Profile
    {
        public CandlesTransferProfile()
        {
            CreateMap<CandleTransferModel, Candle>()
                .ConstructUsing(x => new Candle(
                    x.Open,
                    x.Close,
                    x.High,
                    x.Low,
                    x.Volume,
                    new DateTime(x.Timestamp),
                    (Interval) x.Interval,
                    x.Figi));
        }
    }
}