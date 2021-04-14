using System;
using CrispyEureka.Domain;

namespace CrispyEureka.Persistence
{
    public interface IDto<TRoot>
        where TRoot: AggregateRoot
    {
        Guid Id { get; }
        TRoot ToRoot();
    }
}