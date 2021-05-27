namespace CrispyEureka.Domain.Trading
{
    public enum OrderStatus
    {
        New,
        PartiallyFill,
        Fill,
        Cancelled,
        Replaced,
        PendingCancel,
        Rejected,
        PendingReplace,
        PendingNew,
    }
}