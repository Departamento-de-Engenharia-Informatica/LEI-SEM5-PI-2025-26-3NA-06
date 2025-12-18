using ProjArqsi.Domain.ContainerAggregate;

namespace ProjArqsi.Domain.StorageArea.ValueObjects
{
	public class CurrentContainers(List<ContainerId> containers)
    {
        public List<ContainerId> Value { get; private set; } = containers ?? [];
    }
}
