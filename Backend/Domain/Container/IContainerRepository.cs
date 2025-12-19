using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.ContainerAggregate
{
    public interface IContainerRepository : IRepository<Container, ContainerId>
    {
        Task<Container?> GetByIsoCodeAsync(IsoCode isoCode);
        Task<bool> ExistsByIsoCodeAsync(IsoCode isoCode);
    }
}
