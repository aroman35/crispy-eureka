using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrispyEureka.Application.Commands.MarketData;
using CrispyEureka.Application.Commands.MarketData.Candles;
using CrispyEureka.Domain.MarketData.Candle;
using CrispyEureka.Persistence.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Polly;

namespace CrispyEureka.Persistence.CommandHandlers.AddCandles
{
    public class AddCandlesHandler : IRequestHandler<AddMarketData<Candle>>
    {
        private readonly DbContext<Candle> _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<AddCandlesHandler> _logger;

        public AddCandlesHandler(
            DbContext<Candle> dbContext,
            IMapper mapper,
            ILogger<AddCandlesHandler> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Unit> Handle(AddMarketData<Candle> request, CancellationToken cancellationToken)
        {
            var candles = _mapper.Map<IEnumerable<CandleDto>>(request.Messages.OrderBy(x => x)).ToList();
            var minTimestamp = candles.Min(x => x.Timestamp);
            var maxTimestamp = candles.Max(x => x.Timestamp);

            var deleteResult = await _dbContext.GetCollection<CandleDto>(request.Figi)
                .DeleteManyAsync(
                    new ExpressionFilterDefinition<CandleDto>(x =>
                        x.Timestamp >= minTimestamp && x.Timestamp <= maxTimestamp), cancellationToken);
            
            _logger.LogInformation($"Successfully deleted {deleteResult.DeletedCount} old candles");

            var insertResult = await Policy.Handle<Exception>()
                .WaitAndRetryAsync(5, _ => TimeSpan.FromMilliseconds(100))
                .ExecuteAndCaptureAsync(() => _dbContext.GetCollection<CandleDto>(request.Figi)
                    .InsertManyAsync(candles, null, cancellationToken));

            if (insertResult.Outcome == OutcomeType.Successful)
            {
                _logger.LogInformation("Successfully inserted candles", request.Figi);
            }
            else
            {
                _logger.LogError("An error occured while inserting candles",
                    request.Figi,
                    insertResult.ExceptionType,
                    insertResult.FinalException.Message);
            }

            return Unit.Value;
        }
    }
}