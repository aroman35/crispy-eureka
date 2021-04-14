using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrispyEureka.Domain.MarketData.OrderBook;
using CrispyEureka.Persistence.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Polly;

namespace CrispyEureka.Persistence.CommandHandlers.AddOrderBook
{
    public class AddOrderBookHandler : IRequestHandler<Application.Commands.MarketData.OrderBook.AddOrderBookCommand>
    {
        private readonly DbContext<OrderBook> _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<AddOrderBookHandler> _logger;

        public AddOrderBookHandler(
            DbContext<OrderBook> dbContext,
            IMapper mapper,
            ILogger<AddOrderBookHandler> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Unit> Handle(Application.Commands.MarketData.OrderBook.AddOrderBookCommand request, CancellationToken cancellationToken)
        {
            var orderBookDtos = _mapper.Map<IEnumerable<OrderBookDto>>(request.OrderBooks);

            var insertResult = await Policy.Handle<Exception>()
                .WaitAndRetryAsync(5, _ => TimeSpan.FromMilliseconds(100))
                .ExecuteAndCaptureAsync(() => _dbContext
                    .GetCollection<OrderBookDto>(request.Figi)
                    .InsertManyAsync(orderBookDtos, null, cancellationToken));

            if (insertResult.Outcome == OutcomeType.Successful)
            {
                _logger.LogInformation("Successfully inserted order-books", request.Figi);
            }
            else
            {
                _logger.LogError(
                    "An error occured while inserting order-books",
                    request.Figi,
                    insertResult.ExceptionType,
                    insertResult.FinalException.Message);
            }
            
            return Unit.Value;
        }
    }
}