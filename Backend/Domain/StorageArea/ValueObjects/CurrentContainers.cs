using ProjArqsi.Domain.ContainerAggregate;

namespace ProjArqsi.Domain.StorageArea.ValueObjects
{
	public class CurrentContainers(List<IsoCode> containers)
    {
        public List<IsoCode> Value { get; private set; } = containers ?? [];
    }
}
