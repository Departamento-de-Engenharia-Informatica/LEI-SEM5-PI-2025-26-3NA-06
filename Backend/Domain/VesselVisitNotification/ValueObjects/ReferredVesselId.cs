using ProjArqsi.Domain.VesselAggregate;
namespace ProjArqsi.Domain.VesselVisitNotificationAggregate
{
	public class ReferredVesselId
	{
		public IMOnumber VesselId { get; private set; }

		public ReferredVesselId(IMOnumber vesselId)
		{
			VesselId = vesselId;
		}
	}
}
