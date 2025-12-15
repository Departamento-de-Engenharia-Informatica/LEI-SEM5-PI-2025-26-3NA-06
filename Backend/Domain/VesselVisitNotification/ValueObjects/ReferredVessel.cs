using ProjArqsi.Domain.VesselAggregate;
namespace ProjArqsi.Domain.VesselVisitNotification.ValueObjects
{
	public class ReferredVessel
	{
		public IMOnumber VesselId { get; private set; }

		public ReferredVessel(IMOnumber vesselId)
		{
			VesselId = vesselId;
		}
	}
}
