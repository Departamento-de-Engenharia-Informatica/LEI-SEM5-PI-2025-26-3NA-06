using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselVisitNotificationAggregate
{
    public class RejectionReason : ValueObject
    {
        public string Value { get; private set; }

        public RejectionReason(string value)
        {
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
