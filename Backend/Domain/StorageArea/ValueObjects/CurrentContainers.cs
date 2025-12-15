using ProjArqsi.Domain.ContainerAggregate;

namespace ProjArqsi.Domain.StorageArea.ValueObjects
{
	public class CurrentContainers
	{
		public List<IsoCode> Value { get; private set; } = new();

		public CurrentContainers(List<IsoCode> containers)
		{
			Value = containers ?? new List<IsoCode>();
		}
	}
}
