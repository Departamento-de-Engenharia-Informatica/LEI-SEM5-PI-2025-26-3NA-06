using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselVisitNotificationAggregate
{
    public enum StatusEnum
    {
        InProgress = 0,
        Submitted = 1,
        Accepted = 2,
        Rejected = 3
    }

    public class Status : ValueObject
    {
        public StatusEnum Value { get; private set; }
        public Status(StatusEnum value)
        {
            Value = value;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Status other)
                return Value == other.Value;
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    public static class Statuses
    {
        public static Status InProgress => new Status(StatusEnum.InProgress);
        public static Status Submitted => new Status(StatusEnum.Submitted);
        public static Status Accepted => new Status(StatusEnum.Accepted);
        public static Status Rejected => new Status(StatusEnum.Rejected);
    }
}