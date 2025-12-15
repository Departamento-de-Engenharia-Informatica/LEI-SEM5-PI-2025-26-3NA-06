using System;
using ProjArqsi.Domain.Shared;
namespace ProjArqsi.Domain.VesselVisitNotification.ValueObjects
{
	public class ArrivalDate : IValueObject
	{
		public DateTime? Value { get; private set; }

		public ArrivalDate(DateTime? date)
		{
			Value = date;
		}
	}
}
