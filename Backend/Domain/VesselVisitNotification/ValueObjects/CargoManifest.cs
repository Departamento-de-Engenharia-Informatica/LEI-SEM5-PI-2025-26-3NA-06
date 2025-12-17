using ProjArqsi.Domain.ContainerAggregate;

namespace ProjArqsi.Domain.VesselVisitNotificationAggregate
{
	public class CargoManifest
	{
		public List<IsoCode> Value { get; private set; } = new();

		public CargoManifest(List<IsoCode> containers)
		{
			Value = containers ?? new List<IsoCode>();
		}
	}
}
