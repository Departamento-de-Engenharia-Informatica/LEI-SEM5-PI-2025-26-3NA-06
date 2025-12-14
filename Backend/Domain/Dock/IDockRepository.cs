using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.DockAggregate
{
    public interface IDockRepository : IRepository<Dock, DockId>
    {
        Task<Dock?> FindByNameAsync(DockName name);
    }
}
