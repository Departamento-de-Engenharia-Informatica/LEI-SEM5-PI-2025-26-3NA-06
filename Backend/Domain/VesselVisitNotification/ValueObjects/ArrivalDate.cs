using System;
using ProjArqsi.Domain.Shared;
namespace ProjArqsi.Domain.VesselVisitNotificationAggregate
{
	public class ArrivalDate : ValueObject
	{
		public DateTime? Value { get; private set; }

		public ArrivalDate(DateTime? date)
		{
			Value = date;
		}

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value.HasValue ? (object)Value.Value : "null";
        }
    }
}
