using System;
using System.Linq;
using CrispyEureka.Common;
using CrispyEureka.Domain.Trading;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace CrispyEureka.Testing.Domain
{
    public class PositionTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public PositionTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact(DisplayName = "Opened long full-filled position. Pnl not realised")]
        public void OpenedLongPositionTest()
        {
            var position = Mocks.EmptyPosition();
            
            var orderNew = Mocks.CreateLongOrder(100.0m, 10);
            position.Add(orderNew);
            
            position.IsOpened.ShouldBeFalse();
            position.TotalLots.ShouldBe(0);
            
            var orderFill = Mocks.CloseFullFill(orderNew);
            position.Add(orderFill);
            
            position.TotalLots.ShouldBe(10);
            position.Average.ShouldBe(100.0m);
            position.Price.ShouldBe(1000.0m);
            position.IsOpened.ShouldBeTrue();
            position.IsLong.ShouldBeTrue();
            
            position.NotRealisedPnl(90.0m).ShouldBe(-100);
            position.NotRealisedPnl(120.0m).ShouldBe(200);
            
            OutputOrdersMap(position);
        }

        [Fact(DisplayName = "Opened long full-filled position with multiple orders. Pnl not realised")]
        public void OpenedMultipleLongPositionsTest()
        {
            var position = Mocks.EmptyPosition();

            var newOrders = new[]
            {
                Mocks.CreateLongOrder(100.0m, 100), // 10 000
                Mocks.CreateLongOrder(110.0m, 80), // 8 800
                Mocks.CreateLongOrder(120.0m, 50) // 6 000
            };
            foreach (var order in newOrders)
                position
                    .Add(order)
                    .Add(Mocks.CloseFullFill(order));

            position.TotalLots.ShouldBe(230);
            position.Average.ShouldNotBeNull();
            Math.Round(position.Average.Value, 6).ShouldBe(107.826087m);
            position.Price.ShouldBe(24_800.0m);
            position.NotRealisedPnl(125.0m).ShouldBe(3950m);
            position.IsOpened.ShouldBeTrue();
            
            OutputOrdersMap(position);
        }
        
        [Fact(DisplayName = "Closed long position with full-filled orders. Pnl realised")]
        public void ClosedMultipleLongPositionsTest()
        {
            var position = Mocks.EmptyPosition();

            var newOrders = new[]
            {
                Mocks.CreateLongOrder(100.0m, 100), // 10 000
                Mocks.CreateLongOrder(110.0m, 80), // 8 800
                Mocks.CreateLongOrder(120.0m, 50) // 6 000
            };
            foreach (var order in newOrders)
                position
                    .Add(order)
                    .Add(Mocks.CloseFullFill(order));

            var closeOrder = Mocks.CreateShortOrder(140.0m, 230);
            position
                .Add(closeOrder)
                .Add(Mocks.CloseFullFill(closeOrder));

            position.IsOpened.ShouldBeFalse();
            position.TotalLots.ShouldBe(0);
            position.Average.ShouldBeNull();
            position.Price.ShouldBeNull();
            position.NotRealisedPnl(140.0m).ShouldBeNull();
            position.RealisedPnl.ShouldBe(7400.0m);
            
            OutputOrdersMap(position);
        }
        
        [Fact(DisplayName = "Opened short full-filled position. Pnl not realised")]
        public void OpenedShortPositionTest()
        {
            var position = Mocks.EmptyPosition();
            
            var orderNew = Mocks.CreateShortOrder(100.0m, 10);
            position.Add(orderNew);
            
            position.IsOpened.ShouldBeFalse();
            position.TotalLots.ShouldBe(0);
            
            var orderFill = Mocks.CloseFullFill(orderNew);
            position.Add(orderFill);
            
            position.TotalLots.ShouldBe(-10);
            position.Average.ShouldBe(100.0m);
            position.Price.ShouldBe(-1000.0m);
            position.IsOpened.ShouldBeTrue();
            position.IsShort.ShouldBeTrue();
            
            position.NotRealisedPnl(90.0m).ShouldBe(100);
            position.NotRealisedPnl(120.0m).ShouldBe(-200);
            
            OutputOrdersMap(position);
        }
        
        [Fact(DisplayName = "Opened short full-filled position with multiple orders. Pnl not realised")]
        public void OpenedMultipleShortPositionsTest()
        {
            var position = Mocks.EmptyPosition();

            var newOrders = new[]
            {
                Mocks.CreateShortOrder(100.0m, 100), // 10 000
                Mocks.CreateShortOrder(110.0m, 80), // 8 800
                Mocks.CreateShortOrder(120.0m, 50) // 6 000
            };
            foreach (var order in newOrders)
                position
                    .Add(order)
                    .Add(Mocks.CloseFullFill(order));

            position.TotalLots.ShouldBe(-230);
            position.Average.ShouldNotBeNull();
            Math.Round(position.Average.Value, 6).ShouldBe(107.826087m);
            position.Price.ShouldBe(-24_800.0m);
            position.IsOpened.ShouldBeTrue();
            position.NotRealisedPnl(80.0m).ShouldBe(6400m);
            
            OutputOrdersMap(position);
        }
        
        [Fact(DisplayName = "Closed short position with full-filled orders. Pnl realised")]
        public void ClosedMultipleShortPositionsTest()
        {
            var position = Mocks.EmptyPosition();

            var newOrders = new[]
            {
                Mocks.CreateShortOrder(100.0m, 100), // 10 000
                Mocks.CreateShortOrder(110.0m, 80), // 8 800
                Mocks.CreateShortOrder(120.0m, 50) // 6 000
            };
            foreach (var order in newOrders)
                position
                    .Add(order)
                    .Add(Mocks.CloseFullFill(order));

            var closeOrder = Mocks.CreateLongOrder(80.0m, 230);
            position
                .Add(closeOrder)
                .Add(Mocks.CloseFullFill(closeOrder));

            position.IsOpened.ShouldBeFalse();
            position.TotalLots.ShouldBe(0);
            position.Average.ShouldBeNull();
            position.Price.ShouldBeNull();
            position.NotRealisedPnl(140.0m).ShouldBeNull();
            position.RealisedPnl.ShouldBe(6400.0m);

            OutputOrdersMap(position);
        }

        [Fact(DisplayName = "Opened long half-filled position. Pnl not realised")]
        public void OpenedMultipleLongHalfFilledPositionsTest()
        {
            var position = Mocks.EmptyPosition();

            var newOrders = new[]
            {
                Mocks.CreateLongOrder(100.0m, 100), // 10 000
                Mocks.CreateLongOrder(110.0m, 80), // 8 800
                Mocks.CreateLongOrder(120.0m, 50) // 6 000
            };
            newOrders.ForEach(order => position.Add(order));
            
            var closingOrders = new[]
            {
                Mocks.ClosePartial(newOrders[0], 75),
                Mocks.ClosePartial(newOrders[0], 25),
                Mocks.ClosePartial(newOrders[1], 79),
                Mocks.ClosePartial(newOrders[1], 1),
                Mocks.ClosePartial(newOrders[2], 10) // 40x120.0m Not filled
            };
            closingOrders.ForEach(order => position.Add(order));

            position.IsOpened.ShouldBeTrue();
            position.TotalLots.ShouldBe(190);
            position.Average.ShouldNotBeNull();
            Math.Round(position.Average.Value, 6).ShouldBe(105.263158m); //105.26315789473684210526315789
            position.Price.ShouldBe(20000.0m);
            position.NotRealisedPnl(140.0m).ShouldBe(6600.0m);
            position.RealisedPnl.ShouldBe(-20000.0m);
            
            OutputOrdersMap(position);
        }

        private void OutputOrdersMap(Position position)
        {
            _testOutputHelper.WriteLine("Orders map:");
            foreach (var positionOrder in position.Orders.Where(x => x.Status is OrderStatus.PartiallyFill or OrderStatus.Fill))
            {
                _testOutputHelper.WriteLine(positionOrder.ToString());
            }
            
            _testOutputHelper.WriteLine($"Realised PnL: {position.RealisedPnl} {position.Currency.ToString()}");
        }
    }
}