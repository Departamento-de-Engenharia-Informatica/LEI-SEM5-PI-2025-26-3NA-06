using System;
using ProjArqsi.Domain.Shared;
namespace ProjArqsi.Domain.VesselVisitNotificationAggregate
{
	public class DepartureDate : ValueObject
	{
		public DateTime? Value { get; private set; }

		public DepartureDate(DateTime? date)
		{
			Value = date;
		}

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value ?? new object();
        }
    }
}
