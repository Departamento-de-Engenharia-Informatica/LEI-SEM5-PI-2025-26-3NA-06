using System;
namespace ProjArqsi.Domain.VesselVisitNotification.ValueObjects
{
	public class DepartureDate
	{
		public DateTime? Value { get; private set; }

		public DepartureDate(DateTime? date)
		{
			Value = date;
		}
	}
}
